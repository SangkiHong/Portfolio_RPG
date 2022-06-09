using UnityEngine;
using UnityEngine.UI;
using SK.Quests;
using System.Text;

namespace SK.UI
{
    /* 작성자: 홍상기
     * 내용: 퀘스트 업무에 대한 정보를 표시하는 UI 모듈
     * 작성일: 22년 5월 24일
     */
    public class QuestTask : PoolObject
    {
        // 요구 레벨을 표시하는 텍스트
        public Text requireLevelText;
        // 업무 타이틀을 표시하는 텍스트
        public Text taskTitleText;

        // 할당된 업무를 저장할 변수
        internal Task _assignedTask;

        // 퀘스트 업무의 할당된 여부
        private bool _isAssigned;
        public bool IsAssigned => _isAssigned;

        private StringBuilder _stringBuilder;
        private const string _string_CloseBrace = " ]";

        private void Awake()
        {
            _stringBuilder = new StringBuilder();
            _stringBuilder.Append("[ 권장 레벨 ");
        }

        public void Assign(Task task)
        {
            _isAssigned = true;

            gameObject.SetActive(true);

            _assignedTask = task;

            // 권장 레벨 텍스트에 표시
            if (_stringBuilder.Length > 8)
                _stringBuilder.Remove(8, _stringBuilder.Length - 8);
            _stringBuilder.Append(_assignedTask.RequireLevel);
            _stringBuilder.Append(_string_CloseBrace);
            requireLevelText.text = _stringBuilder.ToString();
            // 업무 타이틀 텍스트에 표시
            taskTitleText.text = _assignedTask.Description;
        }

        public void Unassign()
        {
            _isAssigned = false;

            _assignedTask = null; 
            gameObject.SetActive(false);
        }
    }
}
