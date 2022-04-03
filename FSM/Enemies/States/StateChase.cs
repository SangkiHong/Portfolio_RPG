using UnityEngine;

namespace SK.FSM
{
    public class StateChase : StateBase
    {
        private readonly Enemy _enemy;
        private readonly EnemyStateMachine _stateMachine;

        public StateChase(Enemy enemyControl, EnemyStateMachine stateMachine)
        {
            _enemy = enemyControl;
            _stateMachine = stateMachine;
        }

        public override void StateInit()
        {
            // NavAgent 재가동
            if (_enemy.navAgent.isStopped)
            {
                _enemy.navAgent.Warp(_enemy.mTransform.position);
                _enemy.navAgent.isStopped = false;
                _enemy.navAgent.updatePosition = true;
                _enemy.navAgent.updateRotation = true;
            }
            
            _enemy.navAgent.speed = _enemy.enemyData.Speed;
            _enemy.walkAnimSpeed = 1f;
        }

        public override void FixedTick()
        {
            // 타겟과의 거리
            var dist = Vector3.Distance(_enemy.combat.TargetObject.transform.position, _enemy.mTransform.position);

            // 타겟이 없거나 타겟이 탐색 범위 밖으로 벗어나면 순찰 상태로 전환
            if (dist > _enemy.searchRadar.SeekDistance)
            {
                _enemy.UnassignTarget();
                return;
            }
            
            // 전투 사거리에 진입 시 전투 상태로 전환
            if (dist < _enemy.combat.combatDistance)
            {
                _enemy.stateMachine.ChangeState(_stateMachine.stateCombat);
                return;
            }
            
            if (!_enemy.navAgent.pathPending) // 경로 계산 완료
            {
                if (!_enemy.isInteracting) // Interacting State Check
                {
                    _enemy.navAgent.SetDestination(_enemy.combat.TargetObject.transform.position);
                }
            }
        }
    }
}