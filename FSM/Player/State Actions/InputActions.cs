using UnityEngine;
using UnityEngine.InputSystem;
using SK.Behavior;

namespace SK.FSM
{
    public class InputActions : StateAction
    {
        private readonly Player _player;
        private readonly PlayerInput _playerInput;

        internal readonly InputAction _Input_Move;
        internal readonly InputAction _Input_Rotate;
        internal readonly InputAction _Input_Run;
        internal readonly InputAction _Input_LAttack;
        internal readonly InputAction _Input_RAttack;
        internal readonly InputAction _Input_Interact;
        internal readonly InputAction _Input_DodgeLeft;
        internal readonly InputAction _Input_DodgeRight;
        internal readonly InputAction _Input_DodgeForward;
        internal readonly InputAction _Input_DodgeBackward;
        internal readonly InputAction _Input_Shield;

        private readonly string _inputAction_Jump = "Jump";
        private readonly string _inputAction_SwitchFightMode = "SwitchFightMode";
        private readonly string _inputAction_TargetLockOn = "TargetLockOn";
        private readonly string _inputAction_SwitchMouseMode = "SwitchMouseMode";
        private readonly string _inputAction_UI_SwitchMouseMode = "UI_SwitchMouseMode";

        private float _jumpIntervalTimer; // 점프 후 다음 점프까지의 간격 타이머
        private bool _isShield, // 쉴드를 하고 있는 지에 대한 여부
                     _isMouseMode; // 마우스 모드인 지에 대한 여부
        
        // 생성자를 통한 초기화
        public InputActions(Player player, PlayerInput playerInput)
        {
            _player = player;
            _playerInput = playerInput;

            // 인풋 액션을 변수에 할당
            _Input_Move = _playerInput.actions["Move"];
            _Input_Rotate = _playerInput.actions["Rotate"];
            _Input_Run = _playerInput.actions["Run"];
            _Input_LAttack = _playerInput.actions["LAttack"];
            _Input_RAttack = _playerInput.actions["RAttack"];
            _Input_Interact = _playerInput.actions["Interact"];
            _Input_DodgeLeft = _playerInput.actions["Dodge_Left"];
            _Input_DodgeRight = _playerInput.actions["Dodge_Right"];
            _Input_DodgeForward = _playerInput.actions["Dodge_Forward"];
            _Input_DodgeBackward = _playerInput.actions["Dodge_Backward"];
            _Input_Shield = _playerInput.actions["Shield"];

            AssignInputEvent();

            Debug.Log("InputAtions 초기화 완료");
        }

        // 인풋 이벤트 함수 할당
        public void AssignInputEvent()
        {
            // 인풋 이벤트에 동작 함수 할당
            _Input_Move.performed += HandleMovement;
            _Input_Move.canceled += HandleMovement;

            _Input_LAttack.performed += HandleAttacking;
            _Input_RAttack.performed += HandleAttacking;

            _Input_Shield.started += HandleShield;
            _Input_Shield.performed += HandleShield;
            _Input_Shield.canceled += HandleShield;

            //_Input_DodgeForward.started += HandleDodge;
            _Input_DodgeBackward.started += HandleDodge;
            _Input_DodgeLeft.started += HandleDodge;
            _Input_DodgeRight.started += HandleDodge;

            _playerInput.actions[_inputAction_Jump].started += HandleJumping;
            _playerInput.actions[_inputAction_SwitchFightMode].started += SwitchCombatMode;
            _playerInput.actions[_inputAction_TargetLockOn].started += HandleTargeting;
            _playerInput.actions[_inputAction_SwitchMouseMode].started += SwitchMouseMode;
            _playerInput.actions[_inputAction_UI_SwitchMouseMode].started += SwitchMouseMode;
        }

        // 소멸자 호출 시 인풋 이벤트 해제
        public void UnassignInputEvent()
        {
            // 인풋 이벤트에 동작 함수 할당
            _Input_Move.performed -= HandleMovement;
            _Input_Move.canceled -= HandleMovement;

            _Input_LAttack.performed -= HandleAttacking;
            _Input_RAttack.performed -= HandleAttacking;

            _Input_Shield.started -= HandleShield;
            _Input_Shield.canceled -= HandleShield;

            //_Input_DodgeForward.started -= HandleDodge;
            _Input_DodgeBackward.started -= HandleDodge;
            _Input_DodgeLeft.started -= HandleDodge;
            _Input_DodgeRight.started -= HandleDodge;

            _playerInput.actions["Jump"].started -= HandleJumping;
            _playerInput.actions["SwitchFightMode"].started -= SwitchCombatMode;
            _playerInput.actions["TargetLockOn"].started -= HandleTargeting;
            _playerInput.actions["SwitchMouseMode"].started -= SwitchMouseMode;
            _playerInput.actions["UI_SwitchMouseMode"].started -= SwitchMouseMode;

            Debug.Log("플레이어 InputActions 해제 완료");
        }

