using UnityEngine;
using UnityEngine.AI;

namespace SK.FSM
{
    public class StateCombat : EnemyState
    {
        private readonly Enemy _enemy;
        private PlayerStateManager _targetState;

        private float _targetDist, _attackTimer;
        
        public StateCombat(Enemy enemyControl)
        {
            _enemy = enemyControl;
        }

        public override void StateInit()
        {
            _attackTimer = _enemy.attackCooldown;
            _enemy.Anim.SetBool(Strings.AnimPara_isFight, true);
            //_enemy.NavAgent.SetDestination(CombatRandomPos());
            _enemy.walkAnimSpeed = 0.5f;

            // Assign Check
            if (_targetState && _targetState.gameObject != _enemy.targetObject)
                return;

            // Target Assign
            _targetState = _enemy.targetObject.GetComponent<PlayerStateManager>();
        }

        public override void FixedTick()
        {
            if (_enemy.isInteracting)
                return;

            if (!_enemy.targetObject || _targetState.isDead)
            {
                _enemy.targetObject = null;

                _enemy.stateMachine.ChangeState(_enemy.statePatrol);
                return;
            }

            FollowTarget();

            // 타겟이 공격범위 밖으로 벗어난 경우 추적 상태(Chase State)로 전환
            _targetDist = Vector3.Distance(_enemy.mTransform.position, _enemy.targetObject.transform.position);
            if (_targetDist > _enemy.combat.combatDistance)
            {
                _enemy.stateMachine.ChangeState(_enemy.stateChase);
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
                    _attackTimer = _enemy.attackCooldown;
                    _enemy.OnAttack();
                }
            }
        }
        
        // 타겟 중심으로 180도 반경 내 위치 찾기
        private Vector3 CombatRandomPos()
        {
            if (NavMesh.SamplePosition(GetDodgePoint(180), out var navMeshHit, 3, NavMesh.AllAreas))
                return navMeshHit.position;
            
            return _enemy.mTransform.position;
        }
        
        private Vector3 GetDodgePoint(float angle)
        {
            float randomVal = Random.Range(-1f, 1f); // 반원에서 방향을 랜덤 값으로 정함
            angle = randomVal * angle; // 0 ~ angle 사이의 각을 구함

            return _enemy.targetObject.transform.position
                   + _enemy.mTransform.rotation * Quaternion.Euler(0, angle, 0) * (Vector3.forward * _enemy.combat.combatDistance);
        }
        
        // Targeting
        private void FollowTarget()
        {
            Vector3 dir = (_enemy.targetObject.transform.position - _enemy.mTransform.position).normalized;
            _enemy.mTransform.rotation = Quaternion.Lerp(_enemy.mTransform.rotation, Quaternion.LookRotation(dir), _enemy.fixedDelta * _enemy.lookTargetSpeed);
        }
    }
}