using UnityEngine;
using System.Linq;
using Cinemachine;
using SK.Behavior;

namespace SK.FSM
{
    public class PlayerStateManager : MonoBehaviour
    {
        [SerializeField] private bool isDebugMode;

        public Data.PlayerData playerData;

        [Header("References")]
        public CameraManager cameraManager;
        public CinemachineImpulseSource impulseSource;
        public Animator anim;
        public CharacterController characterController;
        public CapsuleCollider thisCollider;
        public AnimatorHook animHook;
        public Combat combat;
        public Health health;

        [Header("States")] 
        public bool useRootMotion;
        internal bool isDead, isGrounded, isRunning, isJumping, isDodge, isSlipping, isTargeting;

        [Header("Targeting")]
        [SerializeField] private Transform cameraTarget;
        [SerializeField] private float targetSearchRange = 20;
        [SerializeField] private LayerMask targetLayer;
        internal Transform targetEnemy;
        private Collider[] _targetColliders;

        [Header("Movement States")]
        public LayerMask groundLayerMask;
        [SerializeField] private float groundDistance;
        public float frontRayOffset = 1;
        public float frontRayOffsetHeight = 1;
        public float movementsSpeed = 5;
        public float runSpeed = 10;
        public float adaptSpeed = 8;
        public float rotationSpeed = 6;
        public float gravity = 5;
        public float jumpForce = 12;
        public float jumpDuration = 10;
        public float jumpIntervalDelay = 2;
        public float slopeLimitAngle = 60;

        [Header("Dodge")]
        [SerializeField] internal float dodgeSpeed = 1;
        [SerializeField] internal AnimationCurve animationCurve_Forward;
        [SerializeField] internal AnimationCurve animationCurve_back;

        //Controller Values
        internal float horizontal;
        internal float vertical;
        internal float moveAmount;

        // States
        internal PlayerStateMachine stateMachine;

        // States Actions
        internal InputManager inputManager;
        internal MoveCharacter moveCharacter;
        internal MonitorAnimationBool monitorInteracting;

        internal Transform mTransform;

        private PlayerInputAction _playerInputAction;
        private Collider[] _groundCheckCols = new Collider[3];

        internal bool canComboAttack;
        internal float delta, fixedDelta;
        private float _comboTimer, _scrollY;
        private int _environmentLayer;

        #region Unity Events
        private void Awake()
        {
            mTransform = this.transform;
            GameManager.Instance.player = this;

            // Initialize Camera
            if (!cameraManager) cameraManager = Camera.main.GetComponent<CameraManager>();            
            if (cameraManager) cameraManager.Init(cameraTarget);

            // Initialize References
            if (!anim) anim = GetComponent<Animator>();
            if (!animHook) animHook = GetComponentInChildren<AnimatorHook>();
            if (!impulseSource) impulseSource = GetComponent<CinemachineImpulseSource>();
            if (!combat) combat = GetComponent<Combat>();
            if (!health) health = GetComponent<Health>();

            // Initialize Input System
            _playerInputAction = new PlayerInputAction();
            _playerInputAction.Enable();

            // Initialize State Machine
            stateMachine = new PlayerStateMachine(this);

            // Initialize State Actions
            inputManager = new InputManager(this, _playerInputAction);
            moveCharacter = new MoveCharacter(this);
            monitorInteracting = new MonitorAnimationBool(this, Strings.animPara_isInteracting, stateMachine.locomotionState);

            // Initialize Heath
            health.Init(playerData.Level, playerData.Str, playerData.Dex, playerData.Int);

            // Initialize Camera Settings
            cameraManager.normalCamera.m_Lens.FieldOfView = PlayerPrefs.GetFloat("ZoomAmount", 0);
            _playerInputAction.GamePlay.CameraZoom.performed += x => _scrollY = x.ReadValue<float>() * cameraManager.cameraZoomSpeed * -1;

            anim.applyRootMotion = false;
            if (animHook) animHook.Init(this);
            _targetColliders = new Collider[5];

            // Init Variable
            _environmentLayer = LayerMask.NameToLayer("Environment");

        }

        internal virtual void OnEnable()
        {
            // Event 등록
            health.onDamaged += OnDamageEvent;
            health.onDead += OnDeadEvent;
            if (combat) combat.onAttack += CalculateDamage;
        }

        private void FixedUpdate()
        {
            if (isDead) return;

            fixedDelta = Time.fixedDeltaTime;

            if (stateMachine.isAssigned())
                stateMachine.CurrentState.FixedTick();

            // Ground Check
            isGrounded = IsCheckGrounded();

            // Camera Zoom Control
            if (!isTargeting)
                cameraManager.ZoomSetting(_scrollY);            

            // Attack Combo Timer
            if (canComboAttack)
            {
                if (_comboTimer > 0)
                    _comboTimer -= fixedDelta;
                else
                    canComboAttack = false;
            }

            // 
            if (UnityEngine.InputSystem.Keyboard.current.leftCtrlKey.wasPressedThisFrame)
                cameraManager.CameraRotateSwtich(GameManager.Instance.SwitchMouseState());
        }