        public override void Execute()
        {
            // 점프가 가능한 간격 시간 업데이트
            if (_jumpIntervalTimer > 0)            
                _jumpIntervalTimer -= _player.fixedDeltaTime;

            // 이동하지 않거나 쉴드 시 달리기 해제
            if (_player.moveAmount == 0 || _isShield)
                _player.isRunning = false;
            // 이동 시 Run Key 누를 시 달리기 유지
            else if (_player.moveAmount > 0 && _Input_Run.IsPressed())
                _player.isRunning = true;
        }

        #region Actions
        private void HandleMovement(InputAction.CallbackContext context)
        {
            if (context.canceled)
            {
                _player.horizontal = 0;
                _player.vertical = 0;
                _player.moveAmount = 0;
                return;
            }

            // 피해를 입거나 인터렉팅 중이면 이동 불가
            if (_player.isDamaged || _player.isInteracting)
                _player.moveAmount = 0;
            else
            {
                _player.horizontal = _Input_Move.ReadValue<Vector2>().x;
                _player.vertical = _Input_Move.ReadValue<Vector2>().y;

                // 플레이어가 피해를 입었을 경우에는 움직이지 않고, 그 외엔 수평, 수직 입력 값의 합산으로 할당
                _player.moveAmount = Mathf.Clamp01(Mathf.Abs(_player.horizontal) + Mathf.Abs(_player.vertical));
            }
        }

        private void HandleJumping(InputAction.CallbackContext context)
        {
            // 지면에 있으며, 돌격 상태가 아니며, SP 소모 가능한 경우
            if (_player.isGrounded && !_player.isOnRushAttack && _player.stamina.UseSp(_player.useSp_Jump))
            {
                // 초기화
                _jumpIntervalTimer = _player.jumpDelay;
                _player.isJumping = true;

                // 애니메이션 작동
                _player.anim.SetTrigger(Strings.AnimPara_Jump);

                // 점프에 대한 이펙트 재생
                EffectManager.Instance.PlayEffect(4013, _player.mTransform);

                // 사운드 효과
                AudioManager.Instance.PlayAudio(Strings.Audio_FX_Player_Jump, _player.mTransform);
            }
        }

        private void HandleDodge(InputAction.CallbackContext context)
        {
            // 닷지 가능 상태이며, SP 소모 가능한 상태인 경우
            if (_player.CanDodge() && _player.stamina.UseSp(_player.useSp_Dodge))
            {
                bool forward = context.action == _Input_DodgeForward; // 전방 회피
                bool backward = context.action == _Input_DodgeBackward; // 후방 회피
                bool leftside = context.action == _Input_DodgeLeft; // 좌측 회피
                bool rightside = context.action == _Input_DodgeRight; // 우측 회피

                int directionAngle = 0; // DodgeState 에 전달할 회피 방향 각도 float형 변수

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

                Unshielding();

                _player.stateMachine.dodgeState.directionAngle = directionAngle;
                _player.stateMachine.ChangeState(_player.stateMachine.dodgeState);

                // 사운드 효과
                int index = Random.Range(0, Strings.Audio_FX_Player_Movement.Length);
                AudioManager.Instance.PlayAudio(Strings.Audio_FX_Player_Movement[index], _player.mTransform);
            }
        }

        private void HandleAttacking(InputAction.CallbackContext context)
        {
            // 공격 불가 상태 여부 확인 후 리턴
            if (_player.CanAttack() && _player.stamina.UseSp(_player.useSp_Attack))
            {
                // 좌측 클릭으로 좌측 장비(무기)를 사용한 공격인지에 대한 여부
                bool attackLeftSide = context.action == _Input_LAttack;

                // 장비 착용 중이지 않은 경우 리턴
                if (!_player.equipmentHolder.IsEquipedWeapon(attackLeftSide)) return;

                // 비전투모드 중에 공격 버튼을 누른 경우 착용 모션 없이 바로 장비 장착
                if (!_player.onCombatMode)
                {
                    _player.onCombatMode = true;

                    // 즉시 장비 장착
                    _player.ImmediatelyEquipWeapon();
                }

                // 공격 함수를 호출하며 특정 상황(회피, 쉴드)의 경우에 따라 특별 공격으로 호출
                if (_player.isDodge) // 회피 상태일 경우 Attack 실행
                {
                    // 전진 회피 중 공격
                    if (_player.stateMachine.dodgeState.directionAngle == 0)
                        _player.combat.BeginSpecialAttack(AttackType.ChargeAttack);
                    // 그 외 다른 방향으로 회피 중 공격
                    else
                        _player.combat.BeginSpecialAttack(AttackType.DodgeAttack);
                }
                else if (Unshielding()) // Sheild 상태일 경우 Counter Attack 실행            
                    _player.combat.BeginSpecialAttack(AttackType.CounterAttack);
                else // 일반 공격 실행(콤보 가능 여부, 공격 장비 위치-좌,우)            
                    _player.combat.BeginAttack(attackLeftSide);

                // 상태 머신을 공격 상태로 전환
                _player.stateMachine.ChangeState(_player.stateMachine.attackState);
            }
        }

