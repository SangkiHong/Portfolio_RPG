using UnityEngine;
using UnityEngine.InputSystem;

namespace SK.FSM
{
    public class InputActions : StateAction
    {
        private readonly PlayerStateManager _state;

        private readonly InputAction _Input_Move;
        private readonly InputAction _Input_Rotate;
        private readonly InputAction _Input_Run;
        private readonly InputAction _Input_Jump;
        private readonly InputAction _Input_LAttack;
        private readonly InputAction _Input_RAttack;
        private readonly InputAction _Input_Interact;
        private readonly InputAction _Input_DodgeLeft;
        private readonly InputAction _Input_DodgeRight;
        private readonly InputAction _Input_DodgeForward;
        private readonly InputAction _Input_DodgeBackward;
        private readonly InputAction _Input_Targeting;
        private readonly InputAction _Input_Shield;
        private readonly InputAction _Input_SwitchFightMode;
        private readonly InputAction _Input_SwitchMouseMode;
        
        private float _jumpIntervalTimer;
        private bool _interacting, isChangeCombatMode, _isCombatMode, _isShield, _isPressedMouseMode, _isPressedTargeting;
        
        public InputActions(PlayerStateManager states, PlayerInputAction playerInput)
        {
            _state = states;
            var input = playerInput;

            _Input_Move = input.GamePlay.Move;
            _Input_Rotate = input.GamePlay.Rotate;
            _Input_Run = input.GamePlay.Run;
            _Input_Jump = input.GamePlay.Jump;
            _Input_LAttack = input.GamePlay.LAttack;
            _Input_RAttack = input.GamePlay.RAttack;
            _Input_Interact = input.GamePlay.Interact;
            _Input_DodgeLeft = input.GamePlay.Dodge_Left;
            _Input_DodgeRight = input.GamePlay.Dodge_Right;
            _Input_DodgeForward = input.GamePlay.Dodge_Forward;
            _Input_DodgeBackward = input.GamePlay.Dodge_Backward;
            _Input_Targeting = input.GamePlay.TargetLockOn;
            _Input_Shield = input.GamePlay.Shield;
            _Input_SwitchFightMode = input.GamePlay.SwitchFightMode;
            _Input_SwitchMouseMode = input.GamePlay.SwitchMouseMode;
        }
        
