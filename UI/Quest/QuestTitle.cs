using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using SK.Quests;

namespace SK.UI
{
    /* 작성자: 홍상기
     * 내용: 퀘스트 타이틀 UI 표시에 관련된 클래스
     * 작성일: 22년 5월 24일
     */
    public class QuestTitle : PoolObject
    {
        // 퀘스트 카테고리 텍스트 컴포넌트
        public Text categoryText;
        // 퀘스트 타이틀 텍스트 컴포넌트
        public Text titleText;
        // 퀘스트 폴드 버튼
        public Button foldButton;
        // 퀘스트에 속한 업무 클래스 리스트
        internal List<QuestTask> tasks;

        // 퀘스트 업무 UI가 접혀져 있는 지에 대한 여부
        private bool _isFolded;
        // 퀘스트가 할당되어 있는 지에 대한 여부
        private bool _isAssigned;
        public bool IsAssigned => _isAssigned;

        private readonly string[] _categoryTexts = { "[ 메인 ]", "[ 부가 ]", "[ 길드 ]", "[ 완료 ]" };

        private void Awake()
        {
            // 리스트 초기화
            tasks = new List<QuestTask>();

            // 폴드 버튼에 이벤트 함수 등록
            foldButton.onClick.AddListener(delegate { FoldTaskList(); });
        }

        // 퀘스트 정보에 따라 UI에 표시_220525
        public void AssignQuest(Quest quest)
        {
            _isAssigned = true;

            gameObject.SetActive(true);

            for (int i = 0; i < quest.TaskGroups.Count; i++)
            {
                // 업무 그룹이 활성화되지 않았으면 루프문 빠져나옴
                if (quest.TaskGroups[i].State == TaskGroupState.Inactive)
                    break;

                // 오브젝트 풀에서 퀘스트 업무 오브젝트를 가져오면서 업무 클래스 리스트에 추가
                for (int j = 0; j < quest.TaskGroups[i].Tasks.Count; j++)
                {
                    if (tasks.Count < j + 1)
                    {
                        tasks.Add(UIPoolManager.Instance
                                .GetObject(Strings.PoolName_QuestTask, Vector3.zero, transform)
                                .GetComponent<QuestTask>());
                    }

                    // 퀘스트 업무 할당
                    tasks[j].Assign(quest.TaskGroups[i].Tasks[j]);
                }
            }

            // 퀘스트의 카테고리를 텍스트로 표시
            categoryText.text = quest.QuestState != QuestState.Complete ?
                _categoryTexts[(int)quest.Category.questCategory] : _categoryTexts[3];

            // 퀘스트의 타이틀을 텍스트로 표시
            titleText.text = quest.DisplayName;
        }

        // 퀘스트 UI를 해제_220525
        public void Unassign()
        {
            _isAssigned = false;
            _isFolded = false;

            // 업무 리스트의 각 업무를 해제
            for (int i = 0; i < tasks.Count; i++)
                if (tasks[i].IsAssigned)
                    tasks[i].Unassign();

            // 현재 퀘스트 타이틀 오브젝트를 끔
            gameObject.SetActive(false);
        }

        // 퀘스트 업무 UI를 접고 펴는 함수
        private void FoldTaskList()
        {
            // 접혀있지 않은 경우
            if (!_isFolded)
            {
                // 표시된 모든 업무 UI를 끔
                foreach (var task in tasks)                
                    task.gameObject.SetActive(false);                
            }
            // 접혀있는 경우
            else
            {
                foreach (var task in tasks)
                {
                    if (task._assignedTask.State != TaskState.Inactive)
                        task.gameObject.SetActive(true);
                }
            }

            // 접힌 여부를 변경
            _isFolded = !_isFolded;
        }
    }
}
