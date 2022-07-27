using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SK.UI;

namespace SK.Quests
{
    /* 작성자: 홍상기
     * 내용: 퀘스트 UI와 데이터에 대한 전반적인 기능의 관리자 클래스
     * 작성일: 22년 5월 24일
     */

    public class QuestManager : MonoBehaviour
    {
        [Header("Reference")]
        [SerializeField] private UIManager uiManager;
        [SerializeField] private QuestMiniInfoManager miniInfoManager;
        [SerializeField] private QuestInfo questInfo;
        [SerializeField] private RewardPanel rewardPanel;

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

        // 퀘스트 타이틀 컴포넌트를 저장할 리스트
        private List<QuestTitle> _questTitles;
        // 보상 아이템 정보들을 임시로 저장할 버퍼 리스트
        private List<Item> _tempRewardItemList;

        private Vector3 _focusLocalPos;

        private void Awake()
        {
            // 활성화 된 퀘스트 리스트 초기화
            activeQuestsList = new List<Quest>();
            _questTitles = new List<QuestTitle>();
            _tempRewardItemList = new List<Item>();

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
            if (!GameManager.Instance.DataManager.LoadQuestData(activeQuestsList, completedQuestsList))
                return; // 파일이 존재하지 않으면 즉시 반환

            // 퀘스트 리스트를 토대로 UI 생성
            if (activeQuestsList.Count > 0)
            {
                for (int i = 0; i < activeQuestsList.Count; i++)
                    AddQuestUIList(activeQuestsList[i], false);
            }
        }

        #region Quest UI
        public bool ClosePanel()
        {
            if (questInfo.gameObject.activeSelf)
            {
                questInfo.gameObject.SetActive(false);

                // 아이템 세부 정보 창 닫기
                if (uiManager.inventoryManager.itemSpecificsPanel.IsOpen)
                    uiManager.inventoryManager.itemSpecificsPanel.Close();
                return true;
            }
            else
                return false;
        }

        // 퀘스트를 리스트에 추가
        public void AddQuestUIList(Quest newQuest, bool isNew)
        {
            // 퀘스트 초기화
            newQuest.OnRegister(isNew);

            // 오브젝트 풀에서 퀘스트 타이틀 오브젝트를 불러온 후에 퀘스트 타이틀 컴포넌트를 리스트에 추가
            _questTitles.Add(UIPoolManager.Instance
                        .GetObject(Strings.PoolName_QuestTitle, Vector3.zero, contents).GetComponent<QuestTitle>());

            int lastIndex = _questTitles.Count - 1;

            // 활성화 된 퀘스트 리스트에 퀘스트타이틀 컴포넌트 추가
            _questTitles[lastIndex].AssignQuest(newQuest);
            // 퀘스트 바 클릭 시 실행할 이벤트 함수 등록
            _questTitles[lastIndex].OnClickQuest += OpenQuestInfo;

            // 미니 퀘스트 정보 UI 표시
            miniInfoManager.AddMiniInfo(newQuest);
        }
        #endregion

        #region Quest Data
        // 퀘스트를 새로 받아 활성화 퀘스트 리스트에 추가_220614
        public void AddNewQuest()
        {
            if (SelectedQuest != null)
            {
                // 퀘스트 정보 초기화
                SelectedQuest.Initialize();

                AddQuestUIList(SelectedQuest, true);
                activeQuestsList.Add(SelectedQuest);
            }
        }

        // 퀘스트 완수 횟수를 보고 받아 활성화된 퀘스트에 전달(타겟, 수량)_220618
        public void ReportSuccessCount(object target, int amount)
        {
            Debug.Log("ReportSuccessCount");
            if (activeQuestsList.Count > 0)
                for (int i = 0; i < activeQuestsList.Count; i++)
                    activeQuestsList[i].ReceiveReport(target, amount);
        }

