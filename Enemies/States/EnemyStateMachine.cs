﻿using UnityEngine;

namespace SK.FSM
{
    public class EnemyStateMachine
    {
        private Enemy _enemy;
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
    }
}