        private void Update()
        {
            if (isDead) return;

            delta = Time.deltaTime;

            if (stateMachine.isAssigned())
                stateMachine.CurrentState.Tick();
        }

        private void LateUpdate()
        {
            if (isDead) return;

            if (stateMachine.isAssigned())
                stateMachine.CurrentState.LateTick();
        }

        private void OnDisable() 
        {
            _playerInputAction.Disable();

            // Event 해제
            health.onDamaged -= OnDamageEvent;
            health.onDead -= OnDeadEvent;
            if (combat) combat.onAttack -= CalculateDamage;
        }

        private void OnApplicationQuit()
        {
            // 줌 값 저장하기
            PlayerPrefs.SetFloat("ZoomAmount", cameraManager.normalCamera.m_Lens.FieldOfView);
        }
        #endregion

        #region Animation
        public void PlayerTargetAnimation(string targetAnim, bool isInteracting)
        {
            anim.SetBool(Strings.animPara_isInteracting, isInteracting);
            anim.CrossFade(targetAnim, 0.15f);
        }
        #endregion

        #region Lock System
        public void OnAssignLookOverride(Transform lockTarget)
        {
            if (lockTarget == null) return;
            targetEnemy = lockTarget;

            isTargeting = true;
            cameraManager.OnAssignLookOverride(lockTarget);
        }

        public void OnClearLookOverride()
        {
            isTargeting = false;
            targetEnemy = null;
            cameraManager.OnClearLookOverride();
        }

        public Transform FindLockableTarget()
        {
            if (Physics.OverlapSphereNonAlloc(mTransform.position, targetSearchRange, _targetColliders, targetLayer) > 0)
            {
                _targetColliders
                    .Where(x => Vector3.Distance(mTransform.position, x.transform.position) < 30)
                    .OrderBy(x => Vector3.Distance(mTransform.position, x.transform.position));

                if (_targetColliders.Length > 0)
                    return _targetColliders[0].transform;
            }
            
            return null;
        }
        #endregion

        #region State Events
        internal void EnableRootMotion()
        {
            useRootMotion = true;
        }
        internal void DisableRootMotion()
        {
            useRootMotion = false;
        }
        #endregion

        #region Event Func
        public void AbleCombo() // Combo 가능 Animation Event
        {
            canComboAttack = true;
            _comboTimer = combat.canComboDuration;
        }

        private void OnDamageEvent()
        {
            // Shield하고 있을 경우
            if (anim.GetBool(Strings.AnimPara_isShielding))
            { 
                anim.CrossFade(Strings.AnimName_Shield_Hit, 0);
                return;
            }

            impulseSource.GenerateImpulse(10f);
            anim.SetTrigger(Strings.AnimPara_Damaged);
            anim.SetBool(Strings.animPara_isInteracting, true);

            // Debug 시 데미지 받지 않음
            if (isDebugMode) return;

            health.Damaged();
        }

        private void OnDeadEvent()
        {
            isDead = true;
            gameObject.layer = 0;
            health.enabled = false;
            //thisRigidbody.isKinematic = true; //deprecated::Don't use Rigidbody
            thisCollider.isTrigger = true;

            combat.SetTarget(null);
            cameraManager.OnClearLookOverride();

            anim.Rebind();
            anim.SetTrigger(Strings.AnimPara_Dead);
        }

        private void CalculateDamage()
        {
            // Calculate Damage

            combat.calculatedDamage = 
                combat.CalculateDamage(playerData.Level, playerData.Str, playerData.CriticalChance, playerData.CriticalMultiplier);
        }
        #endregion

        #region Collision Check
        private void OnCollisionStay(Collision collision)
        {
            if (collision.gameObject.layer == _environmentLayer)
            {
                //thisRigidbody.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;
            }
        }

        private void OnCollisionExit(Collision collision)
        {
            if (collision.gameObject.layer == _environmentLayer)
            {
                //thisRigidbody.constraints = RigidbodyConstraints.FreezeAll;
            }
        }

        private bool IsCheckGrounded()
        {
            var position = mTransform.position;

            // 1차 Character Controller로 체크
            if (characterController.isGrounded) return true;

            // 2차 OverlapSphere로 체크
            var size = thisCollider.bounds.size.x * 0.5f;
            if (0 < Physics.OverlapSphereNonAlloc(position + Vector3.up * size, size, _groundCheckCols, groundLayerMask))
            {
                isGrounded = true;
                return true;
            }

            // 3차 Ray로 체크
            var ray = new Ray(position + Vector3.up * 0.1f, Vector3.down);
            Debug.DrawRay(position + Vector3.up * 0.1f, Vector3.down * groundDistance, Color.magenta);
            if (Physics.Raycast(ray, groundDistance, groundLayerMask))
            {
                isGrounded = true;
                return true;
            }

            return isGrounded = false;
        }

        void OnDrawGizmosSelected()
        {
            if (!isGrounded)
            {
                var size = thisCollider.bounds.size.x * 0.5f;
                Gizmos.DrawSphere(transform.position + Vector3.up * size, size);
            }
        }
        #endregion
    }
}