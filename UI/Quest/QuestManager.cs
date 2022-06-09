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
            // Ȱ��ȭ �� ����Ʈ ����Ʈ �ʱ�ȭ
            activeQuestsList = new List<Quest>();
            QuestTitles = new List<QuestTitle>();

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

            // ����Ʈ Ÿ��Ʋ ����Ʈ�� �ε��� ��
            int questIndex = 0;

            // ����Ʈ ����Ʈ�� ���� UI ����
            if (activeQuestsList.Count > 0)
            {
                for (int i = 0; i < activeQuestsList.Count; i++)
                {
                    activeQuestsList[i].OnRegister();

                    // ������Ʈ Ǯ���� ����Ʈ Ÿ��Ʋ ������Ʈ�� �ҷ��� �Ŀ� ����Ʈ Ÿ��Ʋ ������Ʈ�� ����Ʈ�� �߰�
                    QuestTitles.Add(UIPoolManager.Instance
                        .GetObject(Strings.PoolName_QuestTitle, Vector3.zero, contents).GetComponent<QuestTitle>());

                    QuestTitles[questIndex++].AssignQuest(activeQuestsList[i]);
                }
            }
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
            foreach (var questTitle in QuestTitles)
                if (questTitle.IsAssigned) 
                    questTitle.Unassign();

            switch (tabIndex)
            {
                case 0: // ���� ���� ���� ����Ʈ
                    for (int i = 0; i < activeQuestsList.Count; i++)
                        QuestTitles[i].AssignQuest(activeQuestsList[i]);
                    break;
                case 1: // ���� ����Ʈ
                    for (int i = 0; i < activeQuestsList.Count; i++)
                        if (activeQuestsList[i].Category.questCategory == Category.MainQuest)
                            QuestTitles[i].AssignQuest(activeQuestsList[i]);
                    break;
                case 2: // �ΰ� ����Ʈ
                    for (int i = 0; i < activeQuestsList.Count; i++)
                        if (activeQuestsList[i].Category.questCategory == Category.SubQuest)
                            QuestTitles[i].AssignQuest(activeQuestsList[i]);
                    break;
                case 3: // ��� ����Ʈ
                    for (int i = 0; i < activeQuestsList.Count; i++)
                        if (activeQuestsList[i].Category.questCategory == Category.GuildQuest)
                            QuestTitles[i].AssignQuest(activeQuestsList[i]);
                    break;
                case 4: // �Ϸ�� ����Ʈ
                    for (int i = 0; i < completedQuestsList.Count; i++)
                        QuestTitles[i].AssignQuest(completedQuestsList[i]);
                    break;
            }
        }
    }
}
