using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SK.UI;

namespace SK.Quests
{
    /* �ۼ���: ȫ���
     * ����: ����Ʈ UI�� �����Ϳ� ���� �������� ����� ������ Ŭ����
     * �ۼ���: 22�� 5�� 24��
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

        // ����Ʈ Ÿ��Ʋ ������Ʈ�� ������ ����Ʈ
        private List<QuestTitle> _questTitles;
        // ���� ������ �������� �ӽ÷� ������ ���� ����Ʈ
        private List<Item> _tempRewardItemList;

        private Vector3 _focusLocalPos;

        private void Awake()
        {
            // Ȱ��ȭ �� ����Ʈ ����Ʈ �ʱ�ȭ
            activeQuestsList = new List<Quest>();
            _questTitles = new List<QuestTitle>();
            _tempRewardItemList = new List<Item>();

            // Tab ��ư �̺�Ʈ �Ҵ�_220519
            for (int i = 0; i < tabButtons.Length; i++)
            {
                // Closuer problem ���� ���� �ӽ� �ε��� ���� ����
                int tempIndex = i; 
                // �� �ε����� ���� ������ ������
                tabButtons[i].onClick.AddListener(delegate { TabButton(tempIndex); });
            }
            
            // �� ��Ŀ�� UI�� ���� ��ġ���� ������ ����
            _focusLocalPos = tabFocus.localPosition;
        }

        // �÷��̾� �����Ϳ� ���� ����Ʈ ��� �ʱ�ȭ_220519
        public void Initialize()
        {
            // ������ ������ Ŭ�����z ���� ����Ʈ�� ����Ʈ �Ҵ�
            if (!GameManager.Instance.DataManager.LoadQuestData(activeQuestsList, completedQuestsList))
                return; // ������ �������� ������ ��� ��ȯ

            // ����Ʈ ����Ʈ�� ���� UI ����
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

                // ������ ���� ���� â �ݱ�
                if (uiManager.inventoryManager.itemSpecificsPanel.IsOpen)
                    uiManager.inventoryManager.itemSpecificsPanel.Close();
                return true;
            }
            else
                return false;
        }

        // ����Ʈ�� ����Ʈ�� �߰�
        public void AddQuestUIList(Quest newQuest, bool isNew)
        {
            // ����Ʈ �ʱ�ȭ
            newQuest.OnRegister(isNew);

            // ������Ʈ Ǯ���� ����Ʈ Ÿ��Ʋ ������Ʈ�� �ҷ��� �Ŀ� ����Ʈ Ÿ��Ʋ ������Ʈ�� ����Ʈ�� �߰�
            _questTitles.Add(UIPoolManager.Instance
                        .GetObject(Strings.PoolName_QuestTitle, Vector3.zero, contents).GetComponent<QuestTitle>());

            int lastIndex = _questTitles.Count - 1;

            // Ȱ��ȭ �� ����Ʈ ����Ʈ�� ����ƮŸ��Ʋ ������Ʈ �߰�
            _questTitles[lastIndex].AssignQuest(newQuest);
            // ����Ʈ �� Ŭ�� �� ������ �̺�Ʈ �Լ� ���
            _questTitles[lastIndex].OnClickQuest += OpenQuestInfo;

            // �̴� ����Ʈ ���� UI ǥ��
            miniInfoManager.AddMiniInfo(newQuest);
        }
        #endregion

        #region Quest Data
        // ����Ʈ�� ���� �޾� Ȱ��ȭ ����Ʈ ����Ʈ�� �߰�_220614
        public void AddNewQuest()
        {
            if (SelectedQuest != null)
            {
                // ����Ʈ ���� �ʱ�ȭ
                SelectedQuest.Initialize();

                AddQuestUIList(SelectedQuest, true);
                activeQuestsList.Add(SelectedQuest);
            }
        }

        // ����Ʈ �ϼ� Ƚ���� ���� �޾� Ȱ��ȭ�� ����Ʈ�� ����(Ÿ��, ����)_220618
        public void ReportSuccessCount(object target, int amount)
        {
            Debug.Log("ReportSuccessCount");
            if (activeQuestsList.Count > 0)
                for (int i = 0; i < activeQuestsList.Count; i++)
                    activeQuestsList[i].ReceiveReport(target, amount);
        }

        // ����Ʈ �Ϸ�
        public bool CompleteQuest(Quest completedQuest)
        {
            // �ӽ� ���� ������ ����Ʈ �ʱ�ȭ
            _tempRewardItemList.Clear();

            // ���� ������ ������ ��Ȳ���� Ȯ�� �� ���� ������ ������ ���� ����Ʈ�� ����
            if (UIManager.Instance.inventoryManager.CanTakeRewardItems(completedQuest.Reward, _tempRewardItemList))
            {
                // ���� ���� ������ ��� ���� ������ ����
                Data.DataManager.Instance.GrantReward(completedQuest.Reward, _tempRewardItemList);
            }
            else // ���� ���� �Ұ��� ��Ȳ�� ��� false ����
                return false;

            //  ����ƮŸ��Ʋ���� �Ҵ� ����
            for (int i = 0; i < _questTitles.Count; i++)
            {
                if (_questTitles[i].AssignedQuest == completedQuest)
                {
                    _questTitles[i].OnClickQuest = null;
                    _questTitles[i].Unassign();
                    break;
                }
            }

            // ����Ʈ �Ϸ� ���·� ��ȯ
            completedQuest.Complete();

            // Ȱ��ȭ ����Ʈ ����Ʈ���� ����
            activeQuestsList.Remove(completedQuest);
            // �Ϸ�� ����Ʈ ����Ʈ�� �߰�
            completedQuestsList.Add(completedQuest);

            // �̴� ����Ʈ UI ��Ͽ��� ����
            miniInfoManager.RemoveMiniInfo(completedQuest);

            // ���� ���� ������ ��� ���� ���� �� ����Ʈ�� �Ϸ� ���·� ��ȯ�ϰ� true ����
            return true;
        }

        // ����Ʈ�� ���� ���� ���θ� Ȯ���Ͽ� �οﰪ ��ȯ�ϴ� �Լ�_220613
        public bool IsAcceptable(Quest quest)
        {
            // ����Ʈ�� �̹� �Ϸ�� ����Ʈ ����Ʈ�� �ִ� ���
            foreach (var _quest in completedQuestsList)
                if (quest == _quest) return false;

            // ����Ʈ�� ���� ������ ��ȿ�� ���
            if (quest.IsAcceptable)
            {
                SelectedQuest = quest;
                return true; 
            }

            return false;
        }

        // �ش� ����Ʈ�� Ȱ��ȭ �� ����Ʈ ����Ʈ�� ���ԵǾ��� ���� ���� ���θ� ��ȯ�ϴ� �Լ�_220613
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
        // ī�װ� ���� ����Ʈ ����(�� �ε���)_220519
        private void TabButton(int tabIndex)
        {
            // ����Ʈ ������ �Լ� ȣ��
            LoadQuestListByCategory(tabIndex);

            // ���õ� �� ��ư ��Ȱ��ȭ
            tabButtons[tabIndex].interactable = false;

            // ���õ� ���� ������ ��� ���� Ȱ��ȭ
            for (int i = 0; i < tabButtons.Length; i++)
                if (i != tabIndex) 
                    tabButtons[i].interactable = true;

            // ��Ŀ�� �̹����� ���õ� �ǿ� �Ʒ� ���� ��
            _focusLocalPos.x = tabButtons[tabIndex].transform.localPosition.x;
            tabFocus.localPosition = _focusLocalPos;

        }

        // ī�װ��� ���� ����Ʈ ��� ǥ��_220525
        private void LoadQuestListByCategory(int tabIndex)
        {
            // �Ҵ�� ����Ʈ ��� ����
            foreach (var questTitle in _questTitles)
                if (questTitle.IsAssigned) 
                    questTitle.Unassign();

            switch (tabIndex)
            {
                case 0: // ���� ���� ���� ����Ʈ
                    for (int i = 0; i < activeQuestsList.Count; i++)
                        _questTitles[i].AssignQuest(activeQuestsList[i]);
                    break;
                case 1: // ���� ����Ʈ
                    for (int i = 0; i < activeQuestsList.Count; i++)
                        if (activeQuestsList[i].Category.questCategory == Category.MainQuest)
                            _questTitles[i].AssignQuest(activeQuestsList[i]);
                    break;
                case 2: // �ΰ� ����Ʈ
                    for (int i = 0; i < activeQuestsList.Count; i++)
                        if (activeQuestsList[i].Category.questCategory == Category.SubQuest)
                            _questTitles[i].AssignQuest(activeQuestsList[i]);
                    break;
                case 3: // ��� ����Ʈ
                    for (int i = 0; i < activeQuestsList.Count; i++)
                        if (activeQuestsList[i].Category.questCategory == Category.GuildQuest)
                            _questTitles[i].AssignQuest(activeQuestsList[i]);
                    break;
                case 4: // �Ϸ�� ����Ʈ
                    for (int i = 0; i < completedQuestsList.Count; i++)
                    { 
                        // ����Ʈ ��� ������Ʈ�� ������ ��� ���� ���� �� �Ҵ�
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
        // ����Ʈ ��ư�� Ŭ�� �� �ش� ����Ʈ ���� â ǥ��_220613
        public void OpenQuestInfo()
            => questInfo.DisplayQuestInfo(SelectedQuest);

        // ����Ʈ �ٸ� Ŭ���Ͽ� �̺�Ʈ �߻� �� ������ ����Ʈ ���� â ǥ�� �Լ�_220610
        public void OpenQuestInfo(Quest quest)
            => questInfo.DisplayQuestInfo(quest);

        // ����Ʈ �Ϸ�� ���� ���� �г��� ǥ��_220716
        public void OpenRewardInfo(Quest quest)
            => rewardPanel.AssignReward(quest);
        #endregion
    }
}
