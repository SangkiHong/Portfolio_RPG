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
        private InputAction _InputAction_RollingHorizontal;
        private InputAction _InputAction_RollingVertical;

        private readonly string animBool_FightMode = "FightMode";
        private readonly string animTrigger_Jump = "Jump";
        private readonly string animName_Attack1 = "Attack 1";
        private readonly string animName_Roll = "Roll_Forward";
        private readonly string animName_RollBack = "Roll_Backward";
        private readonly string animName_DodgeRight = "Dodge_Right";
        private readonly string animName_DodgeLeft = "Dodge_Left";
        private readonly string animName_Equip_Sword = "Equip_Sword";
        private readonly string animName_Unequip_Sword = "Unequip_Sword";

        private float fightModeDuration = 15;
        
        private float jumpTimer, jumpIntervalTimer, fightModeTimer, layerWeight;
        private bool input_Y;
        private bool isFightMode, isAttacking;
        
        public InputManager(PlayerStateManager states, PlayerInputAction playerInput)
        {
            s = states;
            _playerInput = playerInput;

            _InputAction_Move = _playerInput.GamePlay.Move;
            _InputAction_Rotate = _playerInput.GamePlay.Rotate;
            _InputAction_Jump = _playerInput.GamePlay.Jump;
            _InputAction_Interact = _playerInput.GamePlay.Interact;
            _InputAction_RollingHorizontal = _playerInput.GamePlay.Rolling_Horizontal;
            _InputAction_RollingVertical = _playerInput.GamePlay.Rolling_Vertical;
            _InputAction_Move.Enable();
            _InputAction_Rotate.Enable();
            _InputAction_Jump.Enable();
            _InputAction_Interact.Enable();
            _InputAction_RollingHorizontal.Enable();
            _InputAction_RollingVertical.Enable();
        }
        
        public override bool Execute()
        {
            var retVal = false;
            
            // Jump Landing Check
            if (s.isJump && IsCheckGrounded())
            {
                if (jumpTimer < 0.03f) jumpTimer += s.delta;
                else
                {
                    s.anim.SetTrigger("Land");
                    s.isJump = false;
                }
            }
            
            // Jump Interval Timer
            if (jumpIntervalTimer > 0) jumpIntervalTimer -= s.delta;
            else if (s.isGrounded && HandleJumping()) return false;
            
            input_Y = _InputAction_Interact.triggered;
            s.isRun = Keyboard.current.leftShiftKey.isPressed;
            
            s.horizontal = _InputAction_Move.ReadValue<Vector2>().x;
            s.vertical = _InputAction_Move.ReadValue<Vector2>().y;
            
            s.mouseX = _InputAction_Rotate.ReadValue<Vector2>().x;
            s.mouseY = _InputAction_Rotate.ReadValue<Vector2>().y;

            s.moveAmount = Mathf.Clamp01(Mathf.Abs(s.horizontal) + Mathf.Abs(s.vertical));

            if (isFightMode && HandleRolling()) return false;
            
            if (s.isGrounded) retVal = HandleAttacking();
            
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

        private bool HandleJumping()
        {
            if (_InputAction_Jump.triggered)
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
                jumpTimer = 0;
                jumpIntervalTimer = s.jumpIntervalDelay;
            }

            return s.isJump;
        }

        private bool HandleAttacking()
        {
            isAttacking = false;
            
            if (isFightMode && Mouse.current.leftButton.wasPressedThisFrame)
            {
                isAttacking = true;
            }
            
            MonitorFightMode();

            if (s.anim.GetLayerWeight(2) != 0) return false;
            
            if (input_Y)
            {
                isAttacking = false;
            }

            if (isAttacking)
            {
                Vector3 _targetDir = s.mainCamera.forward;
                _targetDir.y = 0;
                s.mTransform.rotation = Quaternion.LookRotation(_targetDir);
                
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
            bool isPositive_Hori = Keyboard.current.dKey.wasReleasedThisFrame;
            bool isPositive_Vert = Keyboard.current.wKey.wasReleasedThisFrame;
            
            if (_InputAction_RollingVertical.triggered)
            {
                // Rolling Forward
                if (isPositive_Vert)
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

            if (_InputAction_RollingHorizontal.triggered)
            {
                Vector3 _targetDir = s.mainCamera.forward;
                _targetDir.y = 0;
                s.mTransform.rotation = Quaternion.LookRotation(_targetDir);
                
                // Rolling Right
                if (isPositive_Hori)
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
            
            if (isPressed) s.ChangeState(s.rollingStateId);

            return isPressed;
        }

        private bool MonitorFightMode()
        {
            bool changed = false;
            
            if (!isFightMode && Mouse.current.leftButton.wasPressedThisFrame)
            {
                isFightMode = true;
                changed = true;
            }

            if (Keyboard.current.tabKey.wasPressedThisFrame)
            {
                isFightMode = !isFightMode;
                changed = true;
            }

            if (changed)
            {
                // Animations
                s.anim.SetBool(animBool_FightMode, isFightMode);
                s.anim.SetLayerWeight(2, 1);
                if (isFightMode)
                {
                    s.PlayerTargetAnimation(animName_Equip_Sword, false);
                }
                else
                {
                    s.PlayerTargetAnimation(animName_Unequip_Sword, false);
                }
            }
            
            return isFightMode;
        }

        private bool IsCheckGrounded()
        {
            bool groundCheck = false;
            
            var maxDistance = 0.12f;

            var position = s.mTransform.position;
            
            var ray1 = new Ray(position, Vector3.down * maxDistance);

            Debug.DrawRay(position, Vector3.down * maxDistance, Color.magenta);
            groundCheck = Physics.Raycast(ray1, maxDistance, s.ignoreForGroundCheck);
            s.isGrounded = groundCheck;
            
            return groundCheck;
        }
    }
}