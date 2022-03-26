using UnityEngine;

namespace SK.FSM
{
    public class EnemyStateMachine
    {
        private readonly Enemy _enemy;
        public EnemyStateMachine(Enemy enemy)
        {
            _enemy = enemy;
        }

        public EnemyState CurrentState { get; private set; }

        public void ChangeState(EnemyState state)
        {
            if (CurrentState != null) 
                CurrentState.StateExit();
            CurrentState = state;
            CurrentState.StateInit();
            _enemy.currentStateName = state.GetType().Name;
        }

        public void OnIdleness()
        {
            CurrentState = null;
            
            // Stop NavAgent
            if (!_enemy.NavAgent.isStopped)
            {
                _enemy.NavAgent.isStopped = true;
                _enemy.NavAgent.updatePosition = false;
                _enemy.NavAgent.updateRotation = false;
            }
        }
    }
}