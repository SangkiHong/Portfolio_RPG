using UnityEngine;
using UnityEngine.AI;
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
    public abstract class Enemy : MonoBehaviour, ITargetable
    {
        [ReadOnly] public string currentStateName;

        #region Stats
        [Header("Stats")]
        public bool isPatrol;
        public Data.EnemyData enemyData;
        [SerializeField] private float attackCooldown = 2.5f;
        [SerializeField] private float lookTargetSpeed = 20f;
        #endregion
        
        #region Flee
        [Header("Flee")]
        [Range(0, 100)]
        [SerializeField] private int fleeHpPercent;
        [Range(0, 100)]
        [SerializeField] private int fleeChance;
        [Range(10, 100)]
        [SerializeField] internal float fleeDistance;
        #endregion

        #region Reference
        [Header("Reference")]
        [SerializeField] internal Animator anim;
        [SerializeField] internal NavMeshAgent navAgent;
        [SerializeField] internal SearchRadar searchRadar;
        [SerializeField] internal Behavior.Combat combat;
        [SerializeField] internal Dodge dodge;
        [SerializeField] internal Health health;
        #endregion

        #region Property
        public float AttackCooldown => attackCooldown;
        public float LookTargetSpeed => lookTargetSpeed;
        #endregion

        #region etc
        public Transform targetingPoint;

        internal Transform mTransform;
        internal Rigidbody mRigidbody;
        internal Collider mCollider;
        
        internal PlayerStateManager targetState;
        internal EnemyStateMachine stateMachine;

        internal float targetDistance;
        internal float walkAnimSpeed = 0.5f;
        internal float delta, fixedDelta;
        [SerializeField]
        internal bool isDead, isFlee, isInteracting, uninterruptibleState;
        #endregion

        #region Unity Events
        private void Awake()
        {
            mTransform = transform;
            mRigidbody = GetComponent<Rigidbody>();
            mCollider = GetComponent<Collider>();
            if (!anim) anim = GetComponent<Animator>();
            if (!navAgent) navAgent = GetComponent<NavMeshAgent>();
            if (!searchRadar) searchRadar = GetComponent<SearchRadar>();
            if (!combat) combat = GetComponent<Behavior.Combat>();
            if (!dodge) dodge = GetComponent<Dodge>();
            if (!health) health = GetComponent<Health>();
            
            stateMachine = new EnemyStateMachine(this);
        }

        private void OnEnable()
        {
            Init();
        }

        private void Start()
        {
            stateMachine.ChangeState(stateMachine.statePatrol); // 기본 상태로 변경
        }

        public virtual void Update()
        {
            if (isDead) return;
            
            delta = Time.deltaTime;
            stateMachine.CurrentState?.Tick();
        }

        public virtual void FixedUpdate()
        {
            if (isDead) return;

            isInteracting = anim.GetBool(Strings.animPara_isInteracting);
            fixedDelta = Time.fixedDeltaTime;
            stateMachine.CurrentState?.FixedTick();

            // Target Check
            if (combat.TargetObject && TargetCheck())
            {
                // 타겟과의 거리
                targetDistance = Vector3.Distance(combat.TargetObject.transform.position, mTransform.position);
            }

            // Nav Control
            if (isInteracting && !navAgent.isStopped) navAgent.isStopped = true;
            else if (!isInteracting && navAgent.isStopped) navAgent.isStopped = false;
        }
        
        public virtual void LateUpdate()
        {
            if (isDead) return;

            stateMachine.CurrentState?.LateTick();
        }

        private void OnDisable()
        {
            // event 해제
            health.onDamaged -= OnDamageEvent;
            health.onDead -= OnDeadEvent;
            if (combat) combat.onAttack -= CalculateDamage;
        }
        #endregion

        #region Private Internal Function
        private void Init()
        {
            isFlee = false;
            if (!health.enabled) health.enabled = true;
            if (!mCollider.enabled) mCollider.enabled = true;
            health.Init(enemyData.Hp); // Initialize Max Hp

            health.onDamaged += OnDamageEvent; // 피해 입을 시 event등록
            health.onDead += OnDeadEvent; // 죽을 시 event등록

            if (combat) combat.onAttack += CalculateDamage; // 공격 시 공격력 계산 event등록
        }

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
            anim.SetBool(Strings.AnimPara_isFight, false);
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

        #region Event Func
        public void AbleCombo() { }        

        private void OnDamageEvent()
        {
            if (isDead) return;

            // Synchronize Hit Object to Target
            if (!combat.TargetObject) combat.SetTarget(health.hitTransform.gameObject);

            // Alert Alliance
            if (combat.alert)
                combat.alert.SendAlert(combat.TargetObject);

            // Dodge
            if (dodge && !uninterruptibleState && dodge.dodgeChance > 0 && Random.value < dodge.dodgeChance)
            {
                if (dodge.DoDodge())
                {
                    health.canDamage = false; // 회피 시 데미지 판정 없기 떄문에 False
                    return;
                }
            }

            // Take damage
            health.Damaged();

            if (health.CurrentHp <= 0) return;

            // Receive damage but can't be interrupted State
            if (uninterruptibleState) return;

            navAgent.velocity = Vector3.zero;
            navAgent.isStopped = true;
            anim.SetTrigger(Strings.AnimPara_Damaged);
            anim.SetBool(Strings.animPara_isInteracting, true);
            
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
            
            if (stateMachine.CurrentState == null || 
                stateMachine.CurrentState == stateMachine.statePatrol || 
                stateMachine.CurrentState == stateMachine.stateFlee)
                stateMachine.ChangeState(stateMachine.stateChase); // 피해를 입을 시 상태 변경
        }

        private void OnDeadEvent()
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

        private void CalculateDamage()
        {
            // Calculate Damage
            combat.calculatedDamage = 
                combat.CalculateDamage(enemyData.Level, enemyData.Str, enemyData.CriticalChance, enemyData.CriticalMultiplier);
        }
        #endregion
    }
}