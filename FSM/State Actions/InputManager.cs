using Sanki;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Sangki
{
    public class InputManager : StateAction
    {
        private PlayerStateManager s;
        private PlayerInputAction _playerInput;
        private InputAction _InputAction_Move;
        private InputAction _InputAction_Rotate;
        private InputAction _InputAction_Jump;
        private InputAction _InputAction_Interact;
        private InputAction _InputAction_Rolling;

        private readonly string animName_Attack1 = "Attack 1";
        private readonly string animName_Roll = "Roll_Forward";
        
        private bool y_Input;
        
        private bool isJump, isAttacking;
        
        public InputManager(PlayerStateManager states, PlayerInputAction playerInput)
        {
            s = states;
            _playerInput = playerInput;

            _InputAction_Move = _playerInput.GamePlay.Move;
            _InputAction_Rotate = _playerInput.GamePlay.Rotate;
            _InputAction_Jump = _playerInput.GamePlay.Jump;
            _InputAction_Interact = _playerInput.GamePlay.Interact;
            _InputAction_Rolling = _playerInput.GamePlay.Rolling;
            _InputAction_Move.Enable();
            _InputAction_Rotate.Enable();
            _InputAction_Jump.Enable();
            _InputAction_Interact.Enable();
            _InputAction_Rolling.Enable();
        }
        
        public override bool Execute()
        {
            isAttacking = false;
            
            s.horizontal = _InputAction_Move.ReadValue<Vector2>().x;
            s.vertical = _InputAction_Move.ReadValue<Vector2>().y;
            
            y_Input = _InputAction_Interact.triggered;
            isJump = _InputAction_Jump.triggered;
            
            s.mouseX = _InputAction_Rotate.ReadValue<Vector2>().x;
            s.mouseY = _InputAction_Rotate.ReadValue<Vector2>().y;

            s.moveAmount = Mathf.Clamp01(Mathf.Abs(s.horizontal) + Mathf.Abs(s.vertical));

            if (!isJump && HandleRolling()) return false;
            
            var retVal = HandleAttacking();
            // TODO: Jump 구현할 차례
            // if(isJump)...
            
            if (Keyboard.current.fKey.wasPressedThisFrame)
            {
                if (s.lockOn)
                {
                    s.OnClearLookOverride();
                }
                else
                {
                    s.debugLock = true;
                }
            }
            
            return retVal;
        }

        private bool HandleAttacking()
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                isAttacking = true;
            }
            
            if (y_Input)
            {
                isAttacking = false;
            }

            if (isAttacking)
            {
                //Find the actual attack animation from the items etc.
                //play animation
                s.PlayerTargetAnimation(animName_Attack1, true);
                s.ChangeState(s.attackStateId);
            }

            return isAttacking;
        }

        private bool HandleRolling()
        {
            bool isPressed = false;
            if (_InputAction_Rolling.triggered)
            {
                s.PlayerTargetAnimation(animName_Roll, true);
                s.ChangeState(s.rollingStateId);
                isPressed = true;
            }

            return isPressed;
        }
    }
}