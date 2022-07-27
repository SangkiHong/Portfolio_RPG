using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SK.Quests
{
    /* 작성자: 홍상기
     * 내용: 퀘스트 업무 그룹에 대한 설정 값을 가진 모듈
     * 작성일: 22년 5월 19일
     */

    // 퀘스트 업무 그룹의 상태 Enum
    public enum TaskGroupState
    {
        Inactive,
        Running,
        Complete
    }

    [System.Serializable]
    public class TaskGroup
    {
        [SerializeField] private Task[] tasks;

        public IReadOnlyList<Task> Tasks => tasks;
        public Quest OwnerQuest { get; private set; }

        public bool IsAllTaskComplete => tasks.All(x => x.IsComplete);
        public bool IsComplete => State == TaskGroupState.Complete;

        public TaskGroupState State { get; internal set; }

        // 그룹 내 모든 퀘스트 업무의 퀘스트를 할당
        public void Setup(Quest owner)
        {
            OwnerQuest = owner;
            foreach (var task in tasks)
            {
                task.Setup(owner);
            }
        }

        public void Start()
        {
            State = TaskGroupState.Running;
            foreach (var task in tasks)
            {
                task.Start();
            }
        }

        public void End()
        {
            foreach (var task in tasks)
            {
                task.End();
            }
        }

        // 업무가 해당 카테고리와 타겟을 가지고 있으면 목표 대상이므로 보고를 받는 함수
        public void ReceiveReport(QuestCategory category, object target, int successCount)
        {
            Debug.Log("TraskGroup ReceiveReport");
            foreach (var task in tasks)
            {
                if (task.IsTarget(category, target))
                    task.ReceiveReport(successCount);
            }
        }

        // 모든 업무를 취소하며 초기화하는 함수
        public void Cancel()
        {
            State = TaskGroupState.Inactive;

            foreach (var task in tasks)
            {
                if (!task.IsComplete)
                    task.Cancel();
            }
        }

        // 업무 그룹을 완료하는 함수
        public void Complete()
        {
            if (IsComplete)
                return;

            State = TaskGroupState.Complete;

            foreach (var task in tasks)
            {
                if (!task.IsComplete)
                    task.Complete();
            }
        }
    }
}