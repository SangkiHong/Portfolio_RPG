using System;
using UnityEngine;
using UnityEngine.AI;

namespace SK
{
    [RequireComponent(typeof(Enemy))]
    public sealed class Dodge : MonoBehaviour
    {
        [Header("Dodge")]
        [Range(0, 1)] 
        public float dodgeChance = 0.3f;
        [Range(0, 180)] 
        [SerializeField] private float dodgeAngle = 30;
        [SerializeField] private float dodgeDistance = 5f;
        [SerializeField] private float dodgeTime = 1f;
        
        [Header("Counter Attack")]
        [Range(0, 1)]
        [SerializeField] private float CAChance = 0.3f;
        [SerializeField] private float CAJumpSpeed = 1.5f;
        
        private Enemy _enemy;
        private Transform _transform;
        private NavMeshHit _navHit;
        private Vector3 _startPos, _tempPos, _destPos;
        
        [NonSerialized] public bool isDodge;
        private float _timer;
        private bool _counterAttack, _canDodge = true;

        private void Awake()
        {
            if (!_enemy) _enemy = GetComponent<Enemy>();
            _transform = GetComponent<Transform>();
        }

        private void Update()
        {
            if (isDodge)
                Move();

            if (_counterAttack)
            {
                AttackJump();
                
                if (!_enemy.isInteracting)
                {
                    _canDodge = true;
                    _counterAttack = false;
                }
            }
        }

        public bool DoDodge()
        {
            if (!_canDodge) return false;
            if (!_enemy.targetObject) return false;
            
            LookAtTarget();
            
            // NavMesh의 길이 있는지 파악 후 위치로 닷지
            if (NavMesh.SamplePosition(GetDodgePoint(dodgeAngle), out _navHit, dodgeDistance, NavMesh.AllAreas))
            {
                _enemy.NavAgent.isStopped = true;
                _timer = 0;
                _startPos = _transform.position;
                _enemy.Anim.SetBool(Strings.animPara_isInteracting, true);
                _enemy.Anim.CrossFade(Strings.AnimName_RollBack, 0.2f);
                isDodge = true;
                return true;
            }
            return false;
        }

        #region Move & CounterAttack
        private void Move()
        {
            _timer += Time.deltaTime;
            _tempPos.x = EasingFunction.EaseOutCubic(_startPos.x, _navHit.position.x, _timer / dodgeTime);
            _tempPos.y = EasingFunction.EaseOutCubic(_startPos.y, _navHit.position.y, _timer / dodgeTime);
            _tempPos.z = EasingFunction.EaseOutCubic(_startPos.z, _navHit.position.z, _timer / dodgeTime);
            _transform.position = _tempPos;
            
            // 이동 완료
            if (_timer >= dodgeTime)
            {
                isDodge = false;
                _enemy.NavAgent.velocity = Vector3.zero;
                _enemy.NavAgent.Warp(_transform.position);
                _enemy.NavAgent.isStopped = false;

                // 이동 완료 후 타격 반응 가능
                _enemy.health.canDamage = true;

                // 닷지 후 반격
                CounterAttack();
            }
        }

        private void CounterAttack()
        {
            if (UnityEngine.Random.value < CAChance)
            {
                _enemy.Anim.SetBool(Strings.animPara_isInteracting, true);
                _enemy.Anim.SetInteger(Strings.AnimPara_ComboIndex, 10); // Jump Attack Index : 10
                _enemy.Anim.SetTrigger(Strings.AnimPara_Attack);
                _enemy.stateMachine.ChangeState(_enemy.stateAttack);
                    
                _timer = 0;
                _startPos = _transform.position;
                _destPos = _enemy.targetObject.transform.position;
                _canDodge = false;
                _counterAttack = true;
                LookAtTarget();
            }
        }

        private void AttackJump()
        {
            _timer += Time.deltaTime;
            _transform.position = Vector3.Slerp(_startPos, _destPos, _timer * CAJumpSpeed);
        }
        #endregion

        #region Utility
        private Vector3 GetDodgePoint(float angle)
        {
            float randomVal = UnityEngine.Random.Range(-1f, 1f); // 반원에서 방향을 랜덤 값으로 정함
            angle = randomVal * angle; // 0 ~ angle 사이의 각을 구함

            return _transform.position + (_transform.rotation * Quaternion.Euler(0, angle, 0)) * (Vector3.forward * -dodgeDistance);
        }

        private void LookAtTarget()
        {
            _transform.rotation = Quaternion.LookRotation(_enemy.targetObject.transform.position - _transform.position);
        }
        #endregion
    }
}
