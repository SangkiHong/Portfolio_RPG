using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using SK.FSM;
using SK.Utilities;
using Random = UnityEngine.Random;

namespace SK
{
    #region RequireComponent
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(SearchRadar))]
    [RequireComponent(typeof(Behavior.Combat))]
    [RequireComponent(typeof(Dodge))]
    [RequireComponent(typeof(Health))]
    #endregion
    public abstract class Enemy : Unit, ITargetable
    {
        // 유닛 상태 디버그용 스트링 변수
        [ReadOnly] public string currentStateName;
        // 넉백 시 호출될 이벤트
        internal UnityAction<Transform, float, float> OnKnockBackState;

        #region Variables
        #region Stats
        [Header("Stats")]
        public bool isPatrol;
        public Data.EnemyData enemyData;
        [SerializeField] private float attackCooldown = 2.5f;
        [SerializeField] private float lookTargetSpeed = 20f;
        #endregion
        
        #region Combat
        [Header("Attack")]
        [SerializeField] internal Behavior.Attack[] normalAttacks;
        [Header("Flee")]
        [Range(0, 100)]
        [SerializeField] private int fleeHpPercent;
        [Range(0, 100)]
        [SerializeField] private int fleeChance;
        [Range(10, 100)]
        [SerializeField] internal float fleeDistance;
        [Header("Alert")]
        [SerializeField] internal bool canAlert;
        [SerializeField] private float alertRange;
        private Behavior.Alert _alert; // 주변 적들에게 전투 상황을 알려주는 기능의 클래스
        #endregion

        #region Reference
        [Header("Reference")]
        [SerializeField] internal NavMeshAgent navAgent;
        [SerializeField] internal SearchRadar searchRadar;
        [SerializeField] internal EquipmentHolderManager equipmentManager;
        [SerializeField] internal Dodge dodge;
        #endregion

        #region Property
        public float AttackCooldown => attackCooldown;
        public float LookTargetSpeed => lookTargetSpeed;
        #endregion

        #region etc
        public Transform targetingPoint;

        internal Rigidbody mRigidbody;
        internal Collider mCollider;
        
        internal Player targetState;
        internal EnemyStateMachine stateMachine;

        internal float targetDistance;
        internal float walkAnimSpeed = 0.5f;

        [SerializeField]
        internal bool isDead, isFlee, isInteracting, uninterruptibleState;
        #endregion
        #endregion

        #region Unity Events
        public override void Awake()
        {
            base.Awake();

            // 레퍼런스 초기화
            mRigidbody = GetComponent<Rigidbody>();
            mCollider = GetComponent<Collider>();
            if (!navAgent) navAgent = GetComponent<NavMeshAgent>();
            if (!searchRadar) searchRadar = GetComponent<SearchRadar>();
            if (!dodge) dodge = GetComponent<Dodge>();

            // 상태 머신 초기화
            stateMachine = new EnemyStateMachine(this);

            // 경보 알림 가능 시 클래스 생성
            if (canAlert && _alert == null)
            {
                // 0이면 전투 가능 범위와 동일하게 초기화
                if (alertRange == 0)
                    alertRange = combat.combatDistance;

                _alert = new Behavior.Alert(gameObject, mTransform, alertRange);
            }
            // 유닛 정보 초기화
            combat.SetUnitInfo(this, enemyData);
        }

        public override void OnEnable()
        {
            base.OnEnable();

            isFlee = false;
            if (!health.enabled) health.enabled = true;
            if (!mCollider.enabled) mCollider.enabled = true;
            health.Init(enemyData.Hp); // Initialize Max Hp
        }

        private void Start()
        {
            stateMachine.ChangeState(stateMachine.statePatrol); // 기본 상태로 변경
        }

        public override void Tick()
        {
            if (isDead) return;
            base.Tick();

            deltaTime = Time.deltaTime;

            isInteracting = anim.GetBool(Strings.animPara_isInteracting);

            stateMachine.CurrentState?.Tick();
        }

        public override void FixedTick()
        {
            if (isDead) return;
            fixedDeltaTime = Time.fixedDeltaTime;
            stateMachine.CurrentState?.FixedTick();

            // Target Check
            if (combat.Target && TargetCheck())
            {
                // 타겟의 위치 값, 트랜스폼의 위치 값을 변수에 저장

                // 타겟과의 거리 업데이트하여 변수에 저장
                targetDistance = MyMath.Instance.GetDistance(mTransform.position, combat.Target.transform.position);
            }

            // Nav Control
            if (isInteracting && !navAgent.isStopped) navAgent.isStopped = true;
            else if (!isInteracting && navAgent.isStopped) navAgent.isStopped = false;
        }
        #endregion

        #region About Target Method        
        private bool TargetCheck()
        {
            if (targetState != null && targetState.isDead)
            {
                UnassignTarget();
                return false;
            }
            return true;
        }

        internal void UnassignTarget()
        {
            combat.SetTarget(null);
            anim.SetBool(Strings.AnimPara_onCombat, false);
            stateMachine.ChangeState(stateMachine.statePatrol);
        }
        #endregion

        #region Public Function
        public void GetAlert(GameObject target)
        {
            combat.SetTarget(target);
            stateMachine.ChangeState(stateMachine.stateCombat);
        }
        #endregion

        #region Event Function
        public void AbleCombo() { }

        public override void OnDamage(Unit attacker, uint damage, bool isStrong)
        {
            if (isDead) return;

            // 전투 타겟 동기화
            if (!combat.Target) combat.SetTarget(attacker.gameObject);

            // 경계 알림이 가능하면 범위 내 유닛들에게 타겟 전달하는 함수 호출
            if (canAlert) _alert.SendAlert(combat.Target);

            // 회피 기동 판정
            if (dodge && !uninterruptibleState && dodge.dodgeChance > 0 && Random.value < dodge.dodgeChance)
            {
                if (dodge.DoDodge())
                {
                    health.CanDamage = false; // 회피 시 데미지 판정 없기 때문에 False
                    return;
                }
            }

            // 피해량 계산 함수 호출 후 데미지 함수 호출
            health.OnDamage(damage, combat.isCriticalHit);

            // HP가 0이거나 이하로 내려간 경우 즉시 리턴
            if (health.CurrentHp <= 0) return;

            // Uninterruptible 상태에서 움직임 정지 없이 리턴
            if (uninterruptibleState) return;

            // 피격에 의한 움직임 일시 정지
            navAgent.velocity = Vector3.zero;
            navAgent.isStopped = true;
            anim.SetBool(Strings.animPara_isInteracting, true);

            // 약 공격 피해
            if (!isStrong)
                anim.SetTrigger(Strings.AnimPara_Damaged);
            else // 강 공격 피해
            {
                anim.SetTrigger(Strings.AnimPara_StrongDamaged);

                // 넉백 효과
                OnKnockBackState?.Invoke(attacker.transform, 0.25f, 0.1f);
            }
            
            // fleeHpPercent 아래로 Hp가 내려갔을 시 일정 확률에 따라 도주 상태로 변경 
            // fleeHpPercent가 0이면 도주 시도 없음
            if (!isFlee && fleeChance > 0 && health.CurrentHp <= enemyData.Hp * fleeHpPercent * 0.01f)
            {
                isFlee = true; // 1회만 도주 시도
                if (Random.value * 100 < fleeChance)
                {
                    stateMachine.ChangeState(stateMachine.stateFlee);
                    return;
                }
            }

            // 전투 상태 또는 공격 상태가 아닌 경우 전투 상태로 전환
            if (stateMachine.CurrentState != stateMachine.stateCombat && 
                stateMachine.CurrentState != stateMachine.stateAttack)
                stateMachine.ChangeState(stateMachine.stateCombat);
        }

        public override void OnDead()
        {
            isDead = true;
            gameObject.layer = 0;
            health.enabled = false;
            mRigidbody.isKinematic = true;
            mCollider.isTrigger = true;
            navAgent.velocity = Vector3.zero;
            navAgent.isStopped = true;
            stateMachine.StopMachine(true);
            UnassignTarget();
            anim.Rebind();
            anim.SetTrigger(Strings.AnimPara_Dead);
            health.PlayDeadFx();
        }
        #endregion
    }
}