        public override void Execute()
        {
            #region Mouse Mode
            // 마우스 모드 전환
            if (!_isPressedMouseMode && _Input_SwitchMouseMode.WasPressedThisFrame())
            {
                _isPressedMouseMode = true;
                _state.cameraManager.CameraRotateSwtich(GameManager.Instance.SwitchMouseState());
                return;
            }
            else if (_isPressedMouseMode && _Input_SwitchMouseMode.WasReleasedThisFrame())            
                _isPressedMouseMode = false;

            // 마우스 모드인 경우 return
            if (GameManager.Instance.CusorVisible) return;
            #endregion

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
            // 플레이어가 피해를 입었을 경우에는 움직이지 않고, 그 외엔 수평, 수직 입력 값의 합산으로 할당
            _state.moveAmount = _state.isDamaged ? 0 : Mathf.Clamp01(Mathf.Abs(_state.horizontal) + Mathf.Abs(_state.vertical));
            #endregion

            #region Targeting Button
            if (!_isPressedTargeting && _Input_Targeting.WasPressedThisFrame())
            {
                _isPressedTargeting = true;
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
            else if (_isPressedTargeting && _Input_Targeting.WasReleasedThisFrame())
                _isPressedTargeting = false;
            #endregion

            #region Attack, Dodge, Shield Button
            if (_state.isGrounded)
            {
                if (_Input_SwitchFightMode.triggered && SwitchCombatMode()) return;
                if (HandleAttacking()) return;
                if (HandleDodge()) return;

                HandleShield();

                // Monitoring Change Combat Mode
                if (isChangeCombatMode && !_state.anim.GetBool(Strings.AnimPara_isChangingEquipState))
                    isChangeCombatMode = false;
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
            if (!_isCombatMode || _state.isDodge) return false;

            bool forward = _Input_DodgeForward.triggered; // 전방 회피
            bool backward = _Input_DodgeBackward.triggered; // 후방 회피
            bool leftside = _Input_DodgeLeft.triggered; // 좌측 회피
            bool rightside = _Input_DodgeRight.triggered; // 우측 회피

            var directionAngle = 0; // DodgeState 에 전달할 회피 방향 각도 float형 변수

            if (forward)
                directionAngle = 0; // 전방 회피일 경우
            else if (backward)
            {
                bool isPressedLeft = Keyboard.current.aKey.isPressed; // 후방 회피 중 좌측 방향키가 눌러졌는지 확인
                bool isPressedRight = Keyboard.current.dKey.isPressed; // 후방 회피 중 우측 방향키가 눌러졌는지 확인

                if (isPressedLeft) directionAngle = 225; // 후방 좌측 방향 각
                else if (isPressedRight) directionAngle = 135; // 후방 우측 방향 각
                else directionAngle = 180; // 후방 각
            }
            else if (leftside || rightside)
                directionAngle = leftside ? 270 : 90; // 좌우 각

            if (forward || backward || leftside || rightside)
            {
                Unshielding();

                _state.stateMachine.dodgeState.directionAngle = directionAngle;
                _state.stateMachine.ChangeState(_state.stateMachine.dodgeState);
            }

            return forward || backward || leftside || rightside;
        }


        private bool HandleAttacking()
        {
            if (_Input_LAttack.IsPressed() || _Input_RAttack.IsPressed())
            {
                bool attackLeftSide = _Input_LAttack.IsPressed();

                // 콤보 공격 불가능 상태 & 장비 장착 해제 중이거나 공격 상태일 때 return
                if (!_state.canComboAttack && (_state.anim.GetBool(Strings.AnimPara_isChangingEquipState) ||
                    _state.stateMachine.CurrentState == _state.stateMachine.attackState))
                    return false;

                // 장비 장착 중이 아닐 시 즉시 장착
                if (!_isCombatMode)
                {
                    _isCombatMode = true;

                    // 주무기 착용
                    _state.equipmentHolder.Equip();

                    // 보조장비 착용
                    if (_state.equipmentHolder.secondaryEquipment)
                        _state.equipmentHolder.Equip(1);

                    // 애니메이션 전투 상태로 전환
                    _state.anim.SetBool(Strings.AnimPara_onCombat, true);

                    // 방패 착용 여부 확인하여 애니메이션 파라미터 변경
                    if (_state.equipmentHolder.secondaryEquipment && 
                        _state.equipmentHolder.secondaryEquipment.equipType == EquipType.Shield)
                        _state.anim.SetBool(Strings.AnimPara_EquipShield, true);
                }

                // Attack 실행
                if (_state.isDodge) // 회피 상태일 경우 Attack 실행
                {
                    if (_state.stateMachine.dodgeState.directionAngle == 0) // 전진 회피 중 공격
                        _state.combat.ExcuteSpecialAttack(AttackType.ChargeAttack);
                    else // 그 외 다른 방향으로 회피 중 공격
                        _state.combat.ExcuteSpecialAttack(AttackType.DodgeAttack);
                }
                else if (Unshielding()) // Sheild 상태일 경우 Counter Attack 실행
                {
                    _state.combat.ExcuteSpecialAttack(AttackType.CounterAttack);
                }
                else // 일반 공격 실행(콤보 가능 여부, 공격 장비 위치-좌,우)
                    _state.combat.ExecuteAttack(_state.canComboAttack, attackLeftSide);

                _state.stateMachine.ChangeState(_state.stateMachine.attackState);

                _state.canComboAttack = false; // 콤보 공격 가능 상태 초기화

                return true;
            }

            return false;
        }

        private bool SwitchCombatMode()
        {
            // 장비 착용, 해제 중일 시 즉시 true return
            if (isChangeCombatMode || _state.anim.GetBool(Strings.AnimPara_isChangingEquipState)) return true;

            // 인터렉팅 중일 경우 flase return
            if (_state.isInteracting) return false;

            isChangeCombatMode = true;

            // 쉴드 해제
            Unshielding();

            _isCombatMode = !_isCombatMode;
                        
            // Animator Layer 변경
            _state.anim.SetLayerWeight(2, 1);
                
            // 주무기 착용, 해제
            if (_isCombatMode)
                _state.PlayerTargetAnimation(Strings.AnimName_Equip_Sword, false);            
            else
                _state.PlayerTargetAnimation(Strings.AnimName_Unequip_Sword, false);            
                    
            // 방패 착용, 해제
            if (_state.equipmentHolder.secondaryEquipment && _state.equipmentHolder.secondaryEquipment is Weapon)
            {
                _state.anim.SetBool(Strings.AnimPara_EquipShield, _isCombatMode);
                _state.anim.SetLayerWeight(3, 1);
                    
                if (_isCombatMode)
                    _state.PlayerTargetAnimation(Strings.AnimName_Equip_Shield, false);
                else
                    _state.PlayerTargetAnimation(Strings.AnimName_Unequip_Shield, false);
            }
                
            _state.anim.SetBool(Strings.AnimPara_onCombat, _isCombatMode);

            return true;
        }

        private bool HandleShield()
        {
            // 장비 착용, 해제 중일 경우 return
            if (_state.anim.GetBool(Strings.AnimPara_isChangingEquipState)) return false;

            if (_state.isDamaged && Unshielding())            
                return false; 
            
            if (!_isCombatMode || _state.isInteracting) return false;
            
            if (_state.isGrounded && _Input_Shield.IsPressed())
            {
                if (!_isShield)
                {
                    _isShield = true;
                    _state.anim.CrossFade(Strings.AnimName_Shielding, 0.15f);
                    _state.anim.SetBool(Strings.AnimPara_isShielding, true);
                }
            }
            else            
                Unshielding();            
            
            return false;
        }

        private bool Unshielding()
        {
            if (_isShield)
            {
                _isShield = false;
                _state.anim.SetBool(Strings.AnimPara_isShielding, false);
                _state.anim.SetLayerWeight(2, 0);
                return true;
            }
            return false;
        }
        #endregion
    }
}