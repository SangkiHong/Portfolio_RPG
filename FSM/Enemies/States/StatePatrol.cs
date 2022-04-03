using UnityEngine;

namespace SK.FSM
{
    public class StatePatrol : StateBase
    {
        private readonly Enemy _enemy;
        private readonly EnemyStateMachine _stateMachine;

        private Vector3 _randomPos;
        private float _seekIdleTimer;
        
        public StatePatrol(Enemy enemyControl, EnemyStateMachine stateMachine)
        {
            _enemy = enemyControl;
            _stateMachine = stateMachine;
        }

        public override void StateInit()
        {
            _enemy.navAgent.speed = _enemy.enemyData.Speed * 0.5f; // 절반 속도로 이동
            _enemy.walkAnimSpeed = 0.5f; // 속도에 맞춰 Animation 동기화

            if (!_enemy.isPatrol) return;

            // NavAgent 재가동
            if (_enemy.navAgent.isOnNavMesh && _enemy.navAgent.isStopped)
            {
                _enemy.navAgent.isStopped = false;
                _enemy.navAgent.updatePosition = true;
                _enemy.navAgent.updateRotation = true;
            }
        }

        public override void FixedTick()
        {
            _enemy.combat.SetTarget(_enemy.searchRadar.FindTarget());

            if (_enemy.combat.TargetObject)
            {
                _enemy.stateMachine.ChangeState(_stateMachine.stateChase);
                return;
            }

            if (!_enemy.isPatrol) return;

            // 이동중이면 리턴
            if (_enemy.navAgent.velocity.magnitude > 0.1f) return;
            
            // 순찰 타이머
            if (_seekIdleTimer < _enemy.searchRadar.SeekIdleDuration)
            {
                _seekIdleTimer += _enemy.fixedDelta;
            }
            else
            {
                _randomPos = _enemy.searchRadar.SeekAndWonder(_enemy.searchRadar.SeekDistance);
                _enemy.navAgent.SetDestination(_randomPos);
                _seekIdleTimer = 0;
                _enemy.navAgent.stoppingDistance = 0;
            }
        }
    }
}