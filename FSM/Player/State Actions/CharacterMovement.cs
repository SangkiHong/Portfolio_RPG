using UnityEngine;

namespace SK.FSM
{
    public class CharacterMovement : StateAction
    {
        private readonly Player _player;
        
        private RaycastHit _raycastHit;
        private Vector3 _tranformPos;
        private Vector3 _targetVelocity, _origin, _targetDir;
        private Quaternion _targetRotation;

        private readonly int _animHashForward = Animator.StringToHash("Forward");
        private readonly int _animHashSideways = Animator.StringToHash("Sideways");

        private float _frontY, _speed, _moveAmount, _elapsed, _gravityAccel;

        public CharacterMovement(Player player) => _player = player;

        public override void Execute()
        {
            if (_player == null) return;

            _moveAmount = _player.moveAmount;
            _tranformPos = _player.mTransform.position;

            HandleMovement();
            HandleRotation();
            HandleGravity();
            HandleJump();
            HandleMovementAnim();

            _player.characterController.Move(_targetVelocity * _player.fixedDeltaTime);
        }

        private void HandleMovement()
        {
            // 피격 상태이거나 움직임이 없을 경우
            if (_player.isDamaged || _moveAmount < 0.1f)
                _targetVelocity = Vector3.zero;
            else
            {
                _speed = _player.isRunning ? _player.runSpeed : _player.movementsSpeed; // 달릴 시 속도 변경

                // 타겟팅 모드가 아닌 경우
                if (!_player.targeting.isTargeting)
                {
                    // 비전투 모드인 경우
                    if (!_player.onCombatMode)
                    {
                        float overrideMove = _moveAmount;

                        // 뒷걸음 시 뒤로 이동 속도 감속
                        if (_player.vertical < 0)
                            overrideMove *= -_player.slowDownBackward;

                        _targetVelocity = _speed * overrideMove * _player.mTransform.forward;
                    }
                    // 전투 모드인 경우
                    else
                    {
                        // 카메라의 전방을 기준으로 전후방 이동
                        _targetVelocity = _player.vertical * _speed * _player.cameraManager.transform.forward;
                        // 플레이어의 전방을 기준으로 좌우 이동(30% 감속)
                        _targetVelocity += 0.7f * _player.horizontal * _speed * _player.mTransform.right;
                        _targetVelocity.y = 0;
                    }
                }
                // 타겟팅 모드인 경우
                else
                {
                    // 타겟팅 시 이동 속도 감속
                    _speed *= _player.slowDownTargeting;

                    _targetVelocity = _player.vertical * _speed * _player.mTransform.forward;
                    _targetVelocity += _player.horizontal * _speed * _player.mTransform.right;
                }

                _origin = _tranformPos + (_targetVelocity * _player.frontRayOffset);
                _origin.y += _player.frontRayOffsetHeight;

                //Debug.DrawRay(_origin, -Vector3.up * (_player.frontRayOffsetHeight + 1), Color.red);
                if (Physics.Raycast(_origin, -Vector3.up, out _raycastHit, _player.frontRayOffsetHeight + 1, _player.groundLayerMask))
                    _frontY = _raycastHit.point.y - _tranformPos.y;

                // 지면에 닿아 있는 경우 비탈면 경사에 따라 미끄러짐
                if (_player.isGrounded)
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
                        var dir = _raycastHit.point - _tranformPos;
                        var angle = Vector3.Angle(dir, _player.mTransform.forward);
                        if (angle > _player.slopeLimitAngle)
                        {
                            _player.isSlipping = true;
                        }
                    }
                }
            }
            
