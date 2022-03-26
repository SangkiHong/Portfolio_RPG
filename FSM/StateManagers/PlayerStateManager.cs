using System.Collections.Generic;
using UnityEngine;

namespace SK.FSM
{
    public class PlayerStateManager : CharacterStateManager
    {
        public static PlayerStateManager Instance;
        
        public Data.PlayerData playerData;
        
        [Header("Inputs")] 
        public float moveAmount;
        public float cameraZoomSpeed = 0.02f;
        
        [Header("Movement States")]
        public LayerMask groundLayerMask;
        public float frontRayOffset = 1;
        public float frontRayOffsetHeight = 1;
        public float movementsSpeed = 5;
        public float runSpeed = 10;
        public float adaptSpeed = 8;
        public float rotationSpeed = 6;
        public float gravity = 5;
        public float jumpForce = 12;
        public float jumpIntervalDelay = 2;
        public float slopeLimitAngle = 60;

        private PlayerInputAction _playerInputAction;

        private const string LocomotionId = "locomotion";
        internal const string AttackStateId = "attackState";
        internal const string RollingStateId = "rollingState";
        [SerializeField]
        internal bool isJumping, isRunning, isSlipping;

        private Collider[] _groundCheckCols = new Collider[3];
        private float _comboTimer, _scrollY;
        private int _defaultLayer;
        
        public override void Init()
        {            
            base.Init();
            Instance = this;
            
            // 일반 상태 생성
            State locomotion = new State(
                new List<StateAction>() //Fixed Update
                { new MovePlayerCharacter(this) }, 
                new List<StateAction>() //Update
                { new InputManager(this, _playerInputAction), }, 
                new List<StateAction>() //Late Update
                { });

            locomotion.onEnter += DisableRootMotion;
            locomotion.onEnter += EnableDamage;

            // 공격 상태 생성
            State attackState = new State(
                new List<StateAction>() //Fixed Update
                { }, 
                new List<StateAction>() //Update
                { new MonitorInteractingAnimation(this, "isInteracting", LocomotionId), }, 
                new List<StateAction>() //Late Update
                { });

            attackState.onEnter += EnableRootMotion;

            // 구르기 상태 생성
            State rollingState = new State(
                new List<StateAction>() //Fixed Update
                { }, 
                new List<StateAction>() //Update
                { new MonitorInteractingAnimation(this, "isInteracting", LocomotionId), }, 
                new List<StateAction>() //Late Update
                { });

            rollingState.onEnter += EnableRootMotion;
            rollingState.onEnter += DisableDamage;

            // State 등록
            RegisterState(LocomotionId, locomotion);
            RegisterState(AttackStateId, attackState);
            RegisterState(RollingStateId, rollingState);
            
            // 기본 State 지정
            ChangeState(LocomotionId); 
        }

        #region Unity Update
        private void OnEnable()
        {
            // Event 등록
            health.onDamaged += OnDamageEvent;
            health.onDead += OnDeadEvent;
            if (combat) combat.onAttack += CalculateDamage;

            _playerInputAction = new PlayerInputAction();
            _playerInputAction.Enable();
            _playerInputAction.GamePlay.CameraZoom.performed += x => _scrollY = x.ReadValue<float>() * cameraZoomSpeed * -1;
            
            // 저장된 줌 값 가져오기
            normalCamera.m_Lens.FieldOfView = PlayerPrefs.GetFloat("ZoomAmount", 0);

            // Init Variable
            _defaultLayer = gameObject.layer;
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
            PlayerPrefs.SetFloat("ZoomAmount", normalCamera.m_Lens.FieldOfView);
        }

        private void FixedUpdate()
        {
            if (isDead) return;

            fixedDelta = Time.fixedDeltaTime;
            FixedTick();

            // Ground Check
            isGrounded = IsCheckGrounded();

            // Camera Zoom Control
            if (!lockOn)
            {
                if (_scrollY < 0 && normalCamera.m_Lens.FieldOfView <= 15)
                {
                    normalCamera.m_Lens.FieldOfView = 15;
                }
                else if (_scrollY > 0 && normalCamera.m_Lens.FieldOfView >= 60)
                {
                    normalCamera.m_Lens.FieldOfView = 60;
                }
                else
                {
                    normalCamera.m_Lens.FieldOfView += _scrollY;
                }
            }
            
            // Attack Combo Timer
            if (combat.canComboAttack)
            {
                if (_comboTimer > 0) 
                    _comboTimer -= fixedDelta;
                else
                    combat.canComboAttack = false;
            }
        }
        
        private void Update()
        {
            if (isDead) return;

            fixedDelta = Time.deltaTime;
            Tick();
        }

        private void LateUpdate()
        {
            if (isDead) return;

            LateTick();
        }
        #endregion

        #region State Events
        private void EnableRootMotion()
        {
            //thisRigidbody.isKinematic = false;
            useRootMotion = true;
        }
        private void DisableRootMotion()
        {
            //thisRigidbody.isKinematic = true; //deprecated::Don't use Rigidbody
            useRootMotion = false;
        }
        private void EnableDamage() => health.canDamage = true;
        private void DisableDamage() => health.canDamage = false;
        #endregion
        
        #region Event Func
        public void AbleCombo() // Combo 가능 Animation Event
        {
            combat.canComboAttack = true;
            _comboTimer = combat.canComboDuration;
        }

        private void OnDamageEvent()
        {
            impulseSource.GenerateImpulse(10f);
            anim.SetTrigger(Strings.AnimPara_Damaged);
            anim.SetBool(Strings.animPara_isInteracting, true);
            health.Damaged();
        }

        private void OnDeadEvent()
        {
            isDead = true;
            gameObject.layer = 0;
            health.enabled = false;
            //thisRigidbody.isKinematic = true; //deprecated::Don't use Rigidbody
            thisCollider.isTrigger = true;
            anim.Rebind();
            anim.SetTrigger(Strings.AnimPara_Dead);
        }

        private void CalculateDamage()
        {
            // Calculate Damage
            var level = playerData.Level;
            int weaponPower = Random.Range(combat.currentUseWeapon.attackMinPower, combat.currentUseWeapon.attackMaxPower + 1);
            var damage = (level * 0.5f) + (playerData.Str * 0.5f) + (weaponPower * 0.5f) + (level + 9);

            // Critical Chance
            if (Random.value < playerData.CriticalChance)
            {
                damage *= playerData.CriticalMultiplier;
                combat.isCriticalHit = true;
            }

            combat.calculatedDamage = (int)damage;
        }
        #endregion

        #region Ground Check
        private bool IsCheckGrounded()
        {
            var maxDistance = 0.15f;
            var position = mTransform.position;

            // 1차 Ray로 체크
            var ray = new Ray(position + Vector3.up * 0.1f, Vector3.down * maxDistance);
            Debug.DrawRay(position + Vector3.up * 0.1f, Vector3.down * maxDistance, Color.magenta);
            if (Physics.Raycast(ray, maxDistance, groundLayerMask))
            {
                isGrounded = true;
                return true;
            }

            // 2차 OverlapSphere로 체크
            var size = thisCollider.bounds.size.x;
            if (0 < Physics.OverlapSphereNonAlloc(position + Vector3.up * size, size, _groundCheckCols, groundLayerMask))
            { 
                isGrounded = true;
                return true;
            }

            Debug.Log("Not on the Ground");
            // Jump or Falling State
            //thisRigidbody.isKinematic = false; //deprecated::Don't use Rigidbody
            //thisRigidbody.drag = 0; //deprecated::Don't use Rigidbody
            //thisRigidbody.velocity += -Vector3.down * gravity; //deprecated::Don't use Rigidbody

            return isGrounded = false;
        }

        void OnDrawGizmosSelected()
        {
            if (!isGrounded)
            {
                var size = thisCollider.bounds.size.x;
                Gizmos.DrawSphere(mTransform.position + Vector3.up * size, size);
            }
        }        
        #endregion
    }
}