using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SK.Quests;

namespace SK.UI
{
    /* �ۼ���: ȫ���
     * ����: ����Ʈ UI�� �����Ϳ� ���� �������� ����� ������ Ŭ����
     * �ۼ���: 22�� 5�� 24��
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
            // Ȱ��ȭ �� ����Ʈ ����Ʈ �ʱ�ȭ
            activeQuestsList = new List<Quest>();
            questTitles = new List<QuestTitle>();

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
            GameManager.Instance.DataManager.LoadQuestData(ref activeQuestsList, ref completedQuestsList);

            // ����Ʈ ����Ʈ�� ���� UI ����
            if (activeQuestsList.Count > 0)
            {
                for (int i = 0; i < activeQuestsList.Count; i++)
                    AddQuest(activeQuestsList[i]);
            }
        }

        // ����Ʈ�� ����Ʈ�� �߰�
        public void AddQuest(Quest newQuest)
        {
            // ����Ʈ �ʱ�ȭ
            newQuest.OnRegister();

            // ������Ʈ Ǯ���� ����Ʈ Ÿ��Ʋ ������Ʈ�� �ҷ��� �Ŀ� ����Ʈ Ÿ��Ʋ ������Ʈ�� ����Ʈ�� �߰�
            questTitles.Add(UIPoolManager.Instance
                        .GetObject(Strings.PoolName_QuestTitle, Vector3.zero, contents).GetComponent<QuestTitle>());

            int lastIndex = questTitles.Count - 1;

            // Ȱ��ȭ �� ����Ʈ ����Ʈ�� ����ƮŸ��Ʋ ������Ʈ �߰�
            questTitles[lastIndex].AssignQuest(newQuest);
            // ����Ʈ �� Ŭ�� �� ������ �̺�Ʈ �Լ� ���
            questTitles[lastIndex].OnClickQuest += OpenQuestInfo;
            // ����Ʈ �Ϸ� �� ������ �̺�Ʈ �Լ� ���
            newQuest.onCompleted += CompleteQuest;
        }

        // ����Ʈ �Ϸ� �� �Ҵ�� ����ƮŸ��Ʋ �Ҵ� ����
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
            foreach (var questTitle in questTitles)
                if (questTitle.IsAssigned) 
                    questTitle.Unassign();

            switch (tabIndex)
            {
                case 0: // ���� ���� ���� ����Ʈ
                    for (int i = 0; i < activeQuestsList.Count; i++)
                        questTitles[i].AssignQuest(activeQuestsList[i]);
                    break;
                case 1: // ���� ����Ʈ
                    for (int i = 0; i < activeQuestsList.Count; i++)
                        if (activeQuestsList[i].Category.questCategory == Category.MainQuest)
                            questTitles[i].AssignQuest(activeQuestsList[i]);
                    break;
                case 2: // �ΰ� ����Ʈ
                    for (int i = 0; i < activeQuestsList.Count; i++)
                        if (activeQuestsList[i].Category.questCategory == Category.SubQuest)
                            questTitles[i].AssignQuest(activeQuestsList[i]);
                    break;
                case 3: // ��� ����Ʈ
                    for (int i = 0; i < activeQuestsList.Count; i++)
                        if (activeQuestsList[i].Category.questCategory == Category.GuildQuest)
                            questTitles[i].AssignQuest(activeQuestsList[i]);
                    break;
                case 4: // �Ϸ�� ����Ʈ
                    for (int i = 0; i < completedQuestsList.Count; i++)
                        questTitles[i].AssignQuest(completedQuestsList[i]);
                    break;
            }
        }

        // ����Ʈ ��ư�� Ŭ�� �� �ش� ����Ʈ ���� â ǥ��_220613
        public void OpenQuestInfo()
            => questInfo.DisplayQuestInfo(SelectedQuest);

        // ����Ʈ �ٸ� Ŭ���Ͽ� �̺�Ʈ �߻� �� ������ ����Ʈ ���� â ǥ�� �Լ�_220610
        public void OpenQuestInfo(Quest quest)
            => questInfo.DisplayQuestInfo(quest);
    }
}
