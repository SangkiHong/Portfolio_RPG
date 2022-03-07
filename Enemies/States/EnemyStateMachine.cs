using UnityEngine;

namespace SK.FSM
{
    public class EnemyStateMachine
    {
        public EnemyState CurrentState { get; private set; }

        public void ChangeState(EnemyState state)
        {
            if (CurrentState != null) 
                CurrentState.StateExit();
            CurrentState = state;
            CurrentState.StateInit();
        }
    }
}