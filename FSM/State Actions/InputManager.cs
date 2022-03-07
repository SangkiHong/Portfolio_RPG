using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;
using Sanki;

namespace SK {
    public class InputManager : StateAction
    {
        private PlayerStateManager s;
        private PlayerInputAction _playerInput;
        private InputAction _Input_Move;
        private InputAction _Input_Rotate;
        private InputAction _Input_Run;
        private InputAction _Input_Jump;
        private InputAction _Input_Attack;
        private InputAction _Input_Interact;
        private InputAction _Input_RollingHori;
        private InputAction _Input_RollingVert;
        private readonly InputAction _inputTargeting;
        private readonly InputAction _inputShield;
        private readonly InputAction _inputSwitchFightMode;

        private readonly string animBool_FightMode = "FightMode";
        private readonly string animBool_isShield = "isShield";
        private readonly string animBool_isShielding = "isShielding";
        private readonly string animTrigger_Jump = "Jump";
        private readonly string animTrigger_Land = "Land";
        private readonly string animName_Roll = "Roll_Forward";
        private readonly string animName_RollBack = "Roll_Backward";
        private readonly string animName_DodgeRight = "Dodge_Right";
        private readonly string animName_DodgeLeft = "Dodge_Left";
        private readonly string animName_Equip_Sword = "Equip_Sword";
        private readonly string animName_Unequip_Sword = "Unequip_Sword";
        private readonly string animName_Equip_Shield = "Equip_Shield";
        private readonly string animName_Unequip_Shield = "Unequip_Shield";
        private readonly string animName_Shielding = "Shielding";
        
        private float _jumpTimer, _jumpIntervalTimer;
        private bool _inputY;
        private bool _isFightMode, _isAttacking, _isShield;
        
        public InputManager(PlayerStateManager states, PlayerInputAction playerInput)
        {
            s = states;
            _playerInput = playerInput;

            _Input_Move = _playerInput.GamePlay.Move;
            _Input_Rotate = _playerInput.GamePlay.Rotate;
            _Input_Run = _playerInput.GamePlay.Run;
            _Input_Jump = _playerInput.GamePlay.Jump;
            _Input_Attack = _playerInput.GamePlay.Attack;
            _Input_Interact = _playerInput.GamePlay.Interact;
            _Input_RollingHori = _playerInput.GamePlay.Rolling_Horizontal;
            _Input_RollingVert = _playerInput.GamePlay.Rolling_Vertical;
            _inputTargeting = _playerInput.GamePlay.TargetLockOn;
            _inputShield = _playerInput.GamePlay.Shield;
            _inputSwitchFightMode = _playerInput.GamePlay.SwitchFightMode;
            _Input_Move.Enable();
            _Input_Rotate.Enable();
            _Input_Run.Enable();
            _Input_Jump.Enable();
            _Input_Attack.Enable();
            _Input_Interact.Enable();
            _Input_RollingHori.Enable();
            _Input_RollingVert.Enable();
            _inputTargeting.Enable();
            _inputShield.Enable();
            _inputSwitchFightMode.Enable();
        }
        
        public override bool Execute()
        {
            var retVal = false;

            #region Jump
            // Jump Landing Check
            if (s.isJump && IsCheckGrounded())
            {
                if (_jumpTimer < 0.03f) _jumpTimer += s.delta;
                else
                {
                    s.anim.SetTrigger(animTrigger_Land);
                    s.isJump = false;
                }
            }
            
            // Jump Interval Timer
            if (_jumpIntervalTimer > 0) _jumpIntervalTimer -= s.delta;
            else if (s.isGrounded && HandleJumping()) return false;
            #endregion
            
            _inputY = _Input_Interact.triggered;
            s.isRun = _Input_Run.IsPressed();
            
            s.horizontal = _Input_Move.ReadValue<Vector2>().x;
            s.vertical = _Input_Move.ReadValue<Vector2>().y;
            
            s.mouseX = _Input_Rotate.ReadValue<Vector2>().x;
            s.mouseY = _Input_Rotate.ReadValue<Vector2>().y;

            s.moveAmount = Mathf.Clamp01(Mathf.Abs(s.horizontal) + Mathf.Abs(s.vertical));

            if (_isFightMode && HandleRolling()) return false;
            
            if (s.isGrounded) retVal = HandleAttacking();
            
            // Lock on Target
            if (_inputTargeting.triggered)
            {
                if (s.lockOn)
                {
                    s.OnClearLookOverride();
                }
                else
                {
                    s.target = s.FindLockableTarget();
                    
                    if (s.target) s.OnAssignLookOverride(s.target);
                }
            }

            HandleShield();
            
            return retVal;
        }

        #region Hnadling Actions
        private bool HandleJumping()
        {
            if (_Input_Jump.triggered)
            {
                // Animation
                s.anim.SetTrigger(animTrigger_Jump);
                s.isJump = true;
                s.isGrounded = false;
                s.rigidbody.isKinematic = false;
                s.rigidbody.AddForce(Vector3.up * (s.jumpForce * -Physics.gravity.y));

                // Feedback
                //s.feedback_Jump?.PlayFeedbacks();

                // Initialize
                _jumpTimer = 0;
                _jumpIntervalTimer = s.jumpIntervalDelay;
            }

            return s.isJump;
        }

        private bool HandleRolling()
        {
            bool isPressed = false;
            bool isPositiveHori = Keyboard.current.dKey.wasReleasedThisFrame;
            bool isPositiveVert = Keyboard.current.wKey.wasReleasedThisFrame;
            
            if (_Input_RollingVert.triggered)
            {
                // Rolling Forward
                if (isPositiveVert)
                {
                    s.PlayerTargetAnimation(animName_Roll, true);
                    isPressed = true;
                }
                // Rolling Backward
                else
                {
                    s.PlayerTargetAnimation(animName_RollBack, true);
                    isPressed = true;
                }
            }

            if (_Input_RollingHori.triggered)
            {
                Vector3 targetDir = s.mainCamera.forward;
                targetDir.y = 0;
                s.mTransform.rotation = Quaternion.LookRotation(targetDir);
                
                // Rolling Right
                if (isPositiveHori)
                {
                    s.PlayerTargetAnimation(animName_DodgeRight, true);
                    isPressed = true;
                }
                // Rolling Left
                else
                {
                    s.PlayerTargetAnimation(animName_DodgeLeft, true);
                    isPressed = true;
                }
            }
            
            if (isPressed) s.ChangeState(PlayerStateManager.RollingStateId);

            return isPressed;
        }

        private bool HandleAttacking()
        {
            if (s.anim.GetLayerWeight(2) >= 0.1f) return false;
            
            _isAttacking = false;
            
            if (_isFightMode && _Input_Attack.triggered)
            {
                _isAttacking = true;
            }
            
            MonitorFightMode();

            if (_inputY)
            {
                _isAttacking = false;
            }

            if (_isAttacking)
            {
                // Camera Rotating to Forward
                Vector3 targetDir = s.mainCamera.forward;
                targetDir.y = 0;
                s.mTransform.DOLocalRotateQuaternion(Quaternion.LookRotation(targetDir), 0.5f);
                //s.mTransform.rotation = Quaternion.LookRotation(targetDir);
                
                // play animation
                s.currentWeaponInUse.ExcuteAction();
            }

            return _isAttacking;
        }

        private bool MonitorFightMode()
        {
            if (s.anim.GetLayerWeight(2) != 0) return false;
            
            bool changed = false;
            
            if (!_isFightMode && _Input_Attack.triggered)
            {
                _isFightMode = true;
                changed = true;
            }

            if (_inputSwitchFightMode.triggered)
            {
                _isFightMode = !_isFightMode;
                changed = true;
            }

            if (changed)
            {
                // Animations
                s.anim.SetLayerWeight(2, 1);
                
                if (_isFightMode)
                {
                    // Sheath Weapon
                    s.PlayerTargetAnimation(animName_Equip_Sword, false);
                }
                else
                {
                    // Unsheath Weapon
                    s.PlayerTargetAnimation(animName_Unequip_Sword, false);
                }
                    
                // Unsheath or Sheath Shield
                if (s.secondaryEquipment)
                {
                    s.anim.SetBool(animBool_isShield, _isFightMode);
                    s.anim.SetLayerWeight(3, 1);
                    
                    if (_isFightMode)
                        s.PlayerTargetAnimation(animName_Equip_Shield, false);
                    else
                        s.PlayerTargetAnimation(animName_Unequip_Shield, false);
                }
                
                s.anim.SetBool(animBool_FightMode, _isFightMode);
            }
            
            return _isFightMode;
        }

        private bool HandleShield()
        {
            if (!_isFightMode) return false;
            
            if (s.isGrounded && !_isAttacking && _inputShield.IsPressed())
            {
                if (!_isShield)
                {
                    _isShield = true;
                    s.anim.SetLayerWeight(2, 1);
                    s.PlayerTargetAnimation(animName_Shielding, false);
                    s.anim.SetBool(animBool_isShielding, true);
                }
            }
            else
            {
                if (_isShield)
                {
                    _isShield = false;
                    s.anim.SetLayerWeight(2, 0);
                    s.anim.SetBool(animBool_isShielding, false);
                }
            }
            
            return false;
        }

        private bool IsCheckGrounded()
        {
            bool groundCheck = false;
            
            var maxDistance = 0.32f;

            var position = s.mTransform.position;
            
            var ray1 = new Ray(position + Vector3.up * 0.2f, Vector3.down * maxDistance);

            Debug.DrawRay(position + Vector3.up * 0.2f, Vector3.down * maxDistance, Color.magenta);
            groundCheck = Physics.Raycast(ray1, maxDistance, s.ignoreForGroundCheck);
            s.isGrounded = groundCheck;
            
            return groundCheck;
        }
        #endregion
    }
}