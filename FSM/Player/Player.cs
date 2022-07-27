using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using Cinemachine;
using SK.Behavior;

namespace SK.FSM
{
    #region Required Component
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(CinemachineImpulseSource))]
    [RequireComponent(typeof(EquipmentHolderManager))]
    [RequireComponent(typeof(AnimatorHook))]
    [RequireComponent(typeof(State.Mana))]
    #endregion
    public class Player : Unit
    {
        #region Variables
        [SerializeField] private bool isDebugMode;

        private Data.PlayerData playerData;

        // Unity Action
        internal UnityAction<Transform, float, float> OnKnockBackState;

        [Header("Component")]
        public CapsuleCollider thisCollider;
        public CharacterController characterController;
        internal CameraManager cameraManager;
        public Transform renderCameraTransform;
        public CinemachineImpulseSource impulseSource;
        public EquipmentHolderManager equipmentHolder;
        public AnimatorHook animHook;
        public Animation.IKControl ikControl;
        public State.Mana mana;
        public State.Stamina stamina;
        internal Targeting targeting;

        [Header("States")]
        [SerializeField] internal uint useSp_Run = 2; // 초당 소모량
        [SerializeField] internal uint useSp_Jump = 5;
        [SerializeField] internal uint useSp_Attack = 1;
        [SerializeField] internal uint useSp_Dodge = 3;
        [SerializeField] internal uint useSp_Shield = 5;
        // 각종 플레이어 상태 여부
        internal bool useRootMotion, isGrounded, 
                      isRunning, isJumping, 
                      isDamaged, isDodge, isSlipping, 
                      onCombatMode;

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

        [Header("Damage Effect")]
        [SerializeField] private float shieldDamageImpulseIntensity = 5;
        [SerializeField] private float damageImpulseIntensity = 10;
        [SerializeField] private float strongDamageImpulseIntensity = 20;

        // Controller Values
        internal float horizontal;
        internal float vertical;
        internal float moveAmount;

        // States
        internal PlayerStateMachine stateMachine; // 상태 머신

        // States Actions
        internal InputActions inputActions; // 플레이어 인풋을 담당하는 클래스
        internal CharacterMovement moveCharacter; // 플레이어 움직임을 담당하는 클래스
        internal MonitorAnimationBool monitorInteracting; // 인터렉팅 상태를 모니터하는 클래스

        // OverlapSphereNonAlloc함수를 통해 콜라이더를 담을 버퍼
        private Collider[] _groundCheckCols;

        internal bool isOnRushAttack;
        private float _rushAttackInterval, _rushAttackElapsed;
        private AnimationEvent _rushAttackAnimEvent;
        private Unit _attacker; // 최근 플레이어를 공격한 유닛 정보

        private bool _isPlayFootStepSound;
        private float _footStepSoundInterval = 0.15f;
        private float _footStepElapsed;
        #endregion

        #region Unity Events
        public override void Awake()
        {
            base.Awake();

            StartCoroutine(PlayerInit());
        }

        IEnumerator PlayerInit()
        {
            var ws = new WaitForEndOfFrame();

            // References 초기화
            if (!equipmentHolder) equipmentHolder = GetComponent<EquipmentHolderManager>();
            if (!impulseSource) impulseSource = GetComponent<CinemachineImpulseSource>();
            if (!animHook) animHook = GetComponentInChildren<AnimatorHook>();
            if (!cameraManager) cameraManager = Camera.main.GetComponent<CameraManager>();
            if (!mana) mana = GetComponent<State.Mana>();
            if (!stamina) stamina = GetComponent<State.Stamina>();

            // 데이터 파일 할당
            playerData = Data.DataManager.Instance.PlayerData;

            // 체력 초기화
            health.Initialize(playerData, mTransform, true);

            // 마력 초기화
            mana.Initialize(playerData, true);

            // 스테미나 초기화
            stamina.Initialize(playerData);

            // 전투 컴포넌트 초기화
            combat.Initialize(this, playerData);

            // 게임매니저에 접근 가능한 상태까지 대기
            while (GameManager.Instance == null)
                yield return ws;
            Debug.Log("게임매니저에 플레이어 할당");
            GameManager.Instance.Player = this;

            // 상태 머신과 상태 액션 초기화
            if (stateMachine == null) stateMachine = new PlayerStateMachine(this);
            if (inputActions == null) inputActions = new InputActions(this, GameManager.Instance.InputManager.playerInput);
            if (moveCharacter == null) moveCharacter = new CharacterMovement(this, transform);
            if (monitorInteracting == null) monitorInteracting = new MonitorAnimationBool(this, Strings.animPara_isInteracting);

            // 타겟팅 기능 클래스 초기 생성
            if (targeting == null) targeting = new Targeting(transform, cameraManager, cameraTarget, targetSearchRange, targetLayerMask);
            if (cameraManager)
            {
                // 카메라 세팅 초기화
                cameraManager.Init(cameraTarget);
                cameraManager.normalCamera.m_Lens.FieldOfView = PlayerPrefs.GetFloat("ZoomAmount", 0);
                GameManager.Instance.InputManager.playerInput.actions["CameraZoom"].performed += Zooming;
            }

            // 애니메이션 초기화
            anim.applyRootMotion = false;
            if (animHook) animHook.Init(this);

            // 플레이어를 최근 접속 위치로 이동
            characterController.enabled = false;
            transform.position = playerData.RecentPosition;
            characterController.enabled = true;

            // 변수 초기화
            _groundCheckCols = new Collider[5];

            // 플레이어 현재 지역 위치 초기화
            SceneManager.Instance.locationManager.SetPlayerLocation(playerData.RecentLocation);

            // 공격 성공 시 호출될 이벤트 함수 할당
            combat.OnAttackSuccess += OnAttackSuccess;
            combat.OnRushAttack += OnRushAttack;

            Debug.Log("플레이어 초기화 완료");
        }

        public override void OnEnable()
        {
            // 씬 매니저의 유닛 관리 대상에 추가
            SceneManager.Instance.AddUnit(this);

            // 리스폰 상태인 경우
            if (isDead)
            {
                useRootMotion = false;
                isGrounded = true;
                isRunning = false;
                isJumping = false;
                isDamaged = false;
                isDodge = false;
                isSlipping = false;
                onCombatMode = false;
                isOnRushAttack = false;
                gameObject.layer = 9;
                health.enabled = true;
                thisCollider.isTrigger = false;
                stateMachine.ChangeState(stateMachine.locomotionState);

                // 체력 초기화
                health.Initialize(playerData, mTransform, true);

                // 마력 초기화
                mana.Initialize(playerData, true);

                // 스테미나 초기화
                stamina.Initialize(playerData);

                // 전투 컴포넌트 초기화
                combat.Initialize(this, playerData);

                // 인풋 이벤트 할당
                inputActions.AssignInputEvent();

                // 카메로 회전 고정 해제
                cameraManager.CameraRotatingHold(false);

                // 애니메이션 초기화
                anim.applyRootMotion = false;
                anim.Rebind();

                // 플레이어를 최근 접속 위치로 이동
                characterController.enabled = true;

                // 화면이 밝아지게 함
                UI.UIManager.Instance.BlackScreenControl(false);
            }

            // 기본 초기화
            ikControl.ActiveIK = true;

            base.OnEnable();
        }

        public override void FixedTick()
        {
            if (isDead) return;

            fixedDeltaTime = Time.fixedDeltaTime;

            if (stateMachine.isAssigned())
                stateMachine.CurrentState.FixedTick();

            // Ground Check
            isGrounded = IsCheckGrounded();

            // 가만히 서 있는 경우 IK 활성화
            if (moveAmount == 0) ikControl.ActiveIK = true;
            // 움직이는 경우 IK 비활성화
            else ikControl.ActiveIK = false;

            // 타겟팅 포인트 회전 값 초기화
            targeting.ResetTargetingPoint(fixedDeltaTime, rotationSpeed);

            // 발걸음 효과음 간격 체크
            if (_isPlayFootStepSound)
            {
                _footStepElapsed += fixedDeltaTime;

                if (_footStepSoundInterval <= _footStepElapsed)
                    _isPlayFootStepSound = false;
            }

            // 돌진 공격 상태인 경우
            if (isOnRushAttack)
            {
                // 공격 중단된 경우
                if (!anim.GetBool(Strings.AnimPara_onRushAttack))
                    OnRushAttack(null);

                _rushAttackElapsed += fixedDeltaTime;

                if (_rushAttackElapsed >= _rushAttackInterval)
                {
                    _rushAttackElapsed = 0;
                    combat.Attack(_rushAttackAnimEvent);
                }

                // 앞으로 돌진
                moveCharacter.Execute();
            }
        }

        public override void Tick()
        {
            if (isDead) return;

            deltaTime = Time.deltaTime;

            // Interacting 상태 Check
            isInteracting = anim.GetBool(Strings.animPara_isInteracting);

            // 피격 상태 업데이트
            if (isDamaged && !isInteracting)
                isDamaged = false;

            if (stateMachine.isAssigned())
                stateMachine.CurrentState.Tick();
        }

        public override void OnDisable()
        {
            base.OnDisable();

            // 씬 매니저의 유닛 관리 대상에서 해제
            if (SceneManager.Instance)
                SceneManager.Instance.RemoveUnit(gameObject.GetInstanceID());
        }

        private void OnApplicationQuit()
        {
            // 줌 값 저장하기
            PlayerPrefs.SetFloat("ZoomAmount", cameraManager.normalCamera.m_Lens.FieldOfView);
            GameManager.Instance.InputManager.playerInput.actions["CameraZoom"].performed -= Zooming;

            // 공격 성공 시 호출 이벤트 해제
            combat.OnAttackSuccess -= OnAttackSuccess;
            combat.OnRushAttack -= OnRushAttack;
        }
        #endregion

        #region Animation Event
        public void PlayerTargetAnimation(string targetAnim, bool isInteracting, int targetLayer = 0)
        {
            anim.SetBool(Strings.animPara_isInteracting, isInteracting);
            anim.CrossFade(targetAnim, 0.15f);
            if (targetLayer > 0) anim.SetLayerWeight(targetLayer, 1);
        }
        
        // 방패로 디펜스 중인 경우 애니메이터 레이어의 웨이트 변경
        public void ShieldCheck()
        {
            if (anim.GetBool(Strings.AnimPara_isShielding))
                anim.SetLayerWeight(2, 1);
        }

        // 발걸음 소리 재생
        public override void FootStepSound()
        {
            // 효과음 간격 시간 체크
            if (!_isPlayFootStepSound)
            {
                _isPlayFootStepSound = true;
                _footStepElapsed = 0;

                // 효과음 재생
                int randomindex = Random.Range(0, Strings.Audio_FX_Player_Footstep.Length);
                AudioManager.Instance.PlayAudio(Strings.Audio_FX_Player_Footstep[randomindex], mTransform);
            }
        }

        internal void EnableRootMotion()
        {
            useRootMotion = true;
        }

        internal void DisableRootMotion()
        {
            useRootMotion = false;
        }

        public void AbleCombo() => combat.SetComboState(true);
        #endregion

        #region Attack
        public void ImmediatelyEquipWeapon()
        {
            // 주무기 착용
            equipmentHolder.Equip();

            // 보조장비 착용
            if (equipmentHolder.secondaryEquipment)
            {
                equipmentHolder.Equip(1);

                // 애니메이션 파라미터 변경
                if (equipmentHolder.secondaryEquipment.isShield)
                    anim.SetBool(Strings.AnimPara_EquipShield, true);
            }

            // 애니메이션 전투 상태로 전환
            anim.SetBool(Strings.AnimPara_onCombat, true);
        }

        private void OnAttackSuccess(bool isStrongAttack, bool isCriticalAttack)
        {
            // 카메라 효과
            impulseSource.GenerateImpulse(damageImpulseIntensity);

            // SP 회복
            stamina.RecoverSp();
        }

        private void OnRushAttack(AnimationEvent animEvent)
        {
            // 돌진 공격 시작
            if (animEvent != null)
            {
                onUninterruptible = true;
                _rushAttackAnimEvent = animEvent;
                // 애니메이션 이벤트 매개변수로 전달된 값을 전달하여 돌진 공격 발생 간격을 변수에 저장
                _rushAttackInterval = _rushAttackAnimEvent.floatParameter;
                _rushAttackElapsed = 0;

                // 카메라 줌 효과
                cameraManager.ZoomEffect(true);
            }
            // 돌진 공격 마침
            else
            {
                onUninterruptible = false;
                _rushAttackAnimEvent = null;

                // 카메라 줌 효과 해제
                cameraManager.ZoomEffect(false);
            }

            // 돌진 공격 여부
            isOnRushAttack = onUninterruptible;
        }
        #endregion

        #region Player State
        public bool CanDodge()
            { return (isGrounded && onCombatMode && !isDodge && !isDamaged && !isOnRushAttack && !useRootMotion); }
        public bool CanAttack()
            { return (isGrounded && !isInteracting && !anim.GetBool(Strings.AnimPara_isChangingEquipState) &&
                stateMachine.CurrentState != stateMachine.attackState && combat.CanComboAttack && !isOnRushAttack);
        }
        public bool CanShield()
            { return (CanAttack() && onCombatMode); }
        #endregion

        #region Event Function
        public override void OnDamage(Unit attacker, uint damage, bool isStrong)
        {
            // 회피 중인 경우 리턴
            if (isDodge) return;

            isDamaged = true;

            // 플레이어를 공격한 유닛 정보를 변수에 저장
            _attacker = attacker;

            // 돌진 공격인 경우
            if (isOnRushAttack) OnRushAttack(null);

            // 크리티컬 여부
            bool isCritical = combat.IsCriticalHit;

            // 애니메이터 파라미터 상태 변경
            anim.SetBool(Strings.animPara_isInteracting, true);

            // 중단 불가 상태가 아닌 경우에만 데미지 효과 실행
            if (!onUninterruptible)
            {
                // 약 공격 피해
                if (!isStrong)
                {
                    // 방패로 막아냄
                    if (anim.GetBool(Strings.AnimPara_isShielding))
                    {
                        // 공격자와 플레이어의 각도
                        float angle = attacker ? Vector3.Angle(mTransform.forward, attacker.mTransform.position - mTransform.position) : 0;

                        // SP 소모 가능하며 각도가 45도 이하인 경우 방패 막기 성공
                        if (stamina.UseSp(useSp_Shield) && angle <= 45)
                        {
                            // 넉백 효과
                            OnKnockBackState?.Invoke(attacker.mTransform, 0.15f, 0.1f);

                            // 방패 피격 애니메이션 재생
                            anim.CrossFade(Strings.AnimName_Shield_Hit, 0);

                            // 카메라 Impulse
                            impulseSource.GenerateImpulse(shieldDamageImpulseIntensity);

                            // 쉴드 이펙트 효과
                                EffectManager.Instance.PlayEffect(4009, mCollider.bounds.center + (mTransform.forward * 0.3f), Quaternion.identity);

                            // 쉴드 사운드 효과
                            int randomindex = Random.Range(0, Strings.Audio_FX_Hit_ShieldImpact.Length);
                            AudioManager.Instance.PlayAudio(Strings.Audio_FX_Hit_ShieldImpact[randomindex], mTransform);
                            return;
                        }
                    }

                    anim.SetTrigger(Strings.AnimPara_Damaged);

                    // 카메라 Impulse
                    impulseSource.GenerateImpulse(damageImpulseIntensity);
                }
                // 강 공격 피해
                else
                {
                    // 애니메이터 파라미터 상태 변경
                    anim.SetBool(Strings.animPara_isInteracting, true);
                    // 강공격 피해 애니메이션 재생
                    anim.SetTrigger(Strings.AnimPara_StrongDamaged);

                    // 방패 막기 해제
                    inputActions.Unshielding();

                    // 넉백 효과
                    if (attacker) OnKnockBackState?.Invoke(attacker.mTransform, 0.3f, 0.1f);

                    // 카메라 Impulse
                    impulseSource.GenerateImpulse(strongDamageImpulseIntensity);
                }

                // 피격 음성 사운드 효과
                int index = Random.Range(0, Strings.Audio_FX_Voice_Player_Pain.Length);
                AudioManager.Instance.PlayAudio(Strings.Audio_FX_Voice_Player_Pain[index], mTransform);
            }
            // 중단 불가 상태인 경우 카메라 흔들림 효과만 발생
            else
                impulseSource.GenerateImpulse(shieldDamageImpulseIntensity);

            // 피해 이펙트 효과
            if (isCritical) // 크리티컬
                EffectManager.Instance.PlayEffect(4008, mCollider.bounds.center, Quaternion.identity);
            else if (isStrong) // 강 공격
                EffectManager.Instance.PlayEffect(4004, mCollider.bounds.center, Quaternion.identity);
            else // 일반 공격
                EffectManager.Instance.PlayEffect(4005, mCollider.bounds.center, Quaternion.identity);

            // 카메라 효과
            cameraManager.DamageEffect(true);

            // Debug 시 데미지 받지 않음
            if (!isDebugMode)
                health.OnDamage(damage, isCritical);
        }

        public override void OnDamage(Transform damagableObject, uint damage, bool isStrong)
        {
            isDamaged = true;

            // 애니메이터 파라미터 상태 변경
            anim.SetBool(Strings.animPara_isInteracting, true);

            // 약 공격 피해
            if (!isStrong)
            {
                // 방패로 막아냄
                if (anim.GetBool(Strings.AnimPara_isShielding))
                {
                    // 공격자와 플레이어의 각도
                    float angle = Vector3.Angle(mTransform.forward, damagableObject.position - mTransform.position);

                    // SP 소모 가능하며 각도가 45도 이하인 경우 방패 막기 성공
                    if (stamina.UseSp(useSp_Shield) && angle <= 45)
                    {
                        // 넉백 효과
                        OnKnockBackState?.Invoke(damagableObject, 0.15f, 0.1f);

                        // 방패 피격 애니메이션 재생
                        anim.CrossFade(Strings.AnimName_Shield_Hit, 0);

                        // 카메라 Impulse
                        impulseSource.GenerateImpulse(shieldDamageImpulseIntensity);

                        // 쉴드 이펙트 효과
                        EffectManager.Instance.PlayEffect(4009, mCollider.bounds.center + (mTransform.forward * 0.3f), Quaternion.identity);

                        // 쉴드 사운드 효과
                        int randomindex = Random.Range(0, Strings.Audio_FX_Hit_ShieldImpact.Length);
                        AudioManager.Instance.PlayAudio(Strings.Audio_FX_Hit_ShieldImpact[randomindex], mTransform);
                        return;
                    }
                }

                anim.SetTrigger(Strings.AnimPara_Damaged);

                // 카메라 Impulse
                impulseSource.GenerateImpulse(damageImpulseIntensity);
            }
            // 강 공격 피해
            else
            {
                // 애니메이터 파라미터 상태 변경
                anim.SetBool(Strings.animPara_isInteracting, true);
                // 강공격 피해 애니메이션 재생
                anim.SetTrigger(Strings.AnimPara_StrongDamaged);

                // 방패 막기 해제
                inputActions.Unshielding();

                // 넉백 효과
                OnKnockBackState?.Invoke(damagableObject, 0.3f, 0.1f);

                // 카메라 Impulse
                impulseSource.GenerateImpulse(strongDamageImpulseIntensity);
            }

            // 피격 음성 사운드 효과
            int index = Random.Range(0, Strings.Audio_FX_Voice_Player_Pain.Length);
            AudioManager.Instance.PlayAudio(Strings.Audio_FX_Voice_Player_Pain[index], mTransform);

            // Debug 시 데미지 받지 않음
            if (!isDebugMode)
                health.OnDamage(damage, false);
        }

        public override void OnDead()
        {
            // 플레이어 무능력 상태 초기화
            isDead = true;
            isOnRushAttack = false;
            gameObject.layer = 0;
            health.enabled = false;
            thisCollider.isTrigger = true;
            stateMachine.StopMachine();

            health.Recovering(false);
            mana.StopRecovering();
            stamina.StopRecovering();

            combat.SetTarget(null);
            cameraManager.OnClearLookOverride();

            anim.Rebind();
            anim.SetTrigger(Strings.AnimPara_Dead);

            // 사운드 효과
            int index = Random.Range(0, Strings.Audio_FX_Voice_Death.Length);
            AudioManager.Instance.PlayAudio(Strings.Audio_FX_Voice_Death[index], mTransform);

            // 메뉴 UI 표시
            UI.UIManager.Instance.respawnMenuHandler.Show(((Enemy)_attacker).enemyData.DisplayName);

            // 게임매니저에게 죽음 상태 전달
            GameManager.Instance.PlayerDead();

            // 카메로 회전 고정
            cameraManager.CameraRotatingHold(true);

            // 인풋 이벤트 할당 해제
            inputActions.UnassignInputEvent();

            // 장비 착용 해제
            equipmentHolder.Unequip(0);
            equipmentHolder.Unequip(1);
        }

        public void SavePlayState()
        {
            playerData.Hp = health.CurrentHp;
            playerData.Mp = mana.CurrentMp;
        }

        private void Zooming(InputAction.CallbackContext context)
        {
            // 카메라 줌 업데이트
            if (!targeting.isTargeting)
                cameraManager.ZoomUpdate(context.ReadValue<float>());
        }
        #endregion

        #region Collision Check
        private bool IsCheckGrounded()
        {
            var position = mTransform.position;

            // 1차 체크 Character Controller
            if (characterController.isGrounded) return true;

            // 2차 체크 Ray
            var ray = new Ray(position + Vector3.up * 0.1f, Vector3.down);
            Debug.DrawRay(position + Vector3.up * 0.1f, Vector3.down * groundDistance, Color.magenta);
            if (Physics.Raycast(ray, groundDistance, groundLayerMask))
            {
                isGrounded = true;
                return true;
            }

            // 3차 체크 OverlapSphere
            var size = thisCollider.bounds.size.x * 0.5f;
            if (0 < Physics.OverlapSphereNonAlloc(position + Vector3.up * size * 0.5f, size, _groundCheckCols, groundLayerMask))
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