using UnityEngine;
using UnityEngine.InputSystem;

namespace SK.FSM
{
    public class InputManager : StateAction
    {
        private readonly PlayerStateManager _state;

        private readonly InputAction _Input_Move;
        private readonly InputAction _Input_Rotate;
        private readonly InputAction _Input_Run;
        private readonly InputAction _Input_Jump;
        private readonly InputAction _Input_Attack;
        private readonly InputAction _Input_Interact;
        private readonly InputAction _Input_DodgeLeft;
        private readonly InputAction _Input_DodgeRight;
        private readonly InputAction _Input_DodgeBackward;
        private readonly InputAction _inputTargeting;
        private readonly InputAction _inputShield;
        private readonly InputAction _inputSwitchFightMode;
        
        private float _jumpIntervalTimer;
        private bool _interacting, _isFightMode, _isAttacking, _isShield;
        
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
            _Input_DodgeLeft = input.GamePlay.Dodge_Left;
            _Input_DodgeRight = input.GamePlay.Dodge_Right;
            _Input_DodgeBackward = input.GamePlay.Dodge_Backward;
            _inputTargeting = input.GamePlay.TargetLockOn;
            _inputShield = input.GamePlay.Shield;
            _inputSwitchFightMode = input.GamePlay.SwitchFightMode;
            _Input_Move.Enable();
            _Input_Rotate.Enable();
            _Input_Run.Enable();
            _Input_Jump.Enable();
            _Input_Attack.Enable();
            _Input_Interact.Enable();
            _Input_DodgeLeft.Enable();
            _Input_DodgeRight.Enable();
            _Input_DodgeBackward.Enable();
            _inputTargeting.Enable();
            _inputShield.Enable();
            _inputSwitchFightMode.Enable();
        }
        
        public override void Execute()
        {
            if (GameManager.Instance.CusorVisible) return;

            _interacting = _Input_Interact.triggered;

            #region Jump      
            // Jump Interval Timer
            if (_jumpIntervalTimer > 0)
                _jumpIntervalTimer -= _state.fixedDelta;
            else if (_state.isGrounded && HandleJumping())
                return;
            #endregion

            #region Movement Value
            // 이동 시 Run Key 누를 시 달리기 유지
            if (_state.moveAmount > 0 && _Input_Run.IsPressed())
                _state.isRunning = true;
            // 이동하지 않거나 쉴드 시 달리기 해제
            else if (_state.moveAmount == 0 || _isShield)
                _state.isRunning = false;
            
            _state.horizontal = _Input_Move.ReadValue<Vector2>().x;
            _state.vertical = _Input_Move.ReadValue<Vector2>().y;

            _state.moveAmount = Mathf.Clamp01(Mathf.Abs(_state.horizontal) + Mathf.Abs(_state.vertical));
            #endregion

            #region Targeting Button
            if (_inputTargeting.triggered)
            {
                if (!_state.isTargeting) // 타겟팅 on
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

            #region Attack, Dodge, Shield Button
            if (_state.isGrounded)
            {
                if (HandleAttacking()) return;
                if (HandleDodge()) return;

                HandleShield();
            }
            #endregion
        }

        #region Actions
        private bool HandleJumping()
        {
            if (_Input_Jump.triggered)
            {
                // Initialize
                _jumpIntervalTimer = _state.jumpIntervalDelay;
                _state.isJumping = true;
                _state.isGrounded = false;

                // Animation
                _state.anim.SetTrigger(Strings.AnimPara_Jump);
            }

            return _state.isJumping;
        }

        private bool HandleDodge()
        {
            if (!_isFightMode || _state.isDodge) return false;

            bool leftside = _Input_DodgeLeft.triggered;
            bool rightside = _Input_DodgeRight.triggered;
            bool backward = _Input_DodgeBackward.triggered;

            if (leftside || rightside)            
                _state.stateMachine.dodgeState.directionNum = leftside ? 1 : 2;            
            else if (backward)            
                _state.stateMachine.dodgeState.directionNum = 0;            

            if (leftside || rightside || backward)
            {
                if (_isShield)
                {
                    _isShield = false;
                    _state.anim.SetBool(Strings.AnimPara_isShielding, false);
                }
                _state.stateMachine.ChangeState(_state.stateMachine.dodgeState);
            }

            return leftside || rightside || backward;
        }

        private bool HandleAttacking()
        {
            // 장비 장착 해제 중이거나 공격 상태일 때 return
            if ((_state.anim.GetLayerWeight(2) >= 0.1f && !_isShield) || 
                _state.stateMachine.CurrentState == _state.stateMachine.attackState) 
                return false;
            
            _isAttacking = false;
            
            if (_isFightMode && _Input_Attack.IsPressed())            
                _isAttacking = true;            
            
            MonitorFightMode();

            if (_interacting) 
                _isAttacking = false;

            // Attack 실행
            if (_isAttacking)
            {
                if (_state.isDodge) // Dodge 상태일 경우 Dodge Attack 실행
                {
                    _state.combat.ExcuteSpecialAttack(AttackType.DodgeAttack);
                }
                else if (_isShield) // Sheild 상태일 경우 Counter Attack 실행
                {
                    _isShield = false; 
                    _state.anim.SetBool(Strings.AnimPara_isShielding, false);
                    _state.combat.ExcuteSpecialAttack(AttackType.CounterAttack);
                }
                else // 일반 콤보 공격 실행                
                    _state.combat.ExecuteAttack(_state.canComboAttack);                

                _state.stateMachine.ChangeState(_state.stateMachine.attackState);
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
            if (!_isFightMode || !_state.monitorInteracting.Execute() || _isAttacking) return false;
            
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
        #endregion
    }
}