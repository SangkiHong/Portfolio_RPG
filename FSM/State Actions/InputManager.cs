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
        private InputAction _InputAction_Interact;

        private bool y_Input;

        private bool isAttacking;
        
        public InputManager(PlayerStateManager states, PlayerInputAction playerInput)
        {
            s = states;
            _playerInput = playerInput;

            _InputAction_Move = _playerInput.GamePlay.Move;
            _InputAction_Rotate = _playerInput.GamePlay.Rotate;
            _InputAction_Interact = _playerInput.GamePlay.Interact;
            _InputAction_Move.Enable();
            _InputAction_Rotate.Enable();
            _InputAction_Interact.Enable();
        }
        
        public override bool Execute()
        {
            bool retVal = false;
            
            isAttacking = false;
            
            s.horizontal = _InputAction_Move.ReadValue<Vector2>().x;
            s.vertical = _InputAction_Move.ReadValue<Vector2>().y;
            
            y_Input = _InputAction_Interact.triggered;
            
            s.mouseX = _InputAction_Rotate.ReadValue<Vector2>().x;
            s.mouseY = _InputAction_Rotate.ReadValue<Vector2>().y;

            s.moveAmount = Mathf.Clamp01(Mathf.Abs(s.horizontal) + Mathf.Abs(s.vertical));

            retVal = HandleAttacking();

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
                s.PlayerTargetAnimation("Attack 1", true);
                s.ChangeState(s.attackStateId);
            }

            return isAttacking;
        }
    }
}