        private void SwitchCombatMode(InputAction.CallbackContext context)
        {
            // 전투 모드 변경 불가 상태인지 여부 확인 후 return
            if (!_player.isGrounded || _player.isInteracting || _player.anim.GetBool(Strings.AnimPara_isChangingEquipState) || _player.isOnRushAttack) 
                return;

            // 장비 착용 중이지 않은 경우 리턴
            if (!_player.equipmentHolder.IsEquipedWeapon(true) && !_player.equipmentHolder.IsEquipedWeapon(false))
                return;

            // 전투 모드 여부 변경
            _player.onCombatMode = !_player.onCombatMode;

            // 쉴드 해제
            Unshielding();

            // 주무기 착용, 해제
            if (_player.onCombatMode)
            {
                if (_player.equipmentHolder.primaryEquipment.sheathPosition == SheathPosition.SwordSheath)
                    _player.PlayerTargetAnimation(Strings.AnimName_Equip_SwordSheath, false, 2);
                else
                    _player.PlayerTargetAnimation(Strings.AnimName_Equip_BackSheath, false, 4);
            }
            else
            {
                if (_player.equipmentHolder.primaryEquipment.sheathPosition == SheathPosition.SwordSheath)
                    _player.PlayerTargetAnimation(Strings.AnimName_Unequip_SwordSheath, false, 2);
                else
                    _player.PlayerTargetAnimation(Strings.AnimName_Unequip_BackSheath, false, 4);
            }

            // 방패 착용 여부 확인 후 착용 또는 해제 모션 실행
            if (_player.equipmentHolder.secondaryEquipment)
            {
                // 애니메이션 파라미터 변경
                _player.anim.SetBool(Strings.AnimPara_EquipShield, _player.onCombatMode);

                if (_player.equipmentHolder.secondaryEquipment.isShield)
                {
                    if (_player.onCombatMode) // 전투모드 시 착용 모션 실행
                        _player.PlayerTargetAnimation(Strings.AnimName_Equip_Shield, false, 3);
                    else // 비전투 모드 시 해제 모션 실행
                        _player.PlayerTargetAnimation(Strings.AnimName_Unequip_Shield, false, 3);
                }
            }

            _player.anim.SetBool(Strings.AnimPara_onCombat, _player.onCombatMode);
        }

        private void HandleTargeting(InputAction.CallbackContext context)
        {
            // 타겟팅 모드
            if (!_player.targeting.isTargeting)
                _player.targeting.OnAssignLookOverride();
            // 타겟팅 off
            else
                _player.targeting.OnClearLookOverride();
        }

        private void SwitchMouseMode(InputAction.CallbackContext context)
        {
            // 마우스 모드 전환
            _isMouseMode = !_isMouseMode;

            // 카메로 회전 고정
            _player.cameraManager.CameraRotatingHold(_isMouseMode);
            // 마우스 화면에 표시 전환
            GameManager.Instance.SwitchMouseState(_isMouseMode);
            // 인풋 모드 전환
            if (_isMouseMode)
                GameManager.Instance.InputManager.SwitchInputMode(InputMode.UI);
            else
                GameManager.Instance.InputManager.SwitchInputMode(InputMode.GamePlay);
        }

        private void HandleShield(InputAction.CallbackContext context)
        {
            // 방패 막기가 가능한 상황인 경우
            if (_player.CanShield())
            {
                // 액션 시작 시
                if (context.phase == InputActionPhase.Started)
                {
                    Shielding();
                }
                // 액션 유지 시
                else if (context.phase == InputActionPhase.Performed)
                {
                    if (_player.isInteracting) return;

                    Shielding();

                    var targetLayerWeight = _player.anim.GetLayerWeight(2);
                    if (targetLayerWeight < 1)
                        _player.anim.SetLayerWeight(2, targetLayerWeight + _player.fixedDeltaTime);
                }
            }

            // 액션 종료 시
            if (context.phase == InputActionPhase.Canceled)
                Unshielding();
        }

        private void Shielding()
        {
            if (!_isShield)
            {
                _isShield = true;

                // 방패 착용 중인 경우
                if (_player.equipmentHolder.secondaryEquipment && _player.equipmentHolder.secondaryEquipment.isShield)
                {
                    _player.anim.CrossFade(Strings.AnimName_Shielding, 0.15f);
                    _player.anim.SetBool(Strings.AnimPara_isShielding, true);

                }
                else
                {
                    // 주무기를 착용 중인 경우 무기로 쉴드
                    if (_player.equipmentHolder.primaryEquipment)
                    {
                        _player.anim.CrossFade(Strings.AnimName_Weapon_Block, 0.15f);
                        _player.anim.SetBool(Strings.AnimPara_isShielding, true);
                    }
                }
            }
        }

        public bool Unshielding()
        {
            if (_isShield)
            {
                _isShield = false;
                _player.anim.SetBool(Strings.AnimPara_isShielding, false);
                _player.anim.SetLayerWeight(2, 0);
                return true;
            }
            return false;
        }
        #endregion
    }
}