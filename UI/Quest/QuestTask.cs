using UnityEngine;
using UnityEngine.UI;
using SK.Quests;
using System.Text;

namespace SK.UI
{
    /* �ۼ���: ȫ���
     * ����: ����Ʈ ������ ���� ������ ǥ���ϴ� UI ���
     * �ۼ���: 22�� 5�� 24��
     */
    public class QuestTask : PoolObject
    {
        // �䱸 ������ ǥ���ϴ� �ؽ�Ʈ
        public Text requireLevelText;
        // ���� Ÿ��Ʋ�� ǥ���ϴ� �ؽ�Ʈ
        public Text taskTitleText;
        // ����Ʈ �׺���̼� ��ư
        [SerializeField] Button navButton;

        // �Ҵ�� ������ ������ ����
        internal Task _assignedTask;

        // ����Ʈ ������ �Ҵ�� ����
        private bool _isAssigned;
        public bool IsAssigned => _isAssigned;

        private StringBuilder _stringBuilder;
        private const string _string_CloseBrace = " ]";

        private void Awake()
        {
            _stringBuilder = new StringBuilder();
            _stringBuilder.Append("[ ���� ���� ");
        }

        public void Assign(Task task, bool isCompletedQuest)
        {
            _isAssigned = true;

            gameObject.SetActive(true);

            _assignedTask = task;

            // ���� ���� �ؽ�Ʈ�� ǥ��
            if (_stringBuilder.Length > 8)
                _stringBuilder.Remove(8, _stringBuilder.Length - 8);
            _stringBuilder.Append(_assignedTask.RequireLevel);
            _stringBuilder.Append(_string_CloseBrace);
            requireLevelText.text = _stringBuilder.ToString();
            // ���� Ÿ��Ʋ �ؽ�Ʈ�� ǥ��
            taskTitleText.text = _assignedTask.Description;

            // �׺���̼� ��ư ǥ��
            if (!isCompletedQuest)
                navButton.gameObject.SetActive(true);
            else // �Ϸ�� ��� ����
                navButton.gameObject.SetActive(false);
        }

        public void Unassign()
        {
            _isAssigned = false;

            _assignedTask = null; 
            gameObject.SetActive(false);
        }
    }
}
