using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SK.Quests;

namespace SK.UI
{
    public class QuestManager : MonoBehaviour
    {
        [Header("Reference")]
        [SerializeField] private UIManager uiManager;

        [Header("Quest")]
        [SerializeField] internal List<Quest> activeQuestsList;
        [SerializeField] internal List<Quest> completedQuestsList;

        [Header("Contents")]
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private Transform contents;
        [Header("Tab")]
        [SerializeField] private Transform tabFocus;
        [SerializeField] private Button[] tabButtons;

        private List<QuestTitle> QuestTitles;
        private QuestTask tmpQuestTask;

        private Vector3 _focusLocalPos;

        private void Awake()
        {
            // 활성화 된 퀘스트 리스트 초기화
            activeQuestsList = new List<Quest>();
            QuestTitles = new List<QuestTitle>();

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

            // 퀘스트 타이틀 리스트의 인덱스 값
            int questIndex = 0;

            // 퀘스트 리스트를 토대로 UI 생성
            if (activeQuestsList.Count > 0)
            {
                for (int i = 0; i < activeQuestsList.Count; i++)
                {
                    activeQuestsList[i].OnRegister();

                    // 오브젝트 풀에서 퀘스트 타이틀 오브젝트를 불러온 후에 퀘스트 타이틀 컴포넌트를 리스트에 추가
                    QuestTitles.Add(UIPoolManager.Instance
                        .GetObject(Strings.PoolName_QuestTitle, Vector3.zero, contents).GetComponent<QuestTitle>());

                    QuestTitles[questIndex++].AssignQuest(activeQuestsList[i]);
                }
            }
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
            foreach (var questTitle in QuestTitles)
                if (questTitle.IsAssigned) 
                    questTitle.Unassign();

            switch (tabIndex)
            {
                case 0: // 현재 진행 중인 퀘스트
                    for (int i = 0; i < activeQuestsList.Count; i++)
                        QuestTitles[i].AssignQuest(activeQuestsList[i]);
                    break;
                case 1: // 메인 퀘스트
                    for (int i = 0; i < activeQuestsList.Count; i++)
                        if (activeQuestsList[i].Category.questCategory == Category.MainQuest)
                            QuestTitles[i].AssignQuest(activeQuestsList[i]);
                    break;
                case 2: // 부가 퀘스트
                    for (int i = 0; i < activeQuestsList.Count; i++)
                        if (activeQuestsList[i].Category.questCategory == Category.SubQuest)
                            QuestTitles[i].AssignQuest(activeQuestsList[i]);
                    break;
                case 3: // 길드 퀘스트
                    for (int i = 0; i < activeQuestsList.Count; i++)
                        if (activeQuestsList[i].Category.questCategory == Category.GuildQuest)
                            QuestTitles[i].AssignQuest(activeQuestsList[i]);
                    break;
                case 4: // 완료된 퀘스트
                    for (int i = 0; i < completedQuestsList.Count; i++)
                        QuestTitles[i].AssignQuest(completedQuestsList[i]);
                    break;
            }
        }
    }
}