        // 퀘스트 완료
        public bool CompleteQuest(Quest completedQuest)
        {
            // 임시 보상 아이템 리스트 초기화
            _tempRewardItemList.Clear();

            // 보상 수락이 가능한 상황인지 확인 및 보상 아이템 정보를 버퍼 리스트에 저장
            if (UIManager.Instance.inventoryManager.CanTakeRewardItems(completedQuest.Reward, _tempRewardItemList))
            {
                // 보상 수락 가능한 경우 보상 아이템 지급
                Data.DataManager.Instance.GrantReward(completedQuest.Reward, _tempRewardItemList);
            }
            else // 보상 수락 불가한 상황인 경우 false 리턴
                return false;

            //  퀘스트타이틀에서 할당 해제
            for (int i = 0; i < _questTitles.Count; i++)
            {
                if (_questTitles[i].AssignedQuest == completedQuest)
                {
                    _questTitles[i].OnClickQuest = null;
                    _questTitles[i].Unassign();
                    break;
                }
            }

            // 퀘스트 완료 상태로 전환
            completedQuest.Complete();

            // 활성화 퀘스트 리스트에서 제거
            activeQuestsList.Remove(completedQuest);
            // 완료된 퀘스트 리스트에 추가
            completedQuestsList.Add(completedQuest);

            // 미니 퀘스트 UI 목록에서 제거
            miniInfoManager.RemoveMiniInfo(completedQuest);

            // 보상 수락 가능한 경우 보상 지급 후 퀘스트를 완료 상태로 전환하고 true 리턴
            return true;
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

        // 해당 퀘스트가 활성화 된 퀘스트 리스트에 포함되었는 지에 대한 여부를 반환하는 함수_220613
        public bool IsActivated(Quest quest)
        {
            foreach (var _quest in activeQuestsList)
            {
                if (quest == _quest)
                {
                    SelectedQuest = _quest;
                    return true; 
                }
            }

            return false;
        }
        #endregion

        #region Quest Category Tab
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
            foreach (var questTitle in _questTitles)
                if (questTitle.IsAssigned) 
                    questTitle.Unassign();

            switch (tabIndex)
            {
                case 0: // 현재 진행 중인 퀘스트
                    for (int i = 0; i < activeQuestsList.Count; i++)
                        _questTitles[i].AssignQuest(activeQuestsList[i]);
                    break;
                case 1: // 메인 퀘스트
                    for (int i = 0; i < activeQuestsList.Count; i++)
                        if (activeQuestsList[i].Category.questCategory == Category.MainQuest)
                            _questTitles[i].AssignQuest(activeQuestsList[i]);
                    break;
                case 2: // 부가 퀘스트
                    for (int i = 0; i < activeQuestsList.Count; i++)
                        if (activeQuestsList[i].Category.questCategory == Category.SubQuest)
                            _questTitles[i].AssignQuest(activeQuestsList[i]);
                    break;
                case 3: // 길드 퀘스트
                    for (int i = 0; i < activeQuestsList.Count; i++)
                        if (activeQuestsList[i].Category.questCategory == Category.GuildQuest)
                            _questTitles[i].AssignQuest(activeQuestsList[i]);
                    break;
                case 4: // 완료된 퀘스트
                    for (int i = 0; i < completedQuestsList.Count; i++)
                    { 
                        // 퀘스트 목록 컴포넌트가 부족한 경우 새로 생성 후 할당
                        if (_questTitles.Count < completedQuestsList.Count)
                            _questTitles.Add(UIPoolManager.Instance
                                        .GetObject(Strings.PoolName_QuestTitle, Vector3.zero, contents).GetComponent<QuestTitle>());

                        _questTitles[i].AssignQuest(completedQuestsList[i]);
                    }
                    break;
            }
        }
        #endregion

        #region Quest Info UI
        // 퀘스트 버튼을 클릭 시 해당 퀘스트 정보 창 표시_220613
        public void OpenQuestInfo()
            => questInfo.DisplayQuestInfo(SelectedQuest);

        // 퀘스트 바를 클릭하여 이벤트 발생 시 수행할 퀘스트 정보 창 표시 함수_220610
        public void OpenQuestInfo(Quest quest)
            => questInfo.DisplayQuestInfo(quest);

        // 퀘스트 완료시 보상 정보 패널을 표시_220716
        public void OpenRewardInfo(Quest quest)
            => rewardPanel.AssignReward(quest);
        #endregion
    }
}
