using UnityEngine;
using UnityEngine.AI;

namespace SK.FSM
{
    public class StateCombat : StateBase
    {
        private readonly Enemy _enemy;
        private readonly EnemyStateMachine _stateMachine;

        internal Vector3 moveDirection;

        private Vector3 _movePoint;
        private float _combatDist, _attackDist, _attackElapsed;

        public StateCombat(Enemy enemyControl, EnemyStateMachine stateMachine)
        {
            _enemy = enemyControl;
            _stateMachine = stateMachine;
            _combatDist = enemyControl.combat.combatDistance;
            _attackDist = enemyControl.combat.attackDistance;
        }

        public override void StateInit()
        {
            // Init Enemy State
            _enemy.uninterruptibleState = false;

            _attackElapsed = _enemy.AttackCooldown;
            _enemy.anim.SetBool(Strings.AnimPara_isFight, true);

            _enemy.navAgent.Warp(_enemy.mTransform.position);
            _enemy.navAgent.isStopped = false;
            _enemy.navAgent.updatePosition = true;
            _enemy.navAgent.updateRotation = false;
            _enemy.navAgent.angularSpeed = 0;

            // 30% 속도로 이동
            _enemy.navAgent.speed = _enemy.enemyData.Speed * 0.3f; 
            _enemy.walkAnimSpeed = 0.3f;

            // 어택 콤보 시작 시에 위치 이동
            if ((_enemy.combat.currentUseWeapon && _enemy.combat.currentUseWeapon.CurrentAttackIndex == 0) ||
                _enemy.combat.primaryEquipment && _enemy.combat.primaryEquipment.CurrentAttackIndex == 0)
                _enemy.navAgent.SetDestination(GetAroundPosition());

            // Assign Check
            if (_enemy.targetState && _enemy.targetState.gameObject.Equals(_enemy.combat.TargetObject))
                return;

            // Target Assign
            _enemy.targetState = _enemy.combat.TargetObject.GetComponent<PlayerStateManager>();
        }

        public override void FixedTick()
        {
            if (_enemy.isInteracting)
                return;

            RotateToTarget();

            // 타겟이 공격범위 밖으로 벗어난 경우 추적 상태(Chase State)로 전환
            if (_enemy.targetDistance > _combatDist + 0.5f)
            {
                _enemy.stateMachine.ChangeState(_stateMachine.stateChase);
                return;
            }

            // Attack Cooldown
            if (_attackElapsed > 0)
            {
                _attackElapsed -= _enemy.fixedDelta;
            }
            else // Do Attack
            {
                if (!_enemy.isInteracting && _enemy.navAgent.velocity.sqrMagnitude <= 0.03f)
                {
                    _attackElapsed = _enemy.AttackCooldown;

                    // Can't be Stop Attack Action
                    _enemy.uninterruptibleState = true;
                    _enemy.stateMachine.ChangeState(_stateMachine.stateAttack);

                    if (_enemy.combat) _enemy.combat.ExecuteAttack();
                }
            }

            Debug.DrawRay(_movePoint, Vector3.up * 3, Color.red);
            Debug.DrawLine(_enemy.combat.TargetObject.transform.position, _movePoint, Color.yellow);
        }

        public void ResetElapsed() => _attackElapsed = _enemy.AttackCooldown; 
        
        // 타겟 중심으로 180도 반경 내 위치 찾기
        private Vector3 GetAroundPosition()
        {
            if (NavMesh.SamplePosition(GetAroundPosition(90), out var navMeshHit, 1, NavMesh.AllAreas))
            {
                _movePoint = navMeshHit.position;
                moveDirection = _movePoint - _enemy.mTransform.position;
                moveDirection.y = Vector3.SignedAngle(moveDirection, _enemy.mTransform.forward, Vector3.up); // y값에 각도 전달
                return navMeshHit.position;
            }            

            return _enemy.mTransform.position;
        }
        
        private Vector3 GetAroundPosition(float degree)
        {
            float randomVal = Random.Range(-1f, 1f); // 반원에서 방향을 랜덤 값으로 정함
            degree = randomVal * degree * 0.5f; // -degree ~ degree 사이의 각을 구함

            var dir = (_enemy.mTransform.position - _enemy.combat.TargetObject.transform.position).normalized;

            return _enemy.combat.TargetObject.transform.position + Quaternion.Euler(0, degree, 0) * (dir * _enemy.combat.attackDistance * 0.9f);
        }
        
        private void RotateToTarget()
        {
            Vector3 dir = (_enemy.combat.TargetObject.transform.position - _enemy.mTransform.position).normalized;
            dir.y = 0;
            _enemy.mTransform.rotation = Quaternion.Lerp(_enemy.mTransform.rotation, Quaternion.LookRotation(dir), _enemy.fixedDelta * _enemy.LookTargetSpeed);
        }
    }
}