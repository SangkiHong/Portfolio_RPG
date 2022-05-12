using UnityEngine;
using UnityEngine.Events;
using Cinemachine;
using SK.Behavior;

namespace SK.FSM
{
    public class PlayerStateManager : MonoBehaviour
    {
        [SerializeField] private bool isDebugMode;

        public Data.PlayerData playerData;
        public Data.PlayerItemData playerItemData;

        [Header("References")]
        public CameraManager cameraManager;
        public CinemachineImpulseSource impulseSource;
        public Animator anim;
        public CharacterController characterController;
        public CapsuleCollider thisCollider;
        public EquipmentHolderManager equipmentHolder;
        public AnimatorHook animHook;
        public Combat combat;
        public Health health;

        [Header("States")] 
        public bool useRootMotion;
        [SerializeField]
        internal bool isDead, isGrounded, isRunning, isJumping, isDamaged, isDodge, isSlipping, isTargeting;

        [Header("Targeting")]
        [SerializeField] internal Transform cameraTarget;
        [SerializeField] private float targetSearchRange = 20;
        [SerializeField] private LayerMask targetLayer;
        [SerializeField] private Collider[] _targetColliders;
        internal Transform targetEnemy;

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
        internal InputActions playerInputs;
        internal MoveCharacter moveCharacter;
        internal MonitorAnimationBool monitorInteracting;

        // Unity Action
        internal UnityAction<float, float> OnKnockBack;

        internal Transform mTransform;

        private PlayerInputAction _playerInputAction;
        private Collider[] _groundCheckCols = new Collider[3];

        internal bool canComboAttack, isInteracting;
        internal float delta, fixedDelta;
        private float _comboTimer, _scrollY;
        private int _environmentLayer;

        #region Unity Events
        private void Awake()
        {
            mTransform = this.transform;
            GameManager.Instance.Player = this;

            // References 초기화
            if (!anim) anim = GetComponent<Animator>();
            if (!equipmentHolder) equipmentHolder = GetComponent<EquipmentHolderManager>();
            if (!animHook) animHook = GetComponentInChildren<AnimatorHook>();
            if (!impulseSource) impulseSource = GetComponent<CinemachineImpulseSource>();
            if (!combat) combat = GetComponent<Combat>();
            if (!health) health = GetComponent<Health>();

            // 인풋 시스템 초기화
            _playerInputAction = GameManager.Instance.InputManager.playerInputAction;

            // 상태 머신과 상태 액션 초기화
            stateMachine = new PlayerStateMachine(this);
            playerInputs = new InputActions(this, _playerInputAction);
            moveCharacter = new MoveCharacter(this);
            monitorInteracting = new MonitorAnimationBool(this, Strings.animPara_isInteracting);

            // Heath 컴포넌트 초기화
            health.Init(playerData.Level, playerData.Str, playerData.Dex, playerData.Int);

            // 카메라 세팅 초기화
            if (!cameraManager) cameraManager = Camera.main.GetComponent<CameraManager>();
            cameraManager.normalCamera.m_Lens.FieldOfView = PlayerPrefs.GetFloat("ZoomAmount", 0);
            cameraManager.Init(cameraTarget);
            _playerInputAction.GamePlay.CameraZoom.performed += x => _scrollY = x.ReadValue<float>() * cameraManager.cameraZoomSpeed;

            // 애니메이션 초기화
            anim.applyRootMotion = false;
            if (animHook) animHook.Init(this);

            // 변수 초기화
            _targetColliders = new Collider[5];
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

            // 카메라 줌 업데이트
            if (!isTargeting)
                cameraManager.ZoomUpdate(_scrollY);

            // 콤보 어택 타이머
            if (canComboAttack)
            {
                if (_comboTimer > 0)
                    _comboTimer -= fixedDelta;
                else
                    canComboAttack = false;
            }

            // 타겟팅 포인트 회전 값 초기화
            if (isTargeting && cameraTarget.localRotation != Quaternion.identity)
                cameraTarget.localRotation = Quaternion.Slerp(cameraTarget.localRotation, 
                                                              Quaternion.identity,
                                                              fixedDelta * rotationSpeed);
        }

        private void Update()
        {
            if (isDead) return;

            delta = Time.deltaTime;

            // Interacting 상태 Check
            isInteracting = anim.GetBool(Strings.animPara_isInteracting);

            // 피격 상태 업데이트
            if (isDamaged && !isInteracting)
                isDamaged = false;

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
                float minDegree = 360;
                int selectedIndex = 0;

                // 카메라 각도 비교 후 가장 작은 각의 target을 return
                for (int i = 0; i < _targetColliders.Length; i++)
                {
                    if (_targetColliders[i] != null)
                    {
                        var dir = (_targetColliders[i].transform.position - mTransform.position).normalized;
                        var degree = Vector3.Angle(cameraManager.mainCameraTr.forward, dir);

                        if (minDegree > degree)
                        {
                            minDegree = degree;
                            selectedIndex = i;
                        }
                    }
                    else
                        break;
                }
                return _targetColliders[selectedIndex].transform;
            }
            
            return null;
        }
        #endregion

        #region State
        internal void EnableRootMotion()
        {
            useRootMotion = true;
        }

        internal void DisableRootMotion()
        {
            useRootMotion = false;
        }
        public void AbleCombo() // Combo 가능 Animation Event
        {
            canComboAttack = true;
            _comboTimer = combat.canComboDuration;
        }
        #endregion

        #region Event Function
        private void OnDamageEvent()
        {
            float impulse;

            anim.SetBool(Strings.animPara_isInteracting, true);

            // 약 공격 피해
            if (!health.IsStrongAttack)
            {
                // 방패로 막아냄
                if (anim.GetBool(Strings.AnimPara_isShielding))
                {
                    // 넉백 효과
                    OnKnockBack?.Invoke(0.15f, 0.1f);

                    // 카메라 Impulse
                    impulseSource.GenerateImpulse(5);
                    return;
                }

                anim.SetTrigger(Strings.AnimPara_Damaged);
                impulse = 10f;
            }
            // 강 공격 피해
            else
            {
                // 넉백 효과
                OnKnockBack?.Invoke(0.3f, 0.1f);

                anim.SetTrigger(Strings.AnimPara_StrongDamaged);
                anim.CrossFade(Strings.AnimName_Shield_Hit, 0);
                impulse = 20f;
            }

            // 카메라 Impulse
            impulseSource.GenerateImpulse(impulse);
            isDamaged = true;

            // Debug 시 데미지 받지 않음
            if (!isDebugMode)
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

        // 공격 시점에 공격력을 계산하여 Combat 컴포넌트에 전달
        private void CalculateDamage()
            => combat.CalculateDamage(playerData.Level, playerData.Str, playerData.CriticalChance, playerData.CriticalMultiplier);
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