using System;
using System.Collections.Generic;
using SK.Behavior;
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
        public string currentStateName;
        
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
        
        #region Reference
        [Header("Reference")]
        [SerializeField]
        private Animator anim;
        [SerializeField]
        private NavMeshAgent navAgent;
        [SerializeField] 
        internal SearchRadar searchRadar;
        [SerializeField] 
        internal Combat combat;
        [SerializeField] 
        internal Dodge dodge;
        #endregion

        #region Property
        public Animator Anim => anim;
        public NavMeshAgent NavAgent => navAgent;
        #endregion

        #region Animation Parameter
        [NonSerialized] 
        
        #endregion
        
        #region ETC
        internal Transform mTransform;
        internal Rigidbody mRigidbody;
        internal Collider mCollider;
        internal EnemyStatsUI enemyHealthBar;
        
        public EnemyStateMachine stateMachine;
        
        public EnemyState statePatrol;
        public EnemyState stateChase;
        public EnemyState stateCombat;
        public EnemyState stateAttack;
        public EnemyState stateFlee;

        internal bool isDamaged;
        internal float delta, fixedDelta;
        #endregion

        private void Awake()
        {
            mTransform = transform;
            if (!anim) anim = GetComponent<Animator>();
            if (!navAgent) navAgent = GetComponent<NavMeshAgent>();
            if (!mRigidbody) mRigidbody = GetComponent<Rigidbody>();
            if (!mCollider) mCollider = GetComponent<Collider>();
            if (!combat) combat = GetComponent<Combat>();
            
            stateMachine = new EnemyStateMachine(this);
            statePatrol = new StatePatrol(this);
            stateChase = new StateChase(this);
            stateCombat = new StateCombat(this);
            stateAttack = new StateAttack(this);
            stateFlee = new StateFlee(this);
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
    }
}