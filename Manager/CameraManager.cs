using UnityEngine;
using Cinemachine;
using UnityEngine.Animations.Rigging;

namespace SK
{
    public class CameraManager : MonoBehaviour
    {
        [Header("Reference")]
        public Transform mainCameraTr;
        public CinemachineFreeLook normalCamera;
        public CinemachineFreeLook lockOnCamera;

        [Header("Zoom")]
        public float cameraZoomSpeed = 20f;
        [SerializeField] private float changeSpeed = 4;
        [SerializeField] private Vector2 zoomMinMax;

        [Header("Targeting")]
        [Range(0, 1)]
        [SerializeField] private float targetingPositionRatio = 0.5f;
        [SerializeField] private float targetingLimitDistance = 30;

        private Camera mainCamera;
        private Enemy _enemy;
        private Transform _transform, _playerTranform;
        private Transform _targetTransform, _targetPoint;
        private Transform _targetingPointUI;

        private Rig _rig;
        private RigBuilder _rigBuilder;
        private MultiAimConstraint _aimConstraint;

        private float _targetFOV, _currentFOV, _elapsed;
        private float _targetDistance;
        private float _defaultAxisSpeedX, _defaultAxisSpeedY;
        private bool _isTargeting;

        public void Init(Transform cameraTarget)
        {
            _playerTranform = GameManager.Instance.Player.transform;
            _transform = transform;
            mainCamera = Camera.main;
            _rigBuilder = _playerTranform.GetComponentInChildren<RigBuilder>();
            _rig = _playerTranform.GetComponentInChildren<Rig>();
            _aimConstraint = _playerTranform.GetComponentInChildren<MultiAimConstraint>();
            _targetingPointUI = GameObject.FindGameObjectWithTag("TargetingPoint").transform;

            if (normalCamera)
            {
                normalCamera.m_Follow = cameraTarget;
                normalCamera.m_LookAt = cameraTarget;
            }
            if (lockOnCamera)
                lockOnCamera.m_Follow = cameraTarget;

            GameObject targetobj = new GameObject("Targeting Point");
            _targetPoint = targetobj.transform;
            lockOnCamera.m_LookAt = _targetPoint;
            targetobj.SetActive(false);

            _targetFOV = normalCamera.m_Lens.FieldOfView;
            _currentFOV = _targetFOV;

            _defaultAxisSpeedX = normalCamera.m_XAxis.m_MaxSpeed;
            _defaultAxisSpeedY = normalCamera.m_YAxis.m_MaxSpeed;
        }

        void LateUpdate()
        {
            #region Targeting
            if (_isTargeting && _enemy)
            {
                _targetDistance = Vector3.Distance(_playerTranform.position, _enemy.transform.position);

                // 타겟 상태 확인
                if (_enemy.isDead || _targetDistance > targetingLimitDistance)
                {
                    OnClearLookOverride();
                    GameManager.Instance.Player.isTargeting = false;
                    GameManager.Instance.Player.targetEnemy = null;

                    return;
                }
                Vector3 dir;
                if (_enemy.targetingPoint != null)
                    dir = _enemy.targetingPoint.position - _playerTranform.position;
                else
                    dir = _enemy.transform.position - _playerTranform.position;

                // 타겟팅 지점 업데이트
                _targetPoint.position = _playerTranform.position + dir.normalized * (_targetDistance * targetingPositionRatio);
                
                // 타겟팅 UI 업데이트
                _targetingPointUI.position = mainCamera.WorldToScreenPoint(_targetTransform.position);
            }
            #endregion

            #region Camera
            // 카메라 줌 변경
            if (_elapsed <= 1 && normalCamera.m_Lens.FieldOfView >= zoomMinMax.x && normalCamera.m_Lens.FieldOfView <= zoomMinMax.y)
            {
                _elapsed += Time.deltaTime * changeSpeed;
                if (_elapsed > 0.99f) _elapsed = 1;
                normalCamera.m_Lens.FieldOfView = Mathf.Lerp(_currentFOV, _targetFOV, _elapsed);

                // Clmap fov
                var fov = normalCamera.m_Lens.FieldOfView;
                if (fov < zoomMinMax.x) normalCamera.m_Lens.FieldOfView = zoomMinMax.x;
                if (fov > zoomMinMax.y) normalCamera.m_Lens.FieldOfView = zoomMinMax.y;
            }
            #endregion
        }

        #region Camera Control
        public void ZoomUpdate(float scrollY)
        {
            _elapsed = 0;
            _currentFOV = normalCamera.m_Lens.FieldOfView;
            _targetFOV -= scrollY * cameraZoomSpeed;

            // Clmap fov
            if (_targetFOV < zoomMinMax.x) _targetFOV = zoomMinMax.x;
            if (_targetFOV > zoomMinMax.y) _targetFOV = zoomMinMax.y;
        }

        public void CameraRotateSwtich(bool switchOn)
        {
            if (switchOn)
            {
                normalCamera.m_XAxis.m_MaxSpeed = _defaultAxisSpeedX;
                normalCamera.m_YAxis.m_MaxSpeed = _defaultAxisSpeedY;
            }
            else
            {
                normalCamera.m_XAxis.m_MaxSpeed = 0;
                normalCamera.m_YAxis.m_MaxSpeed = 0;
            }
        }
        #endregion

        #region Targeting
        public void OnAssignLookOverride(Transform lockTarget)
        {
            _enemy = lockTarget.GetComponent<Enemy>();
            _targetTransform = _enemy.targetingPoint ? _enemy.targetingPoint : lockTarget;

            // Animation Rigging Target 교체
            var data = _aimConstraint.data.sourceObjects;
            if (data.GetTransform(0) != _targetTransform)
            {
                data.SetTransform(0, _targetTransform);
                _aimConstraint.data.sourceObjects = data;
                _rigBuilder.Build();
            }
            // Animation Rigging 활성화
            _rig.weight = 1;

            normalCamera.gameObject.SetActive(false);
            lockOnCamera.gameObject.SetActive(true);
            _targetPoint.gameObject.SetActive(true);
            _targetingPointUI.gameObject.SetActive(true);
            _isTargeting = true;
        }

        public void OnClearLookOverride()
        {
            _isTargeting = false;

            // Animation Rigging 비활성화
            _rig.weight = 0;

            normalCamera.gameObject.SetActive(true);
            lockOnCamera.gameObject.SetActive(false);
            _targetPoint.gameObject.SetActive(false);
            _targetingPointUI.gameObject.SetActive(false);
            _targetTransform = null;
            _enemy = null;
        }

        // 0: forward, 180: backward, 270: left, 90: right,
        // 225: backward left, 135: backward right
        public void LookAtCameraDirection(int direction)
        {
            Vector3 targetDir;

            if (direction == 225) targetDir = _transform.forward + _transform.right; // backward left
            else if (direction == 135) targetDir = _transform.forward + -_transform.right; // backward right
            else targetDir = _transform.forward;

            targetDir.y = 0;
            _playerTranform.rotation = Quaternion.LookRotation(targetDir);
        }
        #endregion
    }
}
