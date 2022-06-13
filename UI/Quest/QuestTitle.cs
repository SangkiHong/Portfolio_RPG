using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using SK.Quests;

namespace SK.UI
{
    /* �ۼ���: ȫ���
     * ����: ����Ʈ Ÿ��Ʋ UI ǥ�ÿ� ���õ� Ŭ����
     * �ۼ���: 22�� 5�� 24��
     */
    public class QuestTitle : PoolObject, IPointerDownHandler
    {
        // ����Ʈ �ٸ� Ŭ�� �� ������ �̺�Ʈ
        public UnityAction<Quest> OnClickQuest;
        // ����Ʈ ī�װ� �ؽ�Ʈ ������Ʈ
        public Text categoryText;
        // ����Ʈ Ÿ��Ʋ �ؽ�Ʈ ������Ʈ
        public Text titleText;
        // ����Ʈ ���� ��ư
        public Button foldButton;
        // ����Ʈ�� ���� ���� Ŭ���� ����Ʈ
        internal List<QuestTask> tasks;

        // �Ҵ�� ����Ʈ
        private Quest _assignedQuest;
        // ����Ʈ ���� UI�� ������ �ִ� ���� ���� ����
        private bool _isFolded;
        // ����Ʈ�� �Ҵ�Ǿ� �ִ� ���� ���� ����
        private bool _isAssigned;

        // ������Ƽ
        public Quest AssignedQuest => _assignedQuest;
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

            _assignedQuest = quest;
            for (int i = 0; i < _assignedQuest.TaskGroups.Count; i++)
            {
                // ���� �׷��� Ȱ��ȭ���� �ʾ����� ������ ��������
                if (_assignedQuest.TaskGroups[i].State == TaskGroupState.Inactive)
                    break;

                // ������Ʈ Ǯ���� ����Ʈ ���� ������Ʈ�� �������鼭 ���� Ŭ���� ����Ʈ�� �߰�
                for (int j = 0; j < _assignedQuest.TaskGroups[i].Tasks.Count; j++)
                {
                    if (tasks.Count < j + 1)
                    {
                        tasks.Add(UIPoolManager.Instance
                                .GetObject(Strings.PoolName_QuestTask, Vector3.zero, transform)
                                .GetComponent<QuestTask>());
                    }

                    // ����Ʈ ���� �Ҵ�
                    tasks[j].Assign(_assignedQuest.TaskGroups[i].Tasks[j]);
                }
            }

            // ����Ʈ�� ī�װ��� �ؽ�Ʈ�� ǥ��
            categoryText.text = _assignedQuest.QuestState != QuestState.Complete ?
                _categoryTexts[(int)_assignedQuest.Category.questCategory] : _categoryTexts[3];

            // ����Ʈ�� Ÿ��Ʋ�� �ؽ�Ʈ�� ǥ��
            titleText.text = _assignedQuest.DisplayName;
        }

        // ����Ʈ UI�� ����_220525
        public void Unassign()
        {
            _isAssigned = false;
            _isFolded = false;
            _assignedQuest = null;

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

        // Ŭ���� ��� ����Ʈ ���� â ���̴� �Լ�
        public void OnPointerDown(PointerEventData eventData)
        {
            OnClickQuest?.Invoke(_assignedQuest);
        }
    }
}
