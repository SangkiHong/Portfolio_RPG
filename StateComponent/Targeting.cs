using UnityEngine;
using UnityEngine.InputSystem;
using SK.Utilities;

namespace SK.Behavior
{
    public class Targeting
    {
        internal Transform target;
        internal bool isTargeting;

        private readonly Transform _transform;
        private readonly Transform _cameraTarget;
        private readonly CameraManager _cameraManager;
        private readonly Collider[] _targetColliders;
        private readonly LayerMask _targetLayerMask;

        private InputActionMap _actionMapTargeting;
        private InputAction _inputAction_TargetChangeLeft;
        private InputAction _inputAction_TargetChangeRight;

        private readonly float _targetSearchRange;
        private float _maxTargetingDistance;
        private float _targetDistance;

        // 클래스 생성자
        public Targeting(Transform transform, CameraManager cameraManager, Transform cameraTarget, float targetSearchRange, LayerMask targetLayerMask)
        {
            // 변수 초기화
            _transform = transform;
            _cameraTarget = cameraTarget;
            _cameraManager = cameraManager;
            _targetColliders = new Collider[20];
            _targetLayerMask = targetLayerMask;
            _targetSearchRange = targetSearchRange;
            _maxTargetingDistance = _targetSearchRange * _targetSearchRange;
            // 인풋 할당
            _inputAction_TargetChangeLeft = GameManager.Instance.InputManager.playerInput.actions["TargetChange_Left"];
            _inputAction_TargetChangeRight = GameManager.Instance.InputManager.playerInput.actions["TargetChange_Right"];
            _inputAction_TargetChangeLeft.started += x => { ChangeTarget(true); };
            _inputAction_TargetChangeRight.started += x => { ChangeTarget(false); };
            _actionMapTargeting = GameManager.Instance.InputManager.playerInput.actions.actionMaps[5];
            _actionMapTargeting.Disable();
        }

        // 타겟을 할당하여 타겟팅을 시작하는 함수
        public void OnAssignLookOverride(Transform targetTransform = null)
        {
            if (targetTransform == null)
                target = FindLockableTarget();
            else
                target = targetTransform;

            if (target != null)
            {
                isTargeting = true;
                _cameraManager.OnAssignLookOverride(target);
                Cursor.lockState = CursorLockMode.Confined;
                // 인풋 액션 맵 사용
                _actionMapTargeting.Enable();
                // 타겟 거리 업데이트
                SceneManager.Instance.OnFixedUpdate += UpdateTargetDistance;
            }
        }

        // 타겟을 할당 해제하여 타겟팅을 중단하는 함수
        public void OnClearLookOverride()
        {
            isTargeting = false;
            target = null;
            _cameraManager.OnClearLookOverride();
            // 인풋 액션 맵 중지
            _actionMapTargeting.Disable();
            // 타겟 거리 업데이트 해제
            SceneManager.Instance.OnFixedUpdate -= UpdateTargetDistance;
        }

        // 타겟팅 포인트를 재설정
        public void ResetTargetingPoint(float fixedDelta, float rotationSpeed)
        {
            if (isTargeting && _cameraTarget.localRotation != Quaternion.identity)
                _cameraTarget.localRotation = 
                    Quaternion.Slerp(_cameraTarget.localRotation, Quaternion.identity, fixedDelta * rotationSpeed);
        }

        public void LookAtTarget()
        {
            if (isTargeting && target)            
                _cameraTarget.LookAt(target);            
        }

        // 플레이어의 정면에서 가장 가까운 타겟을 탐색하여 반환
        private Transform FindLockableTarget()
        {
            if (Physics.OverlapSphereNonAlloc(_transform.position, _targetSearchRange, _targetColliders, _targetLayerMask) > 0)
            {
                float minDegree = 360;
                int selectedIndex = 0;

                // 카메라 각도 비교 후 가장 작은 각의 target을 return
                for (int i = 0; i < _targetColliders.Length; i++)
                {
                    if (_targetColliders[i] != null)
                    {
                        var dir = (_targetColliders[i].transform.position - _transform.position).normalized;
                        var degree = Vector3.Angle(_cameraManager.mainCameraTr.forward, dir);

                        if (minDegree > degree)
                        {
                            minDegree = degree;
                            selectedIndex = i;
                        }
                    }
                    else
                        break;
                }
                return target = _targetColliders[selectedIndex].transform;
            }
            return target = null;
        }

