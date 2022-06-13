using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SK.Quests;

namespace SK.UI
{
    /* 작성자: 홍상기
     * 내용: 퀘스트 UI와 데이터에 대한 전반적인 기능의 관리자 클래스
     * 작성일: 22년 5월 24일
     */
    public class QuestManager : MonoBehaviour
    {
        [Header("Reference")]
        [SerializeField] private UIManager uiManager;
        [SerializeField] private QuestInfo questInfo;

        [Header("Quest")]
        [SerializeField] internal List<Quest> activeQuestsList;
        [SerializeField] internal List<Quest> completedQuestsList;

        [Header("Contents")]
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private Transform contents;

        [Header("Tab")]
        [SerializeField] private Transform tabFocus;
        [SerializeField] private Button[] tabButtons;
        
        public Quest SelectedQuest { get; private set; }

        private List<QuestTitle> questTitles;

        private Vector3 _focusLocalPos;

        private void Awake()
        {
            // 활성화 된 퀘스트 리스트 초기화
            activeQuestsList = new List<Quest>();
            questTitles = new List<QuestTitle>();

            // Tab 버튼 이벤트 할당_220519
            for (int i = 0; i < tabButtons.Length; i++)
            {
                // Closuer problem 으로 인해 임시 인덱스 값을 생성
                int tempIndex = i; 
                // 탭 인덱스에 따른 아이템 리스팅
                tabButtons[i].onClick.AddListener(delegate { TabButton(tempIndex); });
            }
            
            // 탭 포커스 UI의 로컬 위치값을 변수에 저장
            _focusLocalPos = tabFocus.localPosition;
        }

        // 플레이어 데이터에 따른 퀘스트 목록 초기화_220519
        public void Initialize()
        {
            // 데이터 매지저 클래스틑 통해 리스트에 퀘스트 할당
            GameManager.Instance.DataManager.LoadQuestData(ref activeQuestsList, ref completedQuestsList);

            // 퀘스트 리스트를 토대로 UI 생성
            if (activeQuestsList.Count > 0)
            {
                for (int i = 0; i < activeQuestsList.Count; i++)
                    AddQuest(activeQuestsList[i]);
            }
        }

        // 퀘스트를 리스트에 추가
        public void AddQuest(Quest newQuest)
        {
            // 퀘스트 초기화
            newQuest.OnRegister();

            // 오브젝트 풀에서 퀘스트 타이틀 오브젝트를 불러온 후에 퀘스트 타이틀 컴포넌트를 리스트에 추가
            questTitles.Add(UIPoolManager.Instance
                        .GetObject(Strings.PoolName_QuestTitle, Vector3.zero, contents).GetComponent<QuestTitle>());

            int lastIndex = questTitles.Count - 1;

            // 활성화 된 퀘스트 리스트에 퀘스트타이틀 컴포넌트 추가
            questTitles[lastIndex].AssignQuest(newQuest);
            // 퀘스트 바 클릭 시 실행할 이벤트 함수 등록
            questTitles[lastIndex].OnClickQuest += OpenQuestInfo;
            // 퀘스트 완료 시 실행할 이벤트 함수 등록
            newQuest.onCompleted += CompleteQuest;
        }

        // 퀘스트 완료 시 할당된 퀘스트타이틀 할당 해제
        public void CompleteQuest(Quest completedQuest)
        {
            for (int i = 0; i < questTitles.Count; i++)
            {
                if (questTitles[i].AssignedQuest == completedQuest)
                {
                    questTitles[i].OnClickQuest = null;
                    questTitles[i].Unassign();
                    return;
                }
            }
        }

        // 퀘스트의 수락 가능 여부를 확인하여 부울값 반환하는 함수_220613
        public bool IsAcceptable(Quest quest)
        {
            // 퀘스트가 이미 완료된 퀘스트 리스트에 있는 경우
            foreach (var _quest in completedQuestsList)
                if (quest == _quest) return false;

            // 퀘스트의 수락 조건이 유효한 경우
            if (quest.IsAcceptable)
            {
                SelectedQuest = quest;
                return true; 
            }

            return false;
        }

        // 카테고리 별로 퀘스트 정렬(탭 인덱스)_220519
        private void TabButton(int tabIndex)
        {
            // 퀘스트 리스팅 함수 호출
            LoadQuestListByCategory(tabIndex);

            // 선택된 탭 버튼 비활성화
            tabButtons[tabIndex].interactable = false;

            // 선택된 탭을 제외한 모든 탭의 활성화
            for (int i = 0; i < tabButtons.Length; i++)
                if (i != tabIndex) 
                    tabButtons[i].interactable = true;

            // 포커싱 이미지가 선택된 탭에 아래 오게 함
            _focusLocalPos.x = tabButtons[tabIndex].transform.localPosition.x;
            tabFocus.localPosition = _focusLocalPos;

        }

        // 카테고리에 따른 퀘스트 목록 표시_220525
        private void LoadQuestListByCategory(int tabIndex)
        {
            // 할당된 리스트 모두 해제
            foreach (var questTitle in questTitles)
                if (questTitle.IsAssigned) 
                    questTitle.Unassign();

            switch (tabIndex)
            {
                case 0: // 현재 진행 중인 퀘스트
                    for (int i = 0; i < activeQuestsList.Count; i++)
                        questTitles[i].AssignQuest(activeQuestsList[i]);
                    break;
                case 1: // 메인 퀘스트
                    for (int i = 0; i < activeQuestsList.Count; i++)
                        if (activeQuestsList[i].Category.questCategory == Category.MainQuest)
                            questTitles[i].AssignQuest(activeQuestsList[i]);
                    break;
                case 2: // 부가 퀘스트
                    for (int i = 0; i < activeQuestsList.Count; i++)
                        if (activeQuestsList[i].Category.questCategory == Category.SubQuest)
                            questTitles[i].AssignQuest(activeQuestsList[i]);
                    break;
                case 3: // 길드 퀘스트
                    for (int i = 0; i < activeQuestsList.Count; i++)
                        if (activeQuestsList[i].Category.questCategory == Category.GuildQuest)
                            questTitles[i].AssignQuest(activeQuestsList[i]);
                    break;
                case 4: // 완료된 퀘스트
                    for (int i = 0; i < completedQuestsList.Count; i++)
                        questTitles[i].AssignQuest(completedQuestsList[i]);
                    break;
            }
        }

        // 퀘스트 버튼을 클릭 시 해당 퀘스트 정보 창 표시_220613
        public void OpenQuestInfo()
            => questInfo.DisplayQuestInfo(SelectedQuest);

        // 퀘스트 바를 클릭하여 이벤트 발생 시 수행할 퀘스트 정보 창 표시 함수_220610
        public void OpenQuestInfo(Quest quest)
            => questInfo.DisplayQuestInfo(quest);
    }
}
