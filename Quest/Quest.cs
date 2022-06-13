using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace SK.Quests
{
    /* 작성자: 홍상기
     * 내용: 퀘스트에 대한 설정 값을 가진 모듈
     * 작성일: 22년 5월 19일
     */

    [System.Serializable]
    public enum QuestState
    {
        Inactive,
        Running,
        Complete, // 자동으로 완료되는 상태
        Cancel,
        WaitingForCompletion // 완료 버튼을 누르면 완료되는 상태
    }

    public enum ItemList : int { Equipment, Weapon, Props }

    [System.Serializable]
    public struct RewardItem
    {
        public ItemList itemList;
        public int itemId;
        public uint itemAmount;
    }

    [System.Serializable]
    public struct Reward
    {
        public uint exp;
        public uint gold;
        public uint gem;
        public RewardItem[] rewardItems; // 최대 5개를 넘지 않음
    }

    [CreateAssetMenu(menuName = "Quest/Quest", fileName = "Quest_")]
    public class Quest : ScriptableObject
    {
        #region Events
        public delegate void NewTaskGroupHandler(Quest quest, TaskGroup currentTaskGroup, TaskGroup prevTaskGroup);
        public delegate void TaskSuccessChangedHandler(Quest quest, Task task, int currentSuccess, int prevSuccess);
        public delegate void CompletedHandler(Quest quest);
        public delegate void CanceledHandler(Quest quest);
        #endregion

        [SerializeField] private QuestCategory category;

        [Header("Text")]
        [SerializeField] private string displayName;
        [SerializeField, TextArea] private string description;

        [Header("Task")]
        [SerializeField] private TaskGroup[] taskGroups;

        [Header("NPC")]
        [SerializeField] private string completeNPC;

        [Header("Reward")]
        [SerializeField] private Reward reward;

        [Header("Option")]
        [SerializeField] private bool useAutoComplete;
        [SerializeField] private bool isCancelable;

        [Header("Condition")]
        // 퀘스트 수락 가능 조건
        [SerializeField] private QuestCondition[] acceptionCondition;
        // 퀘스트 취소 가능 조건
        [SerializeField] private QuestCondition[] cancelationCondition;

        // 현재 업무 그룹의 인덱스
        private int currentTaskGroupindex;

        public QuestCategory Category => category;
        public string DisplayName => displayName;
        public string Description => description;
        public string CompleteNPC => completeNPC;
        public int CurrentTaskGroupIndex => currentTaskGroupindex;

        public QuestState QuestState { get; private set; }
        public TaskGroup CurrentTaskGroup => taskGroups[currentTaskGroupindex];
        public IReadOnlyList<TaskGroup> TaskGroups => taskGroups;
        public Reward Reward => reward;

        public bool IsRegistered => QuestState != QuestState.Inactive;
        public bool IsCompletable => QuestState == QuestState.WaitingForCompletion;
        public bool IsComplete => QuestState == QuestState.Complete;
        public bool IsCancel => QuestState == QuestState.Cancel;
        public bool IsAcceptable 
        {
            get
            {
                if (acceptionCondition != null)
                    return acceptionCondition.All(x => x.IsPass(this));
                else
                    return true;
            }
        }
        public bool IsCancelable 
        {
            get
            {
                if (cancelationCondition != null)
                    return isCancelable && cancelationCondition.All(x => x.IsPass(this));
                else
                    return true;
            }
        }
        internal void SetState(QuestState state) { QuestState = state; }
        internal void SetTaskGroupIndex(int index) { currentTaskGroupindex = index; }
        internal void SetTaskGroupState(TaskGroupState state) { CurrentTaskGroup.State = state; }

        public event NewTaskGroupHandler onNewTaskGroup;
        public event TaskSuccessChangedHandler onTaskSuccessChanged;
        public event CompletedHandler onCompleted;
        public event CanceledHandler onCanceled;

        // 퀘스트가 시스템 등록되었을 Awake역할의 함수_220520
        public void OnRegister()
        {
            // 퀘스트 중복 등록 방지 위한 디버그
            //Debug.Assert(!IsRegistered, "This quest has already been registered.");

            foreach (var taskGroup in taskGroups)
            {
                // 업무 그룹에 퀘스트 할당_220520
                taskGroup.Setup(this);

                // 각 업무 그룹의 업무마다 콜백 함수 등록_220520
                foreach (var task in taskGroup.Tasks)
                    task.onSuccessChanged += OnSuccessChanged;
            }

            QuestState = QuestState.Running;
            CurrentTaskGroup.Start();
        }

        // 퀘스트 완수를 보고받는 함수_220520
        public void ReceiveReport(QuestCategory category, object target, int successCount)
        {
            // 퀘스트 중복 등록 방지 위한 디버그
            Debug.Assert(!IsRegistered, "This quest has already been registered.");
            // 퀘스트가 취소되었는 지에 대한 디버그
            Debug.Assert(!IsCancel, "This quest has  been canceled.");

            // 퀘스트가 완료된 경우 return
            if (IsComplete)
                return;

            // 현재 업무 그룹에 완료 횟수 보고 함수 호출
            CurrentTaskGroup.ReceiveReport(category, target, successCount);

            // 현재 할당된 퀘스트 업무가 완료된 경우
            if (CurrentTaskGroup.IsAllTaskComplete)
            {
                // 모든 업무가 완료된 경우
                if (currentTaskGroupindex + 1 == taskGroups.Length)
                {
                    // 완료 대기 상태로 전환
                    QuestState = QuestState.WaitingForCompletion;
                    // 자동 완료 옵션인 경우 즉시 완료
                    if (useAutoComplete)
                        Complete();
                }
                else
                {
                    // 현재 업무를 변수에 저장 후 업무 인덱스 값으로 증가
                    var prevTaskGroup = taskGroups[currentTaskGroupindex++];
                    // 완료된 업무 종료 함수 호출
                    prevTaskGroup.End();
                    // 새로 할당된 업무 시작 함수 호출
                    CurrentTaskGroup.Start();
                    // 새로운 업무 등록 콜백 함수 호출
                    onNewTaskGroup?.Invoke(this, CurrentTaskGroup, prevTaskGroup);
                }
            }
            else
                QuestState = QuestState.Running;
        }

        // 퀘스트 완료하는 함수_220520
        public void Complete()
        {
            CheckIsRunning();

            // 모든 업무 그룹을 완료하는 함수 호출
            foreach (var taskGroup in taskGroups)
                taskGroup.Complete();

            // 퀘스트를 완료 상태로 변경
            QuestState = QuestState.Complete;

            // 보상 지급
            GameManager.Instance.DataManager.GetReward(reward);

            onCompleted?.Invoke(this);

            onTaskSuccessChanged = null;
            onCompleted = null;
            onCanceled = null;
            onNewTaskGroup = null;
        }

        // 퀘스트를 취소(포기)하는 함수_220520
        public void Cancel()
        {
            CheckIsRunning();
            Debug.Assert(IsCancelable, "This quest can't be canceled");

            QuestState = QuestState.Cancel;
            onCanceled?.Invoke(this);
        }

        // 업무 이벤트에 퀘스트의 이벤트를 등록하기 위한 콜백 함수_220520
        private void OnSuccessChanged(Task task, int currentSuccess, int prevSuccess)
            => onTaskSuccessChanged?.Invoke(this, task, currentSuccess, prevSuccess);

        // 유니티 에디터 상에서만 실행될 수 있도록 어트리뷰트 세팅_220520
        [Conditional("UNITY_EDITOR")]
        private void CheckIsRunning()
        {
            // 퀘스트 중복 등록 방지 위한 디버그
            Debug.Assert(!IsRegistered, "This quest has already been registered.");
            // 퀘스트가 취소되었는 지에 대한 디버그
            Debug.Assert(!IsCancel, "This quest has  been canceled.");
            // 완료 가능한 퀘스트인지 확인하기 위한 디버그
            Debug.Assert(!IsCompletable, "This quest has already been completed.");
        }
    }
}