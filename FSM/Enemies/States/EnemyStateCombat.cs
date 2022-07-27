using UnityEngine;
using UnityEngine.AI;

namespace SK.FSM
{
    public class EnemyStateCombat : StateBase
    {
        private readonly Enemy _enemy;
        private readonly EnemyStateMachine _stateMachine;

        private Behavior.Attack _currentAttack;

        private int _attackIndex;
        private float _attackDist, _combatDist;
        private float _attackElapsed;
        private bool _rearrangePosition;

        private Transform _transform;
        private NavMeshAgent _navAgent;
        private Vector3 _movePoint; // 이동 위치 디버그용 변수
        private Vector3 _moveDir;
        public Vector3 MoveDirection => _moveDir;

        public EnemyStateCombat(Enemy enemyControl, EnemyStateMachine stateMachine)
        {
            _enemy = enemyControl;
            _transform = _enemy.mTransform;
            _navAgent = _enemy.navAgent;
            _stateMachine = stateMachine;
            _attackDist = enemyControl.combat.attackDistance;
            _combatDist = enemyControl.combat.combatDistance + 0.5f;
            // 거리 비교 연산을 위해 제곱
            _attackDist *= _attackDist;
            _combatDist *= _combatDist;
        }

        public override void StateInit()
        {
            // 초기화
            _enemy.onUninterruptible = false;
            _rearrangePosition = false;

            _attackElapsed = _enemy.AttackCooldown;
            _enemy.anim.SetBool(Strings.AnimPara_onCombat, true);

            _navAgent.Warp(_transform.position);
            _navAgent.isStopped = false;
            _navAgent.updatePosition = true;
            _navAgent.updateRotation = false;
            _navAgent.angularSpeed = 0;

            // 30% 속도로 이동
            _navAgent.speed = _enemy.enemyData.Speed * 0.3f; 
            _enemy.walkAnimSpeed = 0.3f;

            // 어택 콤보 시작 시에 위치 이동
            if (_attackIndex == 0)
                _navAgent.SetDestination(GetAroundPosition());

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

            _enemy.RotateToTarget();

            float distance = _enemy.targetDistance;

            // 타겟이 공격범위 밖으로 벗어난 경우 추적 상태(Chase State)로 전환
            if (distance > _combatDist)
            {
                // Hp 회복
                _enemy.health.Recovering();

                _enemy.stateMachine.ChangeState(_stateMachine.stateChase);
                return;
            }
            // 타겟과의 거리가 공격 범위의 20% 이하로 가까워지면 거리를 둠
            else if (distance < _attackDist * 0.2f)
            {
                if (!_rearrangePosition)
                {
                    Debug.Log("Need Rearrange");
                    _rearrangePosition = true;
                    _enemy.walkAnimSpeed = 1f;
                    _navAgent.speed = _enemy.enemyData.Speed;
                    _movePoint = _transform.position - _transform.forward;
                    _navAgent.SetDestination(_movePoint);
                }
            }
            // 전투 범위 내에 타겟이 있는 경우
            else
            {
                if (_rearrangePosition)
                {
                    Debug.Log("Not Need Rearrange");
                    _rearrangePosition = false;
                    _enemy.walkAnimSpeed = 0.3f;
                    _navAgent.speed = _enemy.enemyData.Speed * 0.3f;
                }
            }

            // Attack Cooldown
            if (_attackElapsed > 0)
            {
                _attackElapsed -= _enemy.fixedDeltaTime;
            }
            else // Do Attack
            {
                if (!_enemy.isInteracting && _navAgent.velocity.sqrMagnitude <= 0.03f)
                {
                    _attackElapsed = _enemy.AttackCooldown;

                    // Can't be Stop Attack Action
                    _enemy.onUninterruptible = true;
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
            if (NavMesh.SamplePosition(GetAnglePosition(90), out var navMeshHit, 1, NavMesh.AllAreas))
            {
                _movePoint = navMeshHit.position;
                _moveDir = _movePoint - _transform.position;
                _moveDir.y = Vector3.SignedAngle(MoveDirection, _transform.forward, Vector3.up); // y값에 각도 전달
                return navMeshHit.position;
            }            

            return _transform.position;
        }
        
        private Vector3 GetAnglePosition(float degree)
        {
            float randomVal = Random.Range(-1f, 1f); // 반원에서 방향을 랜덤 값으로 정함
            degree = randomVal * degree * 0.5f; // -degree ~ degree 사이의 각을 구함

            var dir = (_transform.position - _enemy.combat.Target.transform.position).normalized;

            return _enemy.combat.Target.transform.position + Quaternion.Euler(0, degree, 0) * (dir * _enemy.combat.attackDistance * 0.9f);
        }
    }
}