        // 마우스 이동 방향에 따른 타겟 변경
        private void ChangeTarget(bool toLeft)
        {
            float count = Physics.OverlapSphereNonAlloc(_transform.position, _targetSearchRange, _targetColliders, _targetLayerMask);

            if (count < 2) return;

            float currentTargetPosX = _cameraManager.MainCamera.WorldToScreenPoint(target.position).x;
            float targetPosX;
            float closePosX = toLeft ? 0 : Screen.width;
            int selectedIndex = -1;

            // 카메라 각도 비교 후 가장 작은 각의 target을 return
            for (int i = 0; i < _targetColliders.Length; i++)
            {
                if (_targetColliders[i] != null && _targetColliders[i].transform != target)
                {
                    // 해당 콜라이더의 스크린 위치의 X 값을 변수에 할당
                    targetPosX = _cameraManager.MainCamera.WorldToScreenPoint(_targetColliders[i].transform.position).x;

                    // 왼쪽의 적으로 변경하는 경우
                    if (toLeft)
                    {
                        if (closePosX < targetPosX && targetPosX < currentTargetPosX)
                        {
                            closePosX = targetPosX;
                            selectedIndex = i;
                        }
                    }
                    // 오른쪽의 적으로 변경하는 경우
                    else
                    {
                        if (currentTargetPosX < targetPosX && targetPosX < closePosX)
                        {
                            closePosX = targetPosX;
                            selectedIndex = i;
                        }
                    }
                }
            }

            // 인덱스 값이 -1보다 큰 경우 할당된 것을 의미하며 해당 타겟으로 변경
            if (selectedIndex > -1)
                OnAssignLookOverride(_targetColliders[selectedIndex].transform);
        }

        // 타겟팅 중 마우스 움직임에 따라 타겟 변경
        /*private void UpdateMe()
        {
            float count = Physics.OverlapSphereNonAlloc(_transform.position, _targetSearchRange, _targetColliders, _targetLayerMask);

            if (count <= 1) return;

            // 타겟 변경 가능 시간 업데이트
            if (_intervalElapsed > 0)
            {
                _intervalElapsed -= Time.fixedDeltaTime;

                if (_intervalElapsed <= 0)
                    _prevMousePosX = Input.mousePosition.x;
                return;
            }

            float currentPosX = Input.mousePosition.x;
            // 오른쪽으로 마우스 이동
            if (_prevMousePosX < currentPosX && currentPosX - _prevMousePosX > _targetingMouseSensitive)
            {
                ChangeTarget(false);
                _intervalElapsed = CHANGE_TARGET_INTERVAL;
                _cameraManager.targetPointOffsetX = 0;
                return;
            }
            // 왼쪽으로 마우스 이동
            else if (_prevMousePosX > currentPosX && _prevMousePosX - currentPosX > _targetingMouseSensitive)
            {
                ChangeTarget(true);
                _intervalElapsed = CHANGE_TARGET_INTERVAL;
                _cameraManager.targetPointOffsetX = 0;
            }

            // 마우스 움직임에 따라 타겟팅 위치 변경 
            float offsetX = _cameraManager.targetPointOffsetX;
            if (_prevMousePosX < currentPosX && offsetX < 3f)
            {
                offsetX += 0.1f;
                _cameraManager.targetPointOffsetX = offsetX;
            }
            // 왼쪽으로 마우스 이동
            else if (_prevMousePosX > currentPosX && offsetX > -3f)
            {
                offsetX -= 0.1f;
                _cameraManager.targetPointOffsetX = offsetX;
            }

            _prevMousePosX = currentPosX;
        }*/

        // 타겟 거리 업데이트하며 멀어지면 타겟팅 해제
        private void UpdateTargetDistance()
        {
            if (target)
            {
                // 타겟 거리 구하기
                _targetDistance = MyMath.Instance.GetDistance(_transform.position, target.position);
                // 타겟이 최대 타겟 유지 거리 보다 멀어진 경우 타겟팅 해제
                if (_targetDistance > _maxTargetingDistance)
                    OnClearLookOverride();
            }
        }

        // 소멸자
        ~Targeting()
        {
            // 인풋 이벤트 함수 해제
            _inputAction_TargetChangeLeft.started -= x => { ChangeTarget(true); };
            _inputAction_TargetChangeRight.started -= x => { ChangeTarget(false); };
        }
    }
}