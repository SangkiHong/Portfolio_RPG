using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using SK.Quests;

namespace SK.UI
{
    /* �ۼ���: ȫ���
     * ����: ����Ʈ Ÿ��Ʋ UI ǥ�ÿ� ���õ� Ŭ����
     * �ۼ���: 22�� 5�� 24��
     */
    public class QuestTitle : PoolObject
    {
        // ����Ʈ ī�װ� �ؽ�Ʈ ������Ʈ
        public Text categoryText;
        // ����Ʈ Ÿ��Ʋ �ؽ�Ʈ ������Ʈ
        public Text titleText;
        // ����Ʈ ���� ��ư
        public Button foldButton;
        // ����Ʈ�� ���� ���� Ŭ���� ����Ʈ
        internal List<QuestTask> tasks;

        // ����Ʈ ���� UI�� ������ �ִ� ���� ���� ����
        private bool _isFolded;
        // ����Ʈ�� �Ҵ�Ǿ� �ִ� ���� ���� ����
        private bool _isAssigned;
        public bool IsAssigned => _isAssigned;

        private readonly string[] _categoryTexts = { "[ ���� ]", "[ �ΰ� ]", "[ ��� ]", "[ �Ϸ� ]" };

        private void Awake()
        {
            // ����Ʈ �ʱ�ȭ
            tasks = new List<QuestTask>();

            // ���� ��ư�� �̺�Ʈ �Լ� ���
            foldButton.onClick.AddListener(delegate { FoldTaskList(); });
        }

        // ����Ʈ ������ ���� UI�� ǥ��_220525
        public void AssignQuest(Quest quest)
        {
            _isAssigned = true;

            gameObject.SetActive(true);

            for (int i = 0; i < quest.TaskGroups.Count; i++)
            {
                // ���� �׷��� Ȱ��ȭ���� �ʾ����� ������ ��������
                if (quest.TaskGroups[i].State == TaskGroupState.Inactive)
                    break;

                // ������Ʈ Ǯ���� ����Ʈ ���� ������Ʈ�� �������鼭 ���� Ŭ���� ����Ʈ�� �߰�
                for (int j = 0; j < quest.TaskGroups[i].Tasks.Count; j++)
                {
                    if (tasks.Count < j + 1)
                    {
                        tasks.Add(UIPoolManager.Instance
                                .GetObject(Strings.PoolName_QuestTask, Vector3.zero, transform)
                                .GetComponent<QuestTask>());
                    }

                    // ����Ʈ ���� �Ҵ�
                    tasks[j].Assign(quest.TaskGroups[i].Tasks[j]);
                }
            }

            // ����Ʈ�� ī�װ��� �ؽ�Ʈ�� ǥ��
            categoryText.text = quest.QuestState != QuestState.Complete ?
                _categoryTexts[(int)quest.Category.questCategory] : _categoryTexts[3];

            // ����Ʈ�� Ÿ��Ʋ�� �ؽ�Ʈ�� ǥ��
            titleText.text = quest.DisplayName;
        }

        // ����Ʈ UI�� ����_220525
        public void Unassign()
        {
            _isAssigned = false;
            _isFolded = false;

            // ���� ����Ʈ�� �� ������ ����
            for (int i = 0; i < tasks.Count; i++)
                if (tasks[i].IsAssigned)
                    tasks[i].Unassign();

            // ���� ����Ʈ Ÿ��Ʋ ������Ʈ�� ��
            gameObject.SetActive(false);
        }

        // ����Ʈ ���� UI�� ���� ��� �Լ�
        private void FoldTaskList()
        {
            // �������� ���� ���
            if (!_isFolded)
            {
                // ǥ�õ� ��� ���� UI�� ��
                foreach (var task in tasks)                
                    task.gameObject.SetActive(false);                
            }
            // �����ִ� ���
            else
            {
                foreach (var task in tasks)
                {
                    if (task._assignedTask.State != TaskState.Inactive)
                        task.gameObject.SetActive(true);
                }
            }

            // ���� ���θ� ����
            _isFolded = !_isFolded;
        }
    }
}
