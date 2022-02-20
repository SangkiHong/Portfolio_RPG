using Unity.Mathematics;
using UnityEngine;

namespace Sangki
{
    public class MovePlayerCharacter : StateAction
    {
        PlayerStateManager _states;
        
        private RaycastHit _raycastHit;
        private Vector3 _currentVelocity, _targetVelocity, _origin, _targetDir;
        private Quaternion _targetRotation;
        
        private bool isFixedRotating;
        private readonly int _animHash_Forward = Animator.StringToHash("Forward");
        private readonly int _animHash_Sideways = Animator.StringToHash("Sideways");

        public MovePlayerCharacter(PlayerStateManager playerStateManager)
        {
            _states = playerStateManager;
        }

        public override bool Execute()
        {
            isFixedRotating = _states.isFixRotation;
            
            float frontY = 0;
            
            if (_states.lockOn)
            {
                _targetVelocity = _states.mTransform.forward * _states.vertical * _states.movementsSpeed;
                _targetVelocity += _states.mTransform.right * _states.horizontal * _states.movementsSpeed;
            }
            else
            {
                float m = _states.moveAmount;
                if (_states.vertical < 0)
                {
                    m *= -0.65f; // Slow Backward
                }
                _targetVelocity = _states.mTransform.forward * m * _states.movementsSpeed;
            }
            
            _origin = _states.mTransform.position + (_targetVelocity.normalized * _states.frontRayOffset);
            _origin.y += 0.5f;
            
            Debug.DrawRay(_origin, -Vector3.up, Color.red, 0.01f, false);
            if (Physics.Raycast(_origin, -Vector3.up, out _raycastHit, 1, _states.ignoreForGroundCheck))
            {
                float y = _raycastHit.point.y;
                frontY = y - _states.mTransform.position.y;
            }

            _currentVelocity = _states.rigidbody.velocity;

            if (_states.isGrounded)
            {
                float moveAmount = _states.moveAmount;
                
                if (moveAmount > 0.1f)
                {
                    _states.rigidbody.isKinematic = false;
                    _states.rigidbody.drag = 0;
                    if (Mathf.Abs(frontY) > 0.02f)
                    {
                        _targetVelocity.y = ((frontY > 0) ? frontY + 0.2f : frontY - 0.2f) * _states.movementsSpeed;
                    }
                
                    HandleRotation();
                }
                else
                {
                    float abs = Mathf.Abs(frontY);

                    if (abs > 0.02f)
                    {
                        _states.rigidbody.isKinematic = true;
                        _targetVelocity.y = 0;
                        _states.rigidbody.drag = 4;
                    }
                }
            }
            else
            {
                _states.rigidbody.isKinematic = false;
                _states.rigidbody.drag = 0;
                _targetVelocity.y = _currentVelocity.y;
            }
            
            HandleAnimations();
            
            Debug.DrawRay((_states.mTransform.position + Vector3.up * 0.2f), _targetVelocity, Color.green, 0.01f, false);
            _states.rigidbody.velocity = Vector3.Lerp(_currentVelocity, _targetVelocity, _states.delta * _states.adaptSpeed);
            
            return false;
        }
        
        private void HandleRotation()
        {
            float moveOverride = _states.moveAmount;
            float forward = _states.vertical;
            float sideways = _states.horizontal;

            if (!isFixedRotating)
            {
                if (_states.lockOn)
                {
                    _targetDir = _states.target.position - _states.mTransform.position;
                    moveOverride = 1;
                }
                else
                {
                
                    _targetDir = _states.mTransform.forward * forward;
                    _targetDir += _states.mTransform.right * sideways;
                }
            
                _targetDir.Normalize();
                _targetDir.y = 0;
                if (_targetDir == Vector3.zero)
                    _targetDir = _states.transform.forward;
            
                _targetRotation = Quaternion.LookRotation(_targetDir);
            }
            else
            {
                if (_states.lockOn)
                {
                    _targetDir = _states.target.position - _states.mTransform.position;
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
            }
            
            _states.mTransform.rotation = Quaternion.Slerp(
                                        _states.mTransform.rotation, _targetRotation,
                                        _states.delta * moveOverride * _states.rotationSpeed);
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
                
                    _states.anim.SetFloat(_animHash_Forward, f, 0.2f, _states.delta);
                    
                    float h = Mathf.Abs(_states.horizontal);
                    float s = 0;
                    if (h > 0 && h < 0.5f) 
                        s = 0.5f;
                    else if (h > 0.5f) 
                        s = 1;

                    if (_states.horizontal < 0) s = -1;
                
                    _states.anim.SetFloat(_animHash_Sideways, s, 0.2f, _states.delta);
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
                
                    _states.anim.SetFloat(_animHash_Forward, f, 0.2f, _states.delta);
                    _states.anim.SetFloat(_animHash_Sideways, 0, 0.2f, _states.delta);
                }
            }
            
        }
    }
}