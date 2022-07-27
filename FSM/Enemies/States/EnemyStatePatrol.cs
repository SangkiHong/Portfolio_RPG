using UnityEngine;

namespace SK.FSM
{
    public class EnemyStatePatrol : StateBase
    {
        private readonly Enemy _enemy;
        private readonly EnemyStateMachine _stateMachine;

        public bool isReturnSpawnPoint;

        private Vector3 _randomPos;
        private float _seekIdleTimer;
        private float _seekDistance;
        
        public EnemyStatePatrol(Enemy enemyControl, EnemyStateMachine stateMachine)
        {
            _enemy = enemyControl;
            _stateMachine = stateMachine;
            _seekDistance = enemyControl.searchRadar.searchDistance;
        }

        public override void StateInit()
        {
            _enemy.navAgent.speed = _enemy.enemyData.Speed * 0.5f; // 절반 속도로 이동
            _enemy.walkAnimSpeed = 0.5f; // 속도에 맞춰 Animation 동기화

            if (!isReturnSpawnPoint && !_enemy.isPatrol) return;

            // NavAgent 재가동
            if (_enemy.navAgent.isOnNavMesh)
            {
                _enemy.navAgent.isStopped = false;
                _enemy.navAgent.updatePosition = true;
                _enemy.navAgent.updateRotation = true;
            }
        }

        public override void FixedTick()
        {
            _enemy.combat.SetTarget(_enemy.searchRadar.FindTarget());

            if (_enemy.combat.Target)
            {
                _enemy.stateMachine.ChangeState(_stateMachine.stateChase);
                return;
            }

            if (isReturnSpawnPoint)
            {
                _enemy.navAgent.SetDestination(_enemy.RespawnPoint);
                isReturnSpawnPoint = false;
                return;
            }

            // 순찰하지 않는 유닛이거나 이동중이라면 리턴
            if (!_enemy.isPatrol || _enemy.navAgent.velocity.magnitude > 0.1f) return;

            // 순찰 타이머
            if (_seekIdleTimer < _enemy.searchRadar.idleDuration)            
                _seekIdleTimer += _enemy.fixedDeltaTime;            
            else
            {
                _randomPos = _enemy.searchRadar.GetPatrolPoint(_seekDistance);
                _enemy.navAgent.SetDestination(_randomPos);
                _seekIdleTimer = 0;
            }
        }
    }
}