using System;
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
        public string currentStateName;
        
        #region Stats
        [Header("Stats")]
        public Data.EnemyData enemyData;
        public float attackCooldown = 2.5f;
        public float lookTargetSpeed = 20f;
        #endregion
        
        #region Flee
        [Header("Flee")]
        [Range(0, 100)]
        [SerializeField] private int fleeHpPercent;
        [Range(0, 100)]
        [SerializeField] private int fleeChance;
        #endregion
        
        #region Reference
        [Header("Reference")]
        [SerializeField] private Animator anim;
        [SerializeField] private NavMeshAgent navAgent;
        [SerializeField] internal SearchRadar searchRadar;
        [SerializeField] internal Behavior.Combat combat;
        [SerializeField] internal Dodge dodge;
        [SerializeField] internal Health health;
        #endregion

        #region Property
        public Animator Anim => anim;
        public NavMeshAgent NavAgent => navAgent;
        #endregion

        #region NonSerialized
        [NonSerialized] public float walkAnimSpeed = 0.5f;
        #endregion
        
        #region etc
        internal Transform mTransform;
        internal Rigidbody mRigidbody;
        internal Collider mCollider;
        
        public EnemyStateMachine stateMachine;
        
        public EnemyState statePatrol;
        public EnemyState stateChase;
        public EnemyState stateCombat;
        public EnemyState stateAttack;
        public EnemyState stateFlee;

        [NonSerialized] public GameObject targetObject;
        
        internal float delta, fixedDelta, _comboTimer;
        internal bool isDead, isFlee, isInteracting;
        private int defaultLayer;
        #endregion

        #region Unity Events
        private void Awake()
        {
            mTransform = transform;
            defaultLayer = gameObject.layer;
            if (!anim) anim = GetComponent<Animator>();
            if (!navAgent) navAgent = GetComponent<NavMeshAgent>();
            if (!mRigidbody) mRigidbody = GetComponent<Rigidbody>();
            if (!mCollider) mCollider = GetComponent<Collider>();
            if (!combat) combat = GetComponent<Behavior.Combat>();
            if (!health) health = GetComponent<Health>();
            
            stateMachine = new EnemyStateMachine(this);
            statePatrol = new StatePatrol(this);
            stateChase = new StateChase(this);
            stateCombat = new StateCombat(this);
            stateAttack = new StateAttack(this);
            stateFlee = new StateFlee(this);
        }

        private void OnEnable()
        {
            Init();
        }

        private void OnDisable()
        {
            // event 해제
            health.onDamaged -= OnDamageEvent;
            health.onDead -= OnDeadEvent;
            if (combat) combat.onAttack -= CalculateDamage;
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

            // Attack Combo Timer
            if (combat && combat.canComboAttack)
            {
                if (_comboTimer > 0)
                    _comboTimer -= fixedDelta;
                else
                    combat.canComboAttack = false;
            }
        }
        
        public virtual void LateUpdate()
        {
            if (isDead) return;

            stateMachine.CurrentState?.LateTick();
        }
        #endregion

        public virtual void Init()
        {
            isFlee = false;
            if (!health.enabled) health.enabled = true;
            if (!mCollider.enabled) mCollider.enabled = true;
            health.currentHp = enemyData.Hp; // 최대 Hp 불러오기

            health.onDamaged += OnDamageEvent; // 피해 입을 시 event등록
            health.onDead += OnDeadEvent; // 죽을 시 event등록

            if (combat) combat.onAttack += CalculateDamage; // 공격 시 공격력 계산 event등록
        }

        public void OnAttack()
        {
            stateMachine.ChangeState(stateAttack);
            if (combat) combat.ExecuteAttack();
        }

        #region Event Func
        public void AbleCombo() // Animation Event
        {
            combat.canComboAttack = true;
            _comboTimer = combat.canComboDuration;
        }

        private void OnDamageEvent()
        {
            // Synchronize Hit Object to Target
            if (!targetObject) targetObject = health.hitTransform.gameObject;

            // Dodge
            if (dodge && dodge.dodgeChance > 0 && Random.value < dodge.dodgeChance)
            {
                if (dodge.DoDodge())
                {
                    health.canDamage = false; // 회피 시 데미지 판정 없기 떄문에 False
                    return;
                }
            }

            navAgent.velocity = Vector3.zero;
            navAgent.isStopped = true;
            anim.SetTrigger(Strings.AnimPara_Damaged);
            anim.SetBool(Strings.animPara_isInteracting, true);
            health.Damaged();
            
            // fleeHpPercent 아래로 Hp가 내려갔을 시 일정 확률에 따라 도주 상태로 변경 
            // fleeHpPercent가 0이면 도주 시도 없음
            if (!isFlee && fleeChance > 0 && health.currentHp <= enemyData.Hp * fleeHpPercent * 0.01f)
            {
                isFlee = true; // 1회만 시도
                if (Random.value * 100 < fleeChance)
                {
                    stateMachine.ChangeState(stateFlee);
                    return;
                }
            }
            
            if (stateMachine.CurrentState == statePatrol || stateMachine.CurrentState == stateFlee)
                stateMachine.ChangeState(stateChase); // 피해를 입을 시 상태 변경
        }

        private void OnDeadEvent()
        {
            isDead = true;
            gameObject.layer = 0;
            health.enabled = false;
            mRigidbody.isKinematic = true;
            mCollider.isTrigger = true;
            anim.Rebind();
            anim.SetTrigger(Strings.AnimPara_Dead);
            stateMachine.OnIdleness();
        }

        private void CalculateDamage()
        {
            // Calculate Damage
            var level = enemyData.Level;
            int weaponPower = Random.Range(combat.currentUseWeapon.attackMinPower, combat.currentUseWeapon.attackMaxPower + 1);
            var damage = (level * 0.5f) + (enemyData.Str * 0.5f) + (weaponPower * 0.5f) + (level + 9);

            // Critical Chance
            if (Random.value < enemyData.CriticalChance)
            { 
                damage *= enemyData.CriticalMultiplier;
                combat.isCriticalHit = true;
            }

            combat.calculatedDamage = (int)damage;
        }
        #endregion
    }
}