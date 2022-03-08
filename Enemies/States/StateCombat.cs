using UnityEngine;
using UnityEngine.AI;

namespace SK.FSM
{
    public class StateCombat : EnemyState
    {
        private readonly Enemy _enemy;

        private float _targetDist, _attackTimer;
        
        public StateCombat(Enemy enemyControl)
        {
            _enemy = enemyControl;
            
            _attackTimer = _enemy.combat.attackCooldown * 0.3f;
        }

        public override void StateInit()
        {
            _enemy.Anim.SetBool(AnimParas.AnimPara_isFight, true);
            _enemy.NavAgent.SetDestination(CombatRandomPos());
        }

        public override void FixedTick()
        {
            if (_enemy.Anim.GetBool(AnimParas.animPara_isInteracting))
                return;
            
            if (!_enemy.NavAgent.isStopped)
            {
                _enemy.NavAgent.isStopped = true;
                //_enemy.NavAgent.updatePosition = false;
                _enemy.NavAgent.updateRotation = false;
            }

            // 타겟이 공격범위 밖으로 벗어난 경우 추적 상태(Chase State)로 전환
            _targetDist = Vector3.Distance(_enemy.mTransform.position, _enemy.searchRadar.targetObject.transform.position);
            if (_targetDist > _enemy.combat.combatDistance)
            {
                _enemy.stateMachine.ChangeState(_enemy.stateChase);
                return;
            }
            
            // Attack Cooldown
            if (0 < _attackTimer && !_enemy.isDamaged)
                _attackTimer -= _enemy.fixedDelta;
            // Do Attack
            else
            {
                if (!_enemy.isDamaged)
                {
                    _attackTimer = _enemy.combat.attackCooldown;
                    _enemy.Anim.SetBool(AnimParas.animPara_isInteracting, true);
                    _enemy.Anim.SetInteger(AnimParas.AnimPara_ComboIndex, Random.Range(0, 3));
                    _enemy.Anim.SetTrigger(AnimParas.AnimPara_Attack);
                    _enemy.stateMachine.ChangeState(_enemy.stateAttack);
                }
            }
            
            FollowTarget();
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

            return _enemy.searchRadar.targetObject.transform.position
                   + _enemy.mTransform.rotation * Quaternion.Euler(0, angle, 0) * (Vector3.forward * _enemy.combat.combatDistance);
        }
        
        // Targeting
        private void FollowTarget()
        {
            Vector3 dir = _enemy.searchRadar.targetObject.transform.position - _enemy.mTransform.position;
            _enemy.mTransform.rotation = Quaternion.Lerp(_enemy.mTransform.rotation, Quaternion.LookRotation(dir), _enemy.delta * _enemy.LookTargetSpeed);
        }
    }
}