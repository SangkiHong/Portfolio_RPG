using UnityEngine;

namespace SK.FSM
{
    public class StatePatrol : EnemyState
    {
        private readonly Enemy _enemy;

        private Vector3 _randomPos;
        private float _seekIdleTimer;
        
        public StatePatrol(Enemy enemyControl)
        {
            _enemy = enemyControl;
        }

        public override void StateInit()
        {
            // NavAgent 재가동
            if (_enemy.NavAgent.isOnNavMesh && _enemy.NavAgent.isStopped)
            {
                _enemy.NavAgent.isStopped = false;
                _enemy.NavAgent.updatePosition = true;
                _enemy.NavAgent.updateRotation = true;
            }
            
            _enemy.NavAgent.speed = _enemy.enemyData.Speed * 0.5f; // 절반 속도로 이동
            _enemy.walkAnimSpeed = 0.5f; // 속도에 맞춰 Animation 동기화
        }

        public override void FixedTick()
        {
            _enemy.targetObject = _enemy.searchRadar.FindTarget();

            if (_enemy.targetObject)
            {
                _enemy.stateMachine.ChangeState(_enemy.stateChase);
                return;
            }

            // 이동중이면 리턴
            if (_enemy.NavAgent.velocity.magnitude > 0.1f) return;
            
            // 순찰 타이머
            if (_seekIdleTimer < _enemy.searchRadar.SeekIdleDuration)
            {
                _seekIdleTimer += _enemy.fixedDelta;
            }
            else
            {
                _randomPos = _enemy.searchRadar.SeekAndWonder(_enemy.searchRadar.SeekDistance);
                _enemy.NavAgent.SetDestination(_randomPos);
                _seekIdleTimer = 0;
                _enemy.NavAgent.stoppingDistance = 0;
            }
        }
    }
}