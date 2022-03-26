using UnityEngine;

namespace SK.FSM
{
    public class StateChase : EnemyState
    {
        private readonly Enemy _enemy;
        
        public StateChase(Enemy enemyControl)
        {
            _enemy = enemyControl;
        }

        public override void StateInit()
        {
            // NavAgent 재가동
            if (_enemy.NavAgent.isStopped)
            {
                _enemy.NavAgent.Warp(_enemy.mTransform.position);
                _enemy.NavAgent.isStopped = false;
                _enemy.NavAgent.updatePosition = true;
                _enemy.NavAgent.updateRotation = true;
            }
            
            _enemy.NavAgent.speed = _enemy.enemyData.Speed;
            _enemy.walkAnimSpeed = 1f;
        }

        public override void FixedTick()
        {
            // 타겟이 없거나 타겟이 탐색 범위 밖으로 벗어나면 순찰 상태로 전환
            if (!_enemy.targetObject || _enemy.NavAgent.remainingDistance + 1 > _enemy.searchRadar.SeekDistance)
            {
                _enemy.stateMachine.ChangeState(_enemy.statePatrol);
                return;
            }
            
            // 타겟과의 거리
            var dist = Vector3.Distance(_enemy.targetObject.transform.position, _enemy.mTransform.position);
            
            // 전투 사거리에 진입 시 전투 상태로 전환
            if (dist < _enemy.combat.combatDistance)
            {
                _enemy.stateMachine.ChangeState(_enemy.stateCombat);
                return;
            }
            
            if (!_enemy.NavAgent.pathPending) // 경로 계산 완료
            {
                if (!_enemy.isInteracting) // Interacting State Check
                {
                    _enemy.NavAgent.SetDestination(_enemy.targetObject.transform.position);
                }
            }
        }
    }
}