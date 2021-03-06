using System;
using UnityEngine;
using UnityEngine.AI;
using SK.Utilities;

namespace SK
{
    [RequireComponent(typeof(Enemy))]
    public sealed class Dodge : MonoBehaviour
    {
        [Header("Dodge")]
        [Range(0, 1)] 
        public float dodgeChance = 0.3f;
        [Range(0, 180)] 
        [SerializeField] private float dodgeAngle = 30f;
        [SerializeField] private float dodgeDistance = 3f;  
        [SerializeField] private float dodgeTime = 1f;

        [Header("Counter Attack")]
        [SerializeField] private Behavior.Attack counterAttack;
        [Range(0, 1)]
        [SerializeField] private float CAChance = 0.3f;
        [SerializeField] private float CAJumpSpeed = 1f;
        [SerializeField] private float CAJumpForce = 3f;
        
        private Enemy _enemy;
        private Transform _transform;
        private NavMeshHit _navHit;
        private Vector3 _startPos, _tempPos, _destPos, _direction;
        
        [NonSerialized] public bool isDodge;
        private float _elapsed;
        private bool _counterAttack, _canDodge = true;

        private void Awake()
        {
            if (!_enemy) _enemy = GetComponent<Enemy>();
            _transform = GetComponent<Transform>();
        }

        private void Tick()
        {
            if (!_enemy.combat.Target) return;

            if (isDodge)
                Move();

            if (_counterAttack)
            {
                AttackJump();
            }
        }

        public bool DoDodge()
        {
            if (!_canDodge) return false;
            if (!_enemy.combat.Target) return false;
            
            LookAtTarget();
            
            // NavMesh의 길이 있는지 파악 후 위치로 닷지
            if (NavMesh.SamplePosition(GetDodgePoint(dodgeAngle), out _navHit, dodgeDistance, NavMesh.AllAreas))
            {
                _enemy.navAgent.isStopped = true;
                _elapsed = 0;
                _startPos = _transform.position;
                _enemy.anim.SetBool(Strings.animPara_isInteracting, true);
                _enemy.anim.CrossFade(Strings.AnimName_RollBack, 0.2f);
                isDodge = true;
                SceneManager.Instance.OnUpdate += Tick;
                return true;
            }
            return false;
        }

        #region Move & CounterAttack
        private void Move()
        {
            _elapsed += Time.deltaTime;
            _tempPos.x = EasingFunction.EaseOutCubic(_startPos.x, _navHit.position.x, _elapsed / dodgeTime);
            _tempPos.y = EasingFunction.EaseOutCubic(_startPos.y, _navHit.position.y, _elapsed / dodgeTime);
            _tempPos.z = EasingFunction.EaseOutCubic(_startPos.z, _navHit.position.z, _elapsed / dodgeTime);
            _transform.position = _tempPos;
            
            // 이동 완료
            if (_elapsed >= dodgeTime)
            {
                isDodge = false;
                _enemy.navAgent.velocity = Vector3.zero;
                _enemy.navAgent.Warp(_transform.position);
                _enemy.navAgent.isStopped = false;

                // 이동 완료 후 타격 반응 가능
                _enemy.health.SetDamagableState(true);

                // 닷지 후 반격
                if (!CounterAttack())
                    SceneManager.Instance.OnUpdate -= Tick;
            }
        }

        private bool CounterAttack()
        {
            if (UnityEngine.Random.value < CAChance)
            {
                // 방해 받지 않는 상태로 전환
                _enemy.onUninterruptible = true;

                // Nav Agent 작동 정지
                _enemy.navAgent.updatePosition = false;
                _enemy.navAgent.isStopped = true;

                // StateMachine 정지
                _enemy.stateMachine.StopMachine();

                _enemy.combat.BeginAttack(counterAttack);
                    
                _elapsed = 0;
                _startPos = _transform.position;
                
                _canDodge = false;
                _counterAttack = true;

                LookAtTarget();
                return true;
            }

            return false;
        }

        private void AttackJump()
        {
            if (_elapsed < 1)
                _elapsed += Time.deltaTime * CAJumpSpeed; 
            else
                _elapsed = 1;

            if (_elapsed < 0.5f)
            {
                // 최대 높이까지 타겟 위치 계산
                _direction = _transform.position - _enemy.combat.Target.transform.position;
                _direction.y = 0;
                _destPos = _enemy.combat.Target.transform.position + _direction.normalized * _enemy.combat.attackDistance * 0.5f;
            }

            // 삼각함수 Sine을 통해 점프 구현
            _transform.position = Vector3.Slerp(_startPos, _destPos 
                + (Vector3.up * MyMath.Instance.GetSine((uint)Mathf.RoundToInt(_elapsed * 180)) * CAJumpForce), _elapsed);

            // 디버깅::착지 위치
            Debug.DrawRay(_destPos, Vector3.up, Color.red);

            if (_elapsed >= 1)
            {
                _canDodge = true;
                _counterAttack = false;
                _transform.position = _destPos;

                // Nav Agent 작동 재개
                _enemy.navAgent.isStopped = true;
                _enemy.navAgent.Warp(_transform.position);
                _enemy.navAgent.updatePosition = true;

                _enemy.stateMachine.ChangeState(_enemy.stateMachine.stateAttack);
                SceneManager.Instance.OnUpdate -= Tick;
            }
        }
        #endregion

        #region Utility
        private Vector3 GetDodgePoint(float angle)
        {
            float randomVal = UnityEngine.Random.Range(-1f, 1f); // 반원에서 방향을 랜덤 값으로 정함
            angle = randomVal * angle; // 0 ~ angle 사이의 각을 구함

            return _transform.position + _transform.rotation * Quaternion.Euler(0, angle, 0) * (Vector3.forward * -dodgeDistance);
        }

        private void LookAtTarget()
        {
            _transform.rotation = Quaternion.LookRotation(_enemy.combat.Target.transform.position - _transform.position);
        }
        #endregion
    }
}
