using UnityEngine;

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
        private readonly float _targetSearchRange = 20;

        public Targeting(Transform transform, CameraManager cameraManager, Transform cameraTarget, float targetSearchRange, LayerMask targetLayerMask)
        {
            // 변수 초기화
            _transform = transform;
            _cameraTarget = cameraTarget;
            _cameraManager = cameraManager;
            _targetColliders = new Collider[5];
            _targetLayerMask = targetLayerMask;
            _targetSearchRange = targetSearchRange;
            if (_cameraManager) _cameraManager.Init(cameraTarget);
        }

        // 타겟을 할당하여 타겟팅을 시작하는 함수
        public void OnAssignLookOverride(Transform lockTarget)
        {
            if (lockTarget == null) return;
            target = lockTarget;

            isTargeting = true;
            _cameraManager.OnAssignLookOverride(lockTarget);
        }

        // 타겟을 할당 해제하여 타겟팅을 중단하는 함수
        public void OnClearLookOverride()
        {
            isTargeting = false;
            target = null;
            _cameraManager.OnClearLookOverride();
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

        public Transform FindLockableTarget()
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
    }
}