using UnityEngine;
using SK.Utilities;

namespace SK.FSM
{
    public class MoveCharacter : StateAction
    {
        private readonly PlayerStateManager _state;
        
        private RaycastHit _raycastHit;
        private Vector3 _targetVelocity, _origin, _targetDir;
        private Quaternion _targetRotation;

        private readonly int _animHashForward = Animator.StringToHash("Forward");
        private readonly int _animHashSideways = Animator.StringToHash("Sideways");

        private float _frontY, _speed, _moveAmount, _elapsed;

        public MoveCharacter(PlayerStateManager psm) => _state = psm;        

        public override void Execute()
        {
            _moveAmount = _state.moveAmount;

            // 피격되지 않은 채 점프 상태인 경우
            if (_state.isJumping && !_state.isDamaged)
                HandleJump();
            else
            {
                HandleMovement();
                HandleRotation();
            }

            HandleAnimations();
        }

        private void HandleMovement()
        {
            Vector3 tranformPos = _state.mTransform.position;

            // 피격 상태가 아닌 경우
            if (!_state.isDamaged)
            {
                if (_moveAmount > 0.1f) // Move
                {
                    _speed = _state.isRunning ? _state.runSpeed : _state.movementsSpeed; // 달릴 시 속도 변경

                    // Normal Movement
                    if (!_state.isTargeting)
                    {
                        float overrideMove = _moveAmount;
                        if (_state.vertical < 0)
                        {
                            overrideMove *= -0.7f; // 뒷걸음 시 뒤로 70%로 감속
                        }
                        _targetVelocity = _state.mTransform.forward * overrideMove * _speed;
                    }
                    // Lock on Movement
                    else
                    {
                        _speed *= 0.8f; // 움직임 스피드 80%로 감속
                        _targetVelocity = _state.mTransform.forward * _state.vertical * _speed;
                        _targetVelocity += _state.mTransform.right * _state.horizontal * _speed;
                    }


                    _origin = tranformPos + (_targetVelocity * _state.frontRayOffset);
                    _origin.y += _state.frontRayOffsetHeight;

                    Debug.DrawRay(_origin, -Vector3.up * (_state.frontRayOffsetHeight + 1), Color.red);
                    if (Physics.Raycast(_origin, -Vector3.up, out _raycastHit, _state.frontRayOffsetHeight + 1, _state.groundLayerMask))
                        _frontY = _raycastHit.point.y - tranformPos.y;

                    // Idle, Move, on Air State
                    if (_state.isGrounded)
                    {
                        float absY = Mathf.Abs(_frontY);

                        // Slope
                        if (absY < 0.2f)
                        {
                            if (_moveAmount > 0)
                            {
                                if (absY > 0.02f)
                                    _targetVelocity.y = _frontY * 2f * _speed;
                            }
                        }
                        else if (absY < 1f) // 높이 차 1 미만의 경사 지형에 대한 계산
                        {
                            var dir = _raycastHit.point - tranformPos;
                            var angle = Vector3.Angle(dir, _state.mTransform.forward);
                            if (angle > _state.slopeLimitAngle)
                            {
                                _state.isSlipping = true;
                            }
                        }
                    }
                }
                else
                    _targetVelocity = Vector3.zero;
            }
            // 피격 상태인 경우
            else
            {
                _targetVelocity = Vector3.zero;
            }
            Gravity();

            Debug.DrawRay((tranformPos + Vector3.up * 0.2f), _targetVelocity, Color.green, 0.01f, false);
            //_state.mTransform.position = Vector3.MoveTowards(tranformPos, tranformPos + _targetVelocity, _state.fixedDelta * _state.adaptSpeed);
            _state.characterController.Move(_targetVelocity);
        }
        
        private void HandleRotation()
        {
            float moveOverride = _state.moveAmount;
            float forward = _state.vertical;
            float sideways = _state.horizontal;

            if (_state.isTargeting)
            {
                _targetDir = _state.targetEnemy.position - _state.mTransform.position;
                moveOverride = 1;
                
                _targetDir.Normalize();
                _targetDir.y = 0;
                if (_targetDir == Vector3.zero)
                    _targetDir = _state.transform.forward;
        
                _targetRotation = Quaternion.LookRotation(_targetDir);
            }
            else
            {                
                _targetDir = _state.cameraManager.transform.forward * forward;
                _targetDir += _state.cameraManager.transform.right * sideways;
                if (forward < 0) _targetDir *= -1;
            
                _targetDir.Normalize();
                _targetDir.y = 0;
                if (_targetDir == Vector3.zero)
                    _targetDir = _state.transform.forward;
            
                _targetRotation = Quaternion.LookRotation(_targetDir);
            }
                        
            _state.mTransform.rotation = Quaternion.Slerp(
                                        _state.mTransform.rotation, _targetRotation,
                                        _state.fixedDelta * moveOverride * _state.rotationSpeed);
        }

        private void HandleJump()
        {
            // Landing Check
            if (_state.isJumping && _state.isGrounded)
            {
                // 초기화
                _elapsed = 0;
                _state.isJumping = false;
                _state.anim.SetTrigger(Strings.AnimPara_Land);
                return;
            }

            if (_elapsed < 1)
                _elapsed += _state.fixedDelta * _state.jumpDuration;
            else
                _elapsed = 1;

            _targetVelocity.y = MyMath.Instance.GetSine((uint)Mathf.RoundToInt(_elapsed * 270)) * (1 + _speed) * _state.jumpForce; 
            _state.characterController.Move(_targetVelocity);
        }

        private void HandleAnimations()
        {
            if (_state.isGrounded)
            {
                if (_state.isTargeting)
                {
                    float v = Mathf.Abs(_state.vertical);
                    float f = 0;
                    if (v > 0 && v < 0.5f) 
                        f = 0.5f;
                    else if (v > 0.5f) 
                        f = 1;

                    if (_state.vertical < 0) f = -f;
                    
                    if (!_state.isRunning)
                    {
                        if (f > 0.5f) f = 0.5f;
                        else if (f < -0.5f) f = -0.5f;
                    }
                
                    _state.anim.SetFloat(_animHashForward, f, 0.2f, _state.fixedDelta);
                    
                    float h = Mathf.Abs(_state.horizontal);
                    float s = 0;
                    if (h > 0 && h < 0.5f) 
                        s = 0.5f;
                    else if (h > 0.5f) 
                        s = 1;

                    if (_state.horizontal < 0) s = -1;
                    
                    if (!_state.isRunning)
                    {
                        if (s > 0.5f) s = 0.5f;
                        else if (s < -0.5f) s = -0.5f;
                    }
                
                    _state.anim.SetFloat(_animHashSideways, s, 0.2f, _state.fixedDelta);
                }
                else
                {
                    float f = 0;
                    if (_moveAmount > 0 && _moveAmount < 0.5f) 
                        f = 0.5f;
                    else if (_moveAmount > 0.5f) 
                        f= 1;
                    if (_state.vertical < 0) f *= -1;
                    
                    if (!_state.isRunning)
                    {
                        if (f > 0.5f) f = 0.5f;
                        else if (f < -0.5f) f = -0.5f;
                    }
                    _state.anim.SetFloat(_animHashForward, f, 0.2f, _state.fixedDelta);
                    _state.anim.SetFloat(_animHashSideways, 0, 0.2f, _state.fixedDelta);
                }
            }
            
        }

        private void Gravity()
        {
            if (!_state.isGrounded)
                _targetVelocity.y = -_state.gravity;
            else
                _targetVelocity.y = 0;
        }
    }
}