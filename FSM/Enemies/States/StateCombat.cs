using UnityEngine;
using UnityEngine.AI;

namespace SK.FSM
{
    public class StateCombat : StateBase
    {
        private readonly Enemy _enemy;
        private readonly EnemyStateMachine _stateMachine;

        private Vector3 _randomPoint;
        private float _targetDist, _attackTimer;

        public StateCombat(Enemy enemyControl, EnemyStateMachine stateMachine)
        {
            _enemy = enemyControl;
            _stateMachine = stateMachine;
        }

        public override void StateInit()
        {
            _attackTimer = _enemy.AttackCooldown;
            _enemy.anim.SetBool(Strings.AnimPara_isFight, true);

            if (_enemy.navAgent.isStopped) _enemy.navAgent.isStopped = false;
            _enemy.navAgent.updateRotation = false;
            _enemy.navAgent.SetDestination(CombatRandomPos());
            _enemy.walkAnimSpeed = 0.5f;

            // Assign Check
            if (_enemy.targetState && _enemy.targetState.gameObject != _enemy.combat.TargetObject)
                return;

            // Target Assign
            _enemy.targetState = _enemy.combat.TargetObject.GetComponent<PlayerStateManager>();
        }

        public override void FixedTick()
        {
            if (_enemy.isInteracting)
                return;

            FollowTarget();

            // 타겟이 공격범위 밖으로 벗어난 경우 추적 상태(Chase State)로 전환
            _targetDist = Vector3.Distance(_enemy.mTransform.position, _enemy.combat.TargetObject.transform.position);
            if (_targetDist > _enemy.combat.combatDistance + 1)
            {
                _enemy.stateMachine.ChangeState(_stateMachine.stateChase);
                return;
            }

            // Attack Cooldown
            if (_attackTimer > 0 && !_enemy.isInteracting)
            {
                _attackTimer -= _enemy.fixedDelta;
            }
            else // Do Attack
            {
                if (!_enemy.isInteracting)
                {
                    _attackTimer = _enemy.AttackCooldown;
                    _enemy.stateMachine.ChangeState(_stateMachine.stateAttack);
                    if (_enemy.combat) _enemy.combat.ExecuteAttack();
                }
            }

            Debug.DrawRay(_randomPoint, Vector3.up * 3, Color.red);
        }
        
        // 타겟 중심으로 180도 반경 내 위치 찾기
        private Vector3 CombatRandomPos()
        {
            if (NavMesh.SamplePosition(GetAroundPosition(90), out var navMeshHit, 3, NavMesh.AllAreas))
            {
                _randomPoint = navMeshHit.position;
                return navMeshHit.position;
            }            

            return _enemy.mTransform.position;
        }
        
        private Vector3 GetAroundPosition(float angle)
        {
            float randomVal = Random.Range(-1f, 1f); // 반원에서 방향을 랜덤 값으로 정함
            angle = randomVal * angle; // 0 ~ angle 사이의 각을 구함

            return _enemy.combat.TargetObject.transform.position
                   + _enemy.combat.TargetObject.transform.rotation
                   * Quaternion.Euler(0, angle, 0) * (Vector3.forward * _enemy.combat.combatDistance);
        }
        
        // Targeting
        private void FollowTarget()
        {
            Vector3 dir = (_enemy.combat.TargetObject.transform.position - _enemy.mTransform.position).normalized;
            _enemy.mTransform.rotation = Quaternion.Lerp(_enemy.mTransform.rotation, Quaternion.LookRotation(dir), _enemy.fixedDelta * _enemy.LookTargetSpeed);
        }
    }
}