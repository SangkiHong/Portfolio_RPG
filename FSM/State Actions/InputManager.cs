using UnityEngine;
using UnityEngine.InputSystem;

namespace SK.FSM
{
    public class InputManager : StateAction
    {
        private readonly PlayerStateManager _state;
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
        
        private float _jumpTimer, _jumpIntervalTimer;
        private bool _inputY;
        private bool _isFightMode, _isAttacking, _isShield;
        
        public InputManager(PlayerStateManager states, PlayerInputAction playerInput)
        {
            _state = states;
            var input = playerInput;

            _Input_Move = input.GamePlay.Move;
            _Input_Rotate = input.GamePlay.Rotate;
            _Input_Run = input.GamePlay.Run;
            _Input_Jump = input.GamePlay.Jump;
            _Input_Attack = input.GamePlay.Attack;
            _Input_Interact = input.GamePlay.Interact;
            _Input_RollingHori = input.GamePlay.Rolling_Horizontal;
            _Input_RollingVert = input.GamePlay.Rolling_Vertical;
            _inputTargeting = input.GamePlay.TargetLockOn;
            _inputShield = input.GamePlay.Shield;
            _inputSwitchFightMode = input.GamePlay.SwitchFightMode;
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

            #region Jump Timer          
            // Jump Interval Timer
            if (_jumpIntervalTimer > 0) 
                _jumpIntervalTimer -= _state.fixedDelta;
            else if (_state.isGrounded && HandleJumping()) 
                return false;
            #endregion
            
            _inputY = _Input_Interact.triggered;

            // 방패 막기 시 달리기 안됨
            _state.isRunning = _isShield ? false : _Input_Run.IsPressed();
            
            _state.horizontal = _Input_Move.ReadValue<Vector2>().x;
            _state.vertical = _Input_Move.ReadValue<Vector2>().y;

            _state.moveAmount = Mathf.Clamp01(Mathf.Abs(_state.horizontal) + Mathf.Abs(_state.vertical));

            if (_isFightMode && HandleRolling()) return false;
            
            if (_state.isGrounded) retVal = HandleAttacking();

            #region Lock on Target
            if (_inputTargeting.triggered)
            {
                if (!_state.lockOn) // 타겟팅 on
                {
                    _state.targetEnemy = _state.FindLockableTarget();
                    
                    if (_state.targetEnemy) _state.OnAssignLookOverride(_state.targetEnemy);
                }
                else // 타겟팅 off
                {
                    _state.OnClearLookOverride();
                }
            }
            #endregion

            HandleShield();
            
            return retVal;
        }

        #region Hnadling Actions
        private bool HandleJumping()
        {
            if (_Input_Jump.triggered)
            {
                // Initialize
                _jumpTimer = 0;
                _jumpIntervalTimer = _state.jumpIntervalDelay;
                _state.isJumping = true;
                _state.isGrounded = false;

                // Physics
                //_state.thisRigidbody.isKinematic = false; //deprecated::Don't use Rigidbody
                //_state.thisRigidbody.AddForce(Vector3.up * _state.jumpForce, ForceMode.VelocityChange); //deprecated::Don't use Rigidbody

                // Animation
                _state.anim.SetTrigger(Strings.AnimPara_Jump);
            }

            return _state.isJumping;
        }

        private bool HandleRolling()
        {
            bool isPressed = false;
            bool isPositiveHori = Keyboard.current.dKey.wasReleasedThisFrame;
            bool isPositiveVert = Keyboard.current.wKey.wasReleasedThisFrame;
            
            if (_Input_RollingVert.triggered)
            {
                LookAtCameraDirection();
                
                // Rolling Forward
                if (isPositiveVert)
                {
                    _state.PlayerTargetAnimation(Strings.AnimName_Roll, true);
                    isPressed = true;
                }
                // Rolling Backward
                else
                {
                    _state.PlayerTargetAnimation(Strings.AnimName_RollBack, true);
                    isPressed = true;
                }
            }

            if (_Input_RollingHori.triggered)
            {
                LookAtCameraDirection();
                
                // Rolling Right
                if (isPositiveHori)
                {
                    _state.PlayerTargetAnimation(Strings.AnimName_DodgeRight, true);
                    isPressed = true;
                }
                // Rolling Left
                else
                {
                    _state.PlayerTargetAnimation(Strings.AnimName_DodgeLeft, true);
                    isPressed = true;
                }
            }
            
            if (isPressed) _state.ChangeState(PlayerStateManager.RollingStateId);

            return isPressed;
        }

        private bool HandleAttacking()
        {
            if (_state.anim.GetLayerWeight(2) >= 0.1f) return false;
            
            _isAttacking = false;
            
            if (_isFightMode && _Input_Attack.IsPressed())            
                _isAttacking = true;            
            
            MonitorFightMode();

            if (_inputY) 
                _isAttacking = false;
            

            if (_isAttacking)
            {
                // Camera Rotating to Forward
                LookAtCameraDirection();

                // Execute Attack
                _state.combat.ExecuteAttack();
                _state.ChangeState(PlayerStateManager.AttackStateId);
            }

            return _isAttacking;
        }

        private bool MonitorFightMode()
        {
            if (_state.anim.GetLayerWeight(2) != 0) return false;
            
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
                _state.anim.SetLayerWeight(2, 1);
                
                if (_isFightMode)
                {
                    // Sheath Weapon
                    _state.PlayerTargetAnimation(Strings.AnimName_Equip_Sword, false);
                }
                else
                {
                    // Unsheath Weapon
                    _state.PlayerTargetAnimation(Strings.AnimName_Unequip_Sword, false);
                }
                    
                // Unsheath or Sheath Shield
                if (_state.combat.secondaryEquipment.GetType() != typeof(Weapon) && _state.combat.secondaryEquipment)
                {
                    _state.anim.SetBool(Strings.AnimPara_isShield, _isFightMode);
                    _state.anim.SetLayerWeight(3, 1);
                    
                    if (_isFightMode)
                        _state.PlayerTargetAnimation(Strings.AnimName_Equip_Shield, false);
                    else
                        _state.PlayerTargetAnimation(Strings.AnimName_Unequip_Shield, false);
                }
                
                _state.anim.SetBool(Strings.AnimPara_isFight, _isFightMode);
            }
            
            return _isFightMode;
        }

        private bool HandleShield()
        {
            if (!_isFightMode) return false;
            
            if (_state.isGrounded && !_isAttacking && _inputShield.IsPressed())
            {
                if (!_isShield)
                {
                    _isShield = true;
                    _state.anim.CrossFade(Strings.AnimName_Shielding, 0.15f);
                    _state.anim.SetBool(Strings.AnimPara_isShielding, true);
                }
            }
            else
            {
                if (_isShield)
                {
                    _isShield = false;
                    _state.anim.SetBool(Strings.AnimPara_isShielding, false);
                }
            }
            
            return false;
        }

        private void LookAtCameraDirection()
        {
            Vector3 targetDir = _state.mainCamera.forward;
            targetDir.y = 0;
            _state.mTransform.rotation = Quaternion.LookRotation(targetDir);
        }
        #endregion
    }
}