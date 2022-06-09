using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Cinemachine;
using SK.Behavior;

namespace SK.FSM
{
    public class Player : Unit
    {
        [SerializeField] private bool isDebugMode;

        public Data.PlayerData playerData;
        public Data.PlayerItemData playerItemData;

        // Unity Action
        internal UnityAction<Transform, float, float> OnKnockBackState;

        [Header("Component")]
        internal CameraManager cameraManager;
        public Transform renderCameraTransform;
        public CharacterController characterController;
        public CapsuleCollider thisCollider;

        [Header("Reference")]
        public CinemachineImpulseSource impulseSource;
        public EquipmentHolderManager equipmentHolder;
        public AnimatorHook animHook;
        internal Targeting targeting;

        [Header("States")]
        // 특정 행동(대화, 공격, 점프 등등) 상태 여부
        internal bool isInteracting; 
        // 각종 플레이어 상태 여부
        [SerializeField] internal bool useRootMotion, isDead, isGrounded, 
            isRunning, isJumping, isDamaged, isDodge, isSlipping, onCombatMode;

        [Header("Movement States")]
        public LayerMask groundLayerMask;
        [SerializeField] private float groundDistance;
        public float frontRayOffset = 1;
        public float frontRayOffsetHeight = 1;
        public float movementsSpeed = 5;
        public float runSpeed = 10;
        public float slowDownBackward = 0.7f;
        public float slowDownTargeting = 0.8f;
        public float rotationSpeed = 6;
        [Space]
        public float groundedGravity = -0.05f;
        public float gravity = -9.8f;
        public float gravityAcclation = 1;
        public float slopeLimitAngle = 60;
        [Space]
        public float jumpHeight = 12;
        public float jumpTime = 10;
        public float jumpDelay = 2;

        [Header("Targeting")]
        [SerializeField] internal Transform cameraTarget;
        [SerializeField] private float targetSearchRange = 20;
        [SerializeField] private LayerMask targetLayerMask;

        [Header("Dodge")]
        [SerializeField] internal float dodgeSpeed = 1;
        [SerializeField] internal AnimationCurve animationCurve_Forward;
        [SerializeField] internal AnimationCurve animationCurve_back;

        // Controller Values
        [SerializeField] internal float horizontal;
        [SerializeField] internal float vertical;
        [SerializeField] internal float moveAmount;

        // States
        internal PlayerStateMachine stateMachine; // 상태 머신

        // States Actions
        internal InputActions inputActions; // 플레이어 인풋을 담당하는 클래스
        internal CharacterMovement moveCharacter; // 플레이어 움직임을 담당하는 클래스
        internal MonitorAnimationBool monitorInteracting; // 인터렉팅 상태를 모니터하는 클래스

        // OverlapSphereNonAlloc함수를 통해 콜라이더를 담을 버퍼
        private Collider[] _groundCheckCols;

        private float _scrollY;

        #region Unity Events
        public override void Awake()
        {
            base.Awake();

            StartCoroutine(PlayerInit());
        }

