using UnityEngine;

namespace SK.FSM
{
    public class MovePlayerCharacter : StateAction
    {
        private readonly PlayerStateManager _states;
        
        private RaycastHit _raycastHit;
        private Vector3 _currentVelocity, _targetVelocity, _origin, _targetDir;
        private Quaternion _targetRotation;

        private bool _isJumpPeek;
        private float _frontY, _speed;
        private readonly int _animHashForward = Animator.StringToHash("Forward");
        private readonly int _animHashSideways = Animator.StringToHash("Sideways");

        public MovePlayerCharacter(PlayerStateManager playerStateManager)
        {
            _states = playerStateManager;
        }

        public override bool Execute()
        {
            Debug.Log("MovePlayerCHaracter Called");
            // Jump Landing & Ground Check
            if (_states.isGrounded && _states.isJumping)
            {
                _isJumpPeek = false;
                _states.isJumping = false;
                _states.anim.SetTrigger(Strings.AnimPara_Land);
            }

            _frontY = 0;
            _speed = _states.isRunning ? _states.runSpeed : _states.movementsSpeed; // Change Speed on Running

            // Normal Movement
            if (!_states.lockOn)
            {
                float moveAmount = _states.moveAmount;
                if (_states.vertical < 0)
                {
                    moveAmount *= -0.85f; // 뒷걸음 시 뒤로 85%로 감속
                }
                _targetVelocity = _states.mTransform.forward * moveAmount * _speed;
            }
            // Lock on Movement
            else
            {
                _speed *= 0.8f; // 움직임 스피드 80%로 감속
                _targetVelocity = _states.mTransform.forward * _states.vertical * _speed;
                _targetVelocity += _states.mTransform.right * _states.horizontal * _speed;
            }
            
            _origin = _states.mTransform.position + (_targetVelocity.normalized * _states.frontRayOffset);
            _origin.y += _states.frontRayOffsetHeight;
            
            Debug.DrawRay(_origin, -Vector3.up * (_states.frontRayOffsetHeight + 1), Color.red);
            if (Physics.Raycast(_origin, -Vector3.up, out _raycastHit, _states.frontRayOffsetHeight + 1, _states.groundLayerMask))            
                _frontY = _raycastHit.point.y - _states.mTransform.position.y;

            //_currentVelocity = _states.thisRigidbody.velocity; //deprecated::Don't use Rigidbody

            // Idle, Move, on Air State
            if (_states.isGrounded)
            {
                float moveAmount = _states.moveAmount;
                float absY = Mathf.Abs(_frontY);

                // Move State
                if (moveAmount > 0.1f)
                {
                    //_states.thisRigidbody.isKinematic = false; //deprecated::Don't use Rigidbody
                    //_states.thisRigidbody.drag = 0; //deprecated::Don't use Rigidbody

                    HandleRotation();
                }
                // Stop State
                else
                {
                    //_states.thisRigidbody.isKinematic = true; //deprecated::Don't use Rigidbody
                    //_states.thisRigidbody.drag = 4; //deprecated::Don't use Rigidbody
                }

                // Slope
                if (absY < 0.2f)
                {
                    if (moveAmount > 0)
                    {
                        if (absY > 0.02f)
                            _targetVelocity.y = _frontY * 2f * _speed;
                    }
                }
                else if (absY < 1f) // 높이 차 1 미만의 경사 지형에 대한 계산
                {
                    var dir = _raycastHit.point - _states.mTransform.position;
                    var angle = Vector3.Angle(dir, _states.mTransform.forward);
                    if (angle > _states.slopeLimitAngle)
                    {
                        _states.isSlipping = true;
                    }
                }
            }

            // Jump Velocity
            if (_states.isJumping)
            {
                //deprecated::Don't use Rigidbody
                /*if (!_isJumpPeek && _states.thisRigidbody.velocity.y < _states.jumpForce)
                    _targetVelocity.y = _states.jumpForce; //deprecated::Don't use Rigidbody
                else
                    _isJumpPeek = true;*/
            }

            HandleAnimations();
            
            Debug.DrawRay((_states.mTransform.position + Vector3.up * 0.2f), _targetVelocity, Color.green, 0.01f, false);
            //_states.thisRigidbody.velocity = Vector3.Lerp(_currentVelocity, _targetVelocity, _states.fixedDelta * _states.adaptSpeed); //deprecated::Don't use Rigidbody

            return false;
        }
        
        private void HandleRotation()
        {
            float moveOverride = _states.moveAmount;
            float forward = _states.vertical;
            float sideways = _states.horizontal;

            if (_states.lockOn)
            {
                _targetDir = _states.targetEnemy.position - _states.mTransform.position;
                moveOverride = 1;
                
                _targetDir.Normalize();
                _targetDir.y = 0;
                if (_targetDir == Vector3.zero)
                    _targetDir = _states.transform.forward;
        
                _targetRotation = Quaternion.LookRotation(_targetDir);
            }
            else
            {
                
                _targetDir = _states.mainCamera.forward * forward;
                _targetDir += _states.mainCamera.right * sideways;
                if (forward < 0) _targetDir *= -1;
            
                _targetDir.Normalize();
                _targetDir.y = 0;
                if (_targetDir == Vector3.zero)
                    _targetDir = _states.transform.forward;
            
                _targetRotation = Quaternion.LookRotation(_targetDir);
            }
            
            
            _states.mTransform.rotation = Quaternion.Slerp(
                                        _states.mTransform.rotation, _targetRotation,
                                        _states.fixedDelta * moveOverride * _states.rotationSpeed);
        }

        private void HandleAnimations()
        {
            if (_states.isGrounded)
            {
                if (_states.lockOn)
                {
                    float v = Mathf.Abs(_states.vertical);
                    float f = 0;
                    if (v > 0 && v < 0.5f) 
                        f = 0.5f;
                    else if (v > 0.5f) 
                        f = 1;

                    if (_states.vertical < 0) f = -f;
                    
                    if (!_states.isRunning)
                    {
                        if (f > 0.5f) f = 0.5f;
                        else if (f < -0.5f) f = -0.5f;
                    }
                
                    _states.anim.SetFloat(_animHashForward, f, 0.2f, _states.fixedDelta);
                    
                    float h = Mathf.Abs(_states.horizontal);
                    float s = 0;
                    if (h > 0 && h < 0.5f) 
                        s = 0.5f;
                    else if (h > 0.5f) 
                        s = 1;

                    if (_states.horizontal < 0) s = -1;
                    
                    if (!_states.isRunning)
                    {
                        if (s > 0.5f) s = 0.5f;
                        else if (s < -0.5f) s = -0.5f;
                    }
                
                    _states.anim.SetFloat(_animHashSideways, s, 0.2f, _states.fixedDelta);
                }
                else
                {
                    float m = _states.moveAmount;
                    float f = 0;
                    if (m > 0 && m < 0.5f) 
                        f = 0.5f;
                    else if (m > 0.5f) 
                        f= 1;
                    if (_states.vertical < 0) f *= -1;
                    
                    if (!_states.isRunning)
                    {
                        if (f > 0.5f) f = 0.5f;
                        else if (f < -0.5f) f = -0.5f;
                    }
                    _states.anim.SetFloat(_animHashForward, f, 0.2f, _states.fixedDelta);
                    _states.anim.SetFloat(_animHashSideways, 0, 0.2f, _states.fixedDelta);
                }
            }
            
        }
    }
}