            Debug.DrawRay((_tranformPos + Vector3.up * 0.2f), _targetVelocity, Color.green, 0.01f, false);
        }
        
        private void HandleRotation()
        {
            float moveOverride = _player.moveAmount;
            float forward = _player.vertical;
            float sideways = _player.horizontal;

            // 타겟팅 모드인 경우
            if (_player.targeting.isTargeting)
            {
                _targetDir = _player.targeting.target.position - _tranformPos;
                moveOverride = 1;
                
                _targetDir.Normalize();
                _targetDir.y = 0;
                if (_targetDir == Vector3.zero)
                    _targetDir = _player.transform.forward;
        
                _targetRotation = Quaternion.LookRotation(_targetDir);
            }
            else
            {
                // 목표 방향 초기화
                _targetDir = Vector3.zero;

                // 비전투 모드인 경우
                if (!_player.onCombatMode)
                {
                    _targetDir += Mathf.Abs(forward) * _player.cameraManager.transform.forward;
                    // 후방 이동인 경우 좌우 반전 이동
                    _targetDir += (forward >= 0 ? sideways : -sideways) * _player.cameraManager.transform.right;
                }
                // 전투 모드인 경우
                else
                {
                    _targetDir += Mathf.Clamp01(Mathf.Abs(forward) + Mathf.Abs(sideways)) * _player.cameraManager.transform.forward;
                }
            
                _targetDir.Normalize();
                _targetDir.y = 0;
                if (_targetDir == Vector3.zero)
                    _targetDir = _player.transform.forward;
            
                _targetRotation = Quaternion.LookRotation(_targetDir);
            }
                        
            _player.mTransform.rotation = Quaternion.Slerp(_player.mTransform.rotation, _targetRotation,
                                            _player.fixedDeltaTime * moveOverride * _player.rotationSpeed);
        }

        private void HandleJump()
        {
            if (_player.isJumping)
            {
                var jumpPoint = _elapsed / _player.jumpTime;

                // 점프 중인 경우 가장 높이 올라갔을 때부터 착지 여부 확인
                if (_player.isGrounded && jumpPoint > 0.5f)
                {
                    // 초기화
                    _elapsed = 0;
                    _player.isJumping = false;
                    _player.anim.SetTrigger(Strings.AnimPara_Land);
                    return;
                }

                if (jumpPoint < 1)
                    _elapsed += _player.fixedDeltaTime;
                else
                    _elapsed = _player.jumpTime;

                _targetVelocity.y += Utilities.MyMath.Instance.GetSine((uint)Mathf.RoundToInt(jumpPoint * 270)) * _player.jumpHeight;
            }
        }

        private void HandleMovementAnim()
        {
            if (_player.isGrounded)
            {
                // 타겟팅 중이거나 전투 모드 상태의 애니메이션
                if (_player.targeting.isTargeting || _player.onCombatMode)
                {
                    float vert = Mathf.Abs(_player.vertical);
                    float forward = 0;
                    if (vert > 0 && vert < 0.5f) 
                        forward = 0.5f;
                    else if (vert > 0.5f) 
                        forward = 1;

                    if (_player.vertical < 0) forward = -forward;
                    
                    if (!_player.isRunning)
                    {
                        if (forward > 0.5f) forward = 0.5f;
                        else if (forward < -0.5f) forward = -0.5f;
                    }
                    
                    float hori = Mathf.Abs(_player.horizontal);
                    float sideway = 0;
                    if (hori > 0 && hori < 0.5f) 
                        sideway = 0.5f;
                    else if (hori > 0.5f) 
                        sideway = 1;

                    if (_player.horizontal < 0) sideway = -1;
                    
                    if (!_player.isRunning)
                    {
                        if (sideway > 0.5f) sideway = 0.5f;
                        else if (sideway < -0.5f) sideway = -0.5f;
                    }

                    _player.anim.SetFloat(_animHashForward, forward, 0.2f, _player.fixedDeltaTime);
                    _player.anim.SetFloat(_animHashSideways, sideway, 0.2f, _player.fixedDeltaTime);
                }
                // 일반 이동의 애니메이션
                else
                {
                    // Move Blend의 전후방 애니메이션 파라미터를 위한 변수
                    float forward = 0; 

                    // 이동량이 있는 경우
                    if (_moveAmount != 0)
                    {
                        // 기본 이동 중인 경우
                        if (_moveAmount > 0 && _moveAmount <= 0.5f)
                            forward = 0.5f;
                        // 달리기로 이동 중인 경우
                        else if (_moveAmount > 0.5f)
                            forward = 1;

                        // 뒤로 이동 중인 경우 forward 값을 반전
                        if (_player.vertical < 0) forward *= -1;

                        if (!_player.isRunning)
                        {
                            if (forward > 0.5f) forward = 0.5f;
                            else if (forward < -0.5f) forward = -0.5f;
                        }
                    }

                    if (_player.anim.GetFloat(_animHashForward) != forward)
                        _player.anim.SetFloat(_animHashForward, forward, 0.2f, _player.fixedDeltaTime);
                    if (_player.anim.GetFloat(_animHashSideways) != 0)
                        _player.anim.SetFloat(_animHashSideways, 0, 0.2f, _player.fixedDeltaTime);
                }
            }
        }

        private void HandleGravity()
        {
            if (_player.isGrounded)
            {
                _targetVelocity.y = _player.groundedGravity;
                if (_gravityAccel != 0) _gravityAccel = 0;
            }
            else
            {
                if (_gravityAccel >= _player.gravity)
                    _gravityAccel -= _player.fixedDeltaTime * _player.gravityAcclation;

                _targetVelocity.y = _gravityAccel;
            }
        }
    }
}