        IEnumerator PlayerInit()
        {
            var ws = new WaitForEndOfFrame();

            // 게임매니저에 접근 가능한 상태까지 대기
            while (GameManager.Instance == null)
                yield return ws;
            Debug.Log("게임매니저에 플레이어 할당");
            GameManager.Instance.Player = this;

            // References 초기화
            if (!equipmentHolder) equipmentHolder = GetComponent<EquipmentHolderManager>();
            if (!impulseSource) impulseSource = GetComponent<CinemachineImpulseSource>();
            if (!animHook) animHook = GetComponentInChildren<AnimatorHook>();

            // 유닛 정보 초기화
            combat.SetUnitInfo(this, playerData);

            // Heath 컴포넌트 초기화
            health.Init(playerData.Level, playerData.Str, playerData.Dex, playerData.Int);

            // 카메라 세팅 초기화
            if (!cameraManager) cameraManager = Camera.main.GetComponent<CameraManager>();
            cameraManager.normalCamera.m_Lens.FieldOfView = PlayerPrefs.GetFloat("ZoomAmount", 0);
            GameManager.Instance.InputManager.playerInput
                .actions["CameraZoom"].performed += x => _scrollY = x.ReadValue<float>() * cameraManager.cameraZoomSpeed;

            // 상태 머신과 상태 액션 초기화
            stateMachine = new PlayerStateMachine(this);
            inputActions = new InputActions(this, GameManager.Instance.InputManager.playerInput);
            moveCharacter = new CharacterMovement(this);
            monitorInteracting = new MonitorAnimationBool(this, Strings.animPara_isInteracting);

            // 타겟팅 기능 클래스 초기 생성
            targeting = new Targeting(transform, cameraManager, cameraTarget, targetSearchRange, targetLayerMask);

            // 애니메이션 초기화
            anim.applyRootMotion = false;
            if (animHook) animHook.Init(this);

            // 변수 초기화
            _groundCheckCols = new Collider[5];

            Debug.Log("플레이어 초기화 완료");
        }

        public override void FixedTick()
        {
            if (isDead) return;

            fixedDeltaTime = Time.fixedDeltaTime;

            if (stateMachine.isAssigned())
                stateMachine.CurrentState.FixedTick();

            // Ground Check
            isGrounded = IsCheckGrounded();

            // 카메라 줌 업데이트
            if (!targeting.isTargeting)
                cameraManager.ZoomUpdate(_scrollY);

            // 타겟팅 포인트 회전 값 초기화
            targeting.ResetTargetingPoint(fixedDeltaTime, rotationSpeed);
        }

        public override void Tick()
        {
            if (isDead) return;
            base.Tick();

            deltaTime = Time.deltaTime;

            // Interacting 상태 Check
            isInteracting = anim.GetBool(Strings.animPara_isInteracting);

            // 피격 상태 업데이트
            if (isDamaged && !isInteracting)
                isDamaged = false;

            if (stateMachine.isAssigned())
                stateMachine.CurrentState.Tick();
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
        public void ShieldCheck()
        {
            if (anim.GetBool(Strings.AnimPara_isShielding))
            {
                anim.SetLayerWeight(2, 1);
            }
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

        public void AbleCombo() => combat.canComboAttack = true;
        #endregion

        #region Event Function
        public override void OnDamage(Unit attacker, uint damage, bool isStrong)
        {
            float impulse;

            anim.SetBool(Strings.animPara_isInteracting, true);

            // 약 공격 피해
            if (!isStrong)
            {
                // 방패로 막아냄
                if (anim.GetBool(Strings.AnimPara_isShielding))
                {
                    // 넉백 효과
                    OnKnockBackState?.Invoke(attacker.transform, 0.15f, 0.1f);

                    // 방패 피격 애니메이션
                    anim.CrossFade(Strings.AnimName_Shield_Hit, 0);

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
                OnKnockBackState?.Invoke(attacker.transform, 0.3f, 0.1f);

                anim.SetTrigger(Strings.AnimPara_StrongDamaged);
                anim.CrossFade(Strings.AnimName_Shield_Hit, 0);
                impulse = 20f;
            }

            // 카메라 Impulse
            impulseSource.GenerateImpulse(impulse);

            isDamaged = true;

            // Debug 시 데미지 받지 않음
            if (!isDebugMode)
                health.OnDamage(damage, combat.isCriticalHit);
        }

        public override void OnDead()
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
        #endregion

        #region Collision Check
        private bool IsCheckGrounded()
        {
            var position = mTransform.position;

            // 1차 Character Controller로 체크
            if (characterController.isGrounded) return true;

            // 2차 OverlapSphere로 체크
            var size = thisCollider.bounds.size.x * 0.5f;
            if (0 < Physics.OverlapSphereNonAlloc(position + Vector3.up * size * 0.5f, size, _groundCheckCols, groundLayerMask))
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