using UnityEngine;
using UnityEngine.AI;

namespace SK.FSM
{
    public class EnemyStateCombat : StateBase
    {
        private readonly Enemy _enemy;
        private readonly EnemyStateMachine _stateMachine;

        internal Vector3 moveDirection;

        private Vector3 _movePoint; 
        private Behavior.Attack _currentAttack;

        private int _attackIndex;
        private float _combatDist, _attackDist, _attackElapsed;

        public EnemyStateCombat(Enemy enemyControl, EnemyStateMachine stateMachine)
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
            _enemy.anim.SetBool(Strings.AnimPara_onCombat, true);

            _enemy.navAgent.Warp(_enemy.mTransform.position);
            _enemy.navAgent.isStopped = false;
            _enemy.navAgent.updatePosition = true;
            _enemy.navAgent.updateRotation = false;
            _enemy.navAgent.angularSpeed = 0;

            // 30% 속도로 이동
            _enemy.navAgent.speed = _enemy.enemyData.Speed * 0.3f; 
            _enemy.walkAnimSpeed = 0.3f;

            // 어택 콤보 시작 시에 위치 이동
            if (_attackIndex == 0)
                _enemy.navAgent.SetDestination(GetAroundPosition());

            // 타겟이 할당되었는 지 확인 후 할당되어 있으며 현재 할당된 타겟과 컴뱃 컴포넌트의 타겟이 동일하면 리턴
            if (_enemy.targetState && _enemy.targetState.gameObject.Equals(_enemy.combat.Target))
                return;

            // 컴뱃 컴포넌트의 타겟을 통해 플레이어 컴포넌트를 가져옴
            _enemy.targetState = _enemy.combat.Target.GetComponent<Player>();
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
                _attackElapsed -= _enemy.fixedDeltaTime;
            }
            else // Do Attack
            {
                if (!_enemy.isInteracting && _enemy.navAgent.velocity.sqrMagnitude <= 0.03f)
                {
                    _attackElapsed = _enemy.AttackCooldown;

                    // Can't be Stop Attack Action
                    _enemy.uninterruptibleState = true;
                    _enemy.stateMachine.ChangeState(_stateMachine.stateAttack);

                    if (_enemy.normalAttacks.Length > 0)
                    {
                        // 현재 공격을 변수에 할당
                        _currentAttack = _enemy.normalAttacks[_attackIndex];

                        // 공격 인덱스가 0 ~ 공격 배열 길이 -1 사이의 값을 순환함
                        _attackIndex = (_attackIndex + 1) % _enemy.normalAttacks.Length;

                        // 공격의 애니메이션 이름을 받아 애니메이션 실행하며 트랜지션을 0.2로 고정함
                        _enemy.anim.CrossFade(_currentAttack.animName, 0.2f);

                        _enemy.combat.BeginAttack(_currentAttack);
                    }
                }
            }

            Debug.DrawRay(_movePoint, Vector3.up * 3, Color.red);
            Debug.DrawLine(_enemy.combat.Target.transform.position, _movePoint, Color.yellow);
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

            var dir = (_enemy.mTransform.position - _enemy.combat.Target.transform.position).normalized;

            return _enemy.combat.Target.transform.position + Quaternion.Euler(0, degree, 0) * (dir * _enemy.combat.attackDistance * 0.9f);
        }
        
        private void RotateToTarget()
        {
            Vector3 dir = (_enemy.combat.Target.transform.position - _enemy.mTransform.position).normalized;
            dir.y = 0;
            _enemy.mTransform.rotation = Quaternion.Lerp(_enemy.mTransform.rotation, Quaternion.LookRotation(dir), _enemy.fixedDeltaTime * _enemy.LookTargetSpeed);
        }
    }
}