namespace SK.FSM
{
    public class EnemyStateChase : StateBase
    {
        private readonly Enemy _enemy;
        private readonly EnemyStateMachine _stateMachine;
        private float _combatDistance;
        private float _searchDist;

        public EnemyStateChase(Enemy enemyControl, EnemyStateMachine stateMachine)
        {
            _enemy = enemyControl;
            _stateMachine = stateMachine;
            _combatDistance = enemyControl.combat.combatDistance;
            // 거리 비교 연산을 위해 제곱
            _combatDistance *= _combatDistance;
            _searchDist = _enemy.searchRadar.searchDistance + 1.5f;
            // 거리 비교 연산을 위해 제곱
            _searchDist *= _searchDist;
        }

        public override void StateInit()
        {
            // NavAgent 재가동
            _enemy.navAgent.Warp(_enemy.mTransform.position);
            _enemy.navAgent.isStopped = false;
            _enemy.navAgent.updatePosition = true;
            _enemy.navAgent.updateRotation = true;
            _enemy.navAgent.angularSpeed = 480;

            _enemy.navAgent.speed = _enemy.enemyData.Speed;
            _enemy.walkAnimSpeed = 1f;

            _enemy.anim.SetFloat(Strings.AnimPara_Sideways, 0);
        }

        public override void FixedTick()
        {
            // 타겟이 없거나 타겟이 탐색 범위 밖으로 벗어나면 순찰 상태로 전환
            if (_enemy.targetDistance > _searchDist)
            {
                _enemy.stateMachine.statePatrol.isReturnSpawnPoint = true;
                _enemy.UnassignTarget();
                return;
            }
            
            // 전투 사거리에 진입 시 전투 상태로 전환
            if (_enemy.targetDistance < _combatDistance)
            {
                _enemy.stateMachine.ChangeState(_stateMachine.stateCombat);
                return;
            }
            
            if (!_enemy.navAgent.pathPending) // 경로 계산 완료
            {
                if (!_enemy.isInteracting) // Interacting State Check
                {
                    _enemy.navAgent.SetDestination(_enemy.combat.Target.transform.position);
                }
            }
        }
    }
}