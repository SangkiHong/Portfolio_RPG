using System;
using System.Collections.Generic;
using Sanki;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Sangki
{
    public class PlayerStateManager : HumanoidStateManager
    {
        [Header("Inputs")] 
        public bool debugLock;
        public bool isFixRotation;
        public float mouseX;
        public float mouseY;
        public float moveAmount;
        public float cameraZoomSpeed = 0.02f;

        [Header("References")] 
        public Cinemachine.CinemachineFreeLook normalCamera;
        public Cinemachine.CinemachineFreeLook lockOnCamera;
        
        [Header("Movement States")]
        public float frontRayOffset = 0.5f;
        public float movementsSpeed = 2;
        public float adaptSpeed = 10;
        public float rotationSpeed;

        private PlayerInputAction _playerInputAction;
        [HideInInspector] 
        public LayerMask ignoreForGroundCheck;
        internal readonly string locomotionId = "locomotion";
        internal readonly string attackStateId = "attackState";
        internal readonly string rollingStateId = "rollingState";

        internal bool isRolling;
        private float scrollY;
        
        public override void Init()
        {
            base.Init();
            
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
                    new MonitorInteractingAnimation(this, "isInteracting", locomotionId),
                }, new List<StateAction>() //Late UPdate
                {
                });
            
            State rollingState = new State(
                new List<StateAction>() //Fixed Update
                {
                }, new List<StateAction>() //Update
                {
                    new MonitorInteractingAnimation(this, "isInteracting", locomotionId),
                }, new List<StateAction>() //Late UPdate
                {
                });

            attackState.onEnter = EnableRootMotion;
            rollingState.onEnter = EnableRootMotion;
            
            RegisterState(locomotionId, locomotion);
            RegisterState(attackStateId, attackState);
            RegisterState(rollingStateId, rollingState);
            
            ChangeState(locomotionId);

            ignoreForGroundCheck = ~(1 << 9 | 1 << 10);
            
            weaponHolderManager.Init();
            weaponHolderManager.LoadWeaponOnHook(leftWeapon, true);
            weaponHolderManager.LoadWeaponOnHook(rightWeapon, false);
        }

        #region Unity Update
        private void OnEnable()
        {
            _playerInputAction = new PlayerInputAction();
            _playerInputAction.Enable();
            _playerInputAction.GamePlay.MouseScrollY.performed += x => scrollY = x.ReadValue<float>() * cameraZoomSpeed * -1;
            
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

            if (!lockOn)
            {
                if (scrollY < 0 && normalCamera.m_Lens.FieldOfView <= 15)
                {
                    normalCamera.m_Lens.FieldOfView = 15;
                }
                else if (scrollY > 0 && normalCamera.m_Lens.FieldOfView >= 60)
                {
                    normalCamera.m_Lens.FieldOfView = 60;
                }
                else
                {
                    normalCamera.m_Lens.FieldOfView += scrollY;
                }
            }
        }
        private void Update()
        {
            if (debugLock)
            {
                debugLock = false;
                OnAssignLookOverride(target);
            }
            
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
    }
}