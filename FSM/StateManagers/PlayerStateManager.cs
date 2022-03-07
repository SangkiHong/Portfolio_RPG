using System.Collections.Generic;
using Sanki;
using SK.FSM;
using UnityEngine;

namespace SK
{
    public class PlayerStateManager : CharacterStateManager
    {
        public static PlayerStateManager Instance;
        
        [Header("Inputs")] 
        public float mouseX;
        public float mouseY;
        public float moveAmount;
        public float cameraZoomSpeed = 0.02f;

        [Header("References")] 
        public Cinemachine.CinemachineFreeLook normalCamera;
        public Cinemachine.CinemachineFreeLook lockOnCamera;
        
        [Header("Movement States")]
        public float frontRayOffset = 1;
        public float movementsSpeed = 10;
        public float runSpeed = 15;
        public float adaptSpeed = 10;
        public float rotationSpeed = 6;
        public float jumpForce = 10;
        public float jumpIntervalDelay = 3;

        [Header("Attack")]
        public bool canComboAttack;
        public float canComboDuration = 1.5f;
        public ComboAttack[] currentCombo;

        private PlayerInputAction _playerInputAction;
        
        internal LayerMask ignoreForGroundCheck;

        internal const string LocomotionId = "locomotion";
        internal const string AttackStateId = "attackState";
        internal const string RollingStateId = "rollingState";

        [HideInInspector]
        public bool isChangingWeight;
        
        internal bool isJump, isRun;
        
        private int _targetLayer;
        private float _targetWeight, _comboTimer, _scrollY;
        
        public override void Init()
        {
            base.Init();
            Instance = this;
            
            State locomotion = new State(
                new List<StateAction>() //Fixed Update
                {
                    new MovePlayerCharacter(this)
                }, new List<StateAction>() //Update
                {
                    new InputManager(this, _playerInputAction),
                }, new List<StateAction>() //Late UPdate
                {
                });

            locomotion.onEnter = DisableRootMotion;
            
            State attackState = new State(
                new List<StateAction>() //Fixed Update
                {
                }, new List<StateAction>() //Update
                {
                    new MonitorInteractingAnimation(this, "isInteracting", LocomotionId),
                }, new List<StateAction>() //Late UPdate
                {
                });
            
            State rollingState = new State(
                new List<StateAction>() //Fixed Update
                {
                }, new List<StateAction>() //Update
                {
                    new MonitorInteractingAnimation(this, "isInteracting", LocomotionId),
                }, new List<StateAction>() //Late Update
                {
                });

            attackState.onEnter = EnableRootMotion;
            rollingState.onEnter = EnableRootMotion;
            
            RegisterState(LocomotionId, locomotion);
            RegisterState(AttackStateId, attackState);
            RegisterState(RollingStateId, rollingState);
            
            ChangeState(LocomotionId);

            ignoreForGroundCheck = ~(1 << 9 | 1 << 10);
            
            equipmentHolderManager.Init();
            equipmentHolderManager.LoadEquipmentOnHook(primaryEquipment, true);
            equipmentHolderManager.LoadEquipmentOnHook(secondaryEquipment, false);
            if (primaryEquipment.GetType() == typeof(Weapon))
                AssignCurrentWeapon(primaryEquipment);
            if (secondaryEquipment.GetType() == typeof(Weapon))
                AssignCurrentWeapon(secondaryEquipment);
        }

        #region Unity Update
        private void OnEnable()
        {
            _playerInputAction = new PlayerInputAction();
            equipmentHolderManager = GetComponent<EquipmentHolderManager>();
            _playerInputAction.Enable();
            _playerInputAction.GamePlay.CameraZoom.performed += x => _scrollY = x.ReadValue<float>() * cameraZoomSpeed * -1;
            
            normalCamera.m_Lens.FieldOfView = PlayerPrefs.GetFloat("ZoomAmount", 0);
        }

        private void OnDisable()
        {
            _playerInputAction.Disable();
        }

        private void OnApplicationQuit()
        {
            PlayerPrefs.SetFloat("ZoomAmount", normalCamera.m_Lens.FieldOfView);
        }

        private void FixedUpdate()
        {
            delta = Time.fixedDeltaTime;
            base.FixedTick();

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
            if (canComboAttack)
            {
                if (_comboTimer > 0) _comboTimer -= delta;
                else
                {
                    canComboAttack = false;
                }
            }

            // Change Animator Layer Weight
            if (isChangingWeight)
            {
                float currentWeight = anim.GetLayerWeight(_targetLayer);

                if (_targetWeight > 0.5f)
                {
                    if (currentWeight < 0.99f) currentWeight += delta * 3f;
                    else
                    {
                        currentWeight = 1;
                        isChangingWeight = false;
                    }
                }
                else
                {
                    if (currentWeight > 0.01f) currentWeight -= delta * 3f;
                    else
                    {
                        currentWeight = 0;
                        isChangingWeight = false;
                    }
                }
                anim.SetLayerWeight(_targetLayer, currentWeight);
            }
        }
        
        private void Update()
        {
            delta = Time.deltaTime;
            base.Tick();
        }

        private void LateUpdate()
        {
            base.LateTick();
        }
        #endregion

        #region Lock on
        public override void OnAssignLookOverride(Transform lockTarget)
        {
            base.OnAssignLookOverride(lockTarget);
            normalCamera.gameObject.SetActive(false);
            lockOnCamera.gameObject.SetActive(true);
            lockOnCamera.m_LookAt = lockTarget;
        }

        public override void OnClearLookOverride()
        {
            base.OnClearLookOverride();
            normalCamera.gameObject.SetActive(true);
            lockOnCamera.gameObject.SetActive(false);
        }
        #endregion
        
        #region State Events
        private void DisableRootMotion() => useRootMotion = false;
        private void EnableRootMotion() => useRootMotion = true;
        #endregion
        
        #region Animation Event
        public void AbleCombo()
        {
            canComboAttack = true;
            _comboTimer = canComboDuration;
        }

        public void ChangeLayerWeight(int targetLayerIndex, float targetWeight)
        {
            _targetLayer = targetLayerIndex;
            _targetWeight = targetWeight;
            isChangingWeight = true;
        }
        
        // Land Sound
        public void Land(){}
        #endregion
    }
}