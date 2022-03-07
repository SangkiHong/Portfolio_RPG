using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using SK.FSM;

namespace SK
{
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(Animator))]
    public abstract class Enemy : MonoBehaviour
    {
        #region Event
        public event UnityAction OnDied;
        #endregion
        
        #region Stats
        [Header("Stats")]
        public EnemyData enemyData;

        public int currentHP; 
        public float LookTargetSpeed = 1f;
        [SerializeField]
        private float blinkTime = 0.5f;
        [SerializeField]
        private float navMeshLinkSpeed = 0.5f;
        #endregion

        #region Attack
        [Header("Attack")]
        public float AttackCooldown = 2.5f;
        public Vector3 AttackColOffset;
        public Vector3 AttackColScale;
        public int AttackColSize;
        public float AttackColInteval;
        [SerializeField]
        private float attackStepsize = 0.5f;
        [SerializeField]
        private float dodgeChance = 0.3f;
        [SerializeField]
        private float dodgeAngle = 30;
        [SerializeField]
        private float dodgeDistance = 5f;
        [SerializeField]
        private float counterattackChance = 0.3f;
        #endregion
        
        #region Reference
        [Header("Reference")]
        [SerializeField]
        private Animator anim;
        [SerializeField]
        private NavMeshAgent navAgent;
        [SerializeField] 
        internal SearchRadar searchRadar;
        #endregion

        #region Property
        public Animator Anim => anim;
        public NavMeshAgent NavAgent => navAgent;
        #endregion

        #region Animation Parameter
        [NonSerialized] 
        public int AnimPara_isFight, AnimPara_isInteracting,
                   AnimPara_MoveBlend, AnimPara_Attack;
        #endregion
        
        #region ETC
        internal Transform mTransfrom;
        internal Rigidbody mRigidbody;
        internal Collider mCollider;
        internal EnemyStatsUI enemyHealthBar;
        
        public EnemyStateMachine stateMachine;
        
        public EnemyState statePatrol;
        public EnemyState stateChase;
        public EnemyState stateAttack;
        public EnemyState stateFlee;

        internal bool isDamaged;
        internal float delta, fixedDelta;
        #endregion

        private void Awake()
        {
            mTransfrom = transform;
            if (!anim) anim = GetComponent<Animator>();
            if (!navAgent) navAgent = GetComponent<NavMeshAgent>();
            if (!mRigidbody) mRigidbody = GetComponent<Rigidbody>();
            if (!mCollider) mCollider = GetComponent<Collider>();
            
            stateMachine = new EnemyStateMachine();
            statePatrol = new StatePatrol(this);
            stateChase = new StateChase(this);
            stateAttack = new StateAttack(this);
            stateFlee = new StateFlee(this);
            
            AnimPara_isFight = Animator.StringToHash("isFight");
            AnimPara_isInteracting = Animator.StringToHash("isInteracting");
            AnimPara_MoveBlend = Animator.StringToHash("MoveBlend");
            AnimPara_Attack = Animator.StringToHash("Attack 1");
            currentHP = enemyData.Hp;
        }

        private void Start()
        {
            Init();
        }

        public virtual void Init()
        {
            delta = Time.deltaTime;
            fixedDelta = Time.fixedDeltaTime;
        }

        void OnDrawGizmosSelected()
        {
            if (stateMachine != null && stateMachine.CurrentState == stateAttack)
            {
                Gizmos.matrix = mTransfrom.localToWorldMatrix;
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireCube(AttackColOffset, AttackColScale);
            }
        }
    }
}