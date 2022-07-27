using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum TaskState
{
    Inactive,
    Running,
    Complete
}

namespace SK.Quests
{
    /* 작성자: 홍상기
     * 내용: 퀘스트 업무에 대한 전반적인 설정 값을 가진 모듈
     * 작성일: 22년 5월 19일
     */
    [CreateAssetMenu(menuName = "Quest/Task/Task", fileName = "Task_")]
    public class Task : ScriptableObject
    {
        #region Events
        public delegate void StateChangedHandler(Task task, TaskState currentState, TaskState prevState);
        public delegate void SuccessChangedHandler(Task task, int currentSuccess, int prevSuccess);
        #endregion

        [Header("Category")]
        // 현재 퀘스트 업무의 카테고리
        [SerializeField] private QuestCategory category; 

        [Header("Text")]
        [SerializeField] private string codeName;
        [SerializeField] private string description;

        [Header("Action")]
        [SerializeField] TaskAction action;

        [Header("Target")]
        // 퀘스트 완료를 위한 타겟 정보를 담고 있는 모듈
        [SerializeField] TaskTarget[] targets;

        [Header("Setting")]
        [SerializeField] private int requireLevel;
        // 초기 성공 횟수를 받을 모듈
        [SerializeField] private InitialSuccessValue initialSuccessValue;
        
        // 퀘스트가 완료되기 위한 성공 횟수
        [SerializeField] private int needSuccessToComplete; 
        
        // 퀘스트가 완료되었어도 계속해서 성공 횟수를 보고 받을 지에 대한 옵션
        [SerializeField] private bool canReceiveReportsDuringCompletion; 

        // 퀘스트 업무 상태
        private TaskState _taskState;
        private int _currentSuccess;

        // 퀘스트 상태 변화 시 호출 이벤트
        public event StateChangedHandler onStateChanged;
        public event SuccessChangedHandler onSuccessChanged;

        // 현재 완수한 카운트
        public int CurrentSuccess 
        {
            get => _currentSuccess;
            set
            {
                // 변경 전 완료 횟수를 변수에 저장
                int prevSuccess = _currentSuccess;
                // 필요 횟수 안에서 현재 완료 횟수를 업데이트
                _currentSuccess = Mathf.Clamp(value, 0, needSuccessToComplete);
                if (_currentSuccess != prevSuccess)
                {
                    // 완료 횟수가 필요 횟수와 같으면 업무 상태를 완료로, 그렇지 않으면 업무중으로 변경
                    State = _currentSuccess == needSuccessToComplete ? TaskState.Complete : TaskState.Running;
                    // 완료 횟수 변경에 따른 이벤트 호출
                    onSuccessChanged?.Invoke(this, _currentSuccess, prevSuccess);
                }
            }
        }
        public QuestCategory Category => category;
        public string CodeName => codeName;
        public string Description => description;
        public int RequireLevel => requireLevel;
        public int NeedSuccessToComplete => needSuccessToComplete;
        public TaskState State
        {
            get => _taskState;
            set
            {
                var prevState = _taskState;
                _taskState = value;
                onStateChanged?.Invoke(this, _taskState, prevState);
            }
        }
        public bool IsComplete => _taskState == TaskState.Complete;
        public Quest OwnerQuest { get; private set; }

        // 업무의 퀘스트를 할당
        public void Setup(Quest owner) => OwnerQuest = owner;        

        // 업무 시작과 초기화하는 함수
        public void Start()
        {
            // 업무 상태 초기화
            State = TaskState.Running;

            // 초기 완료 횟수가 있으면 완료 횟수에 할당
            if (initialSuccessValue)
                _currentSuccess = initialSuccessValue.GetValue(this);
        }

        // 업무가 종료되는 함수
        public void End()
        {
            // 이벤트 등록을 해제
            onStateChanged = null;
            onSuccessChanged = null;
        }

        public void SetCurrentSuccess(int data)
        {
            _currentSuccess = data;
            if (_currentSuccess >= needSuccessToComplete)
                _taskState = TaskState.Complete;
        }

        // TaskAction의 현재 완수 횟수와 인자로 받은 횟수를 더하여 현재 완수 횟수를 업데이트_220519
        public bool ReceiveReport(int successCount)
        {
            CurrentSuccess = action.Run(this, CurrentSuccess, successCount);
            Debug.Log($"타겟 확인 완료 후 리포팅: {successCount}, 현재 완료 횟수: {_currentSuccess}");

            // 업무 완수 횟수가 충족된 경우
            if (_currentSuccess >= needSuccessToComplete)
            {
                _taskState = TaskState.Complete;
                return true;
            }

            return false;
        }

        // 퀘스트 업무를 취소하는 함수
        public void Cancel()
        {
            _currentSuccess = 0;
            _taskState = TaskState.Inactive;
        }

        // 퀘스트 즉시 완료하는 함수
        public void Complete()
            => _currentSuccess = needSuccessToComplete;

        // 타겟 전체를 돌며 비교하여 동일한지를 반환하는 함수_220519
        public bool IsTarget(QuestCategory category, object target)
            => Category == category && 
            targets.Any(x => x.IsEqual(target)) &&
            (!IsComplete || (!IsComplete && canReceiveReportsDuringCompletion));
    }
}
