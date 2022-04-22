using UnityEngine;
using Cinemachine;

namespace SK
{
    public class CameraManager : MonoBehaviour
    {
        [Header("Reference")]
        public Transform mainCamera;
        public CinemachineFreeLook normalCamera;
        public CinemachineFreeLook lockOnCamera;

        [Header("Zoom")]
        public float cameraZoomSpeed = 0.02f;
        [SerializeField] private Vector2 zoomMinMax;

        [Header("Targeting")]
        [Range(0, 1)]
        [SerializeField] private float targetingPositionRatio = 0.5f;
        [SerializeField] private float targetingLimitDistance = 30;

        private Enemy _enemy;
        private Transform _transform;
        private Transform _playerTranform;
        private Transform _targetPoint;
        private float _targetDistance;
        private bool _isTargeting;

        public void Init(Transform cameraTarget)
        {
            _playerTranform = GameManager.Instance.player.transform;
            _transform = transform;

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
        }

        void LateUpdate()
        {
            if (_isTargeting && _enemy)
            {
                _targetDistance = Vector3.Distance(_playerTranform.position, _enemy.transform.position);

                // 타겟 상태 확인
                if (_enemy.isDead || _targetDistance > targetingLimitDistance)
                {
                    OnClearLookOverride();
                    GameManager.Instance.player.isTargeting = false;
                    GameManager.Instance.player.targetEnemy = null;

                    return;
                }
                Vector3 dir;
                if (_enemy.targetingPoint != null)
                    dir = _enemy.targetingPoint.position - _playerTranform.position;
                else
                    dir = _enemy.transform.position - _playerTranform.position;

                // 타겟팅 지점 업데이트
                _targetPoint.position = _playerTranform.position + dir.normalized * (_targetDistance * targetingPositionRatio);
            }
        }

        public void ZoomSetting(float scrollY)
        {
            if (scrollY < 0 && normalCamera.m_Lens.FieldOfView <= zoomMinMax[0])
            {
                normalCamera.m_Lens.FieldOfView = zoomMinMax[0];
            }
            else if (scrollY > 0 && normalCamera.m_Lens.FieldOfView >= zoomMinMax[1])
            {
                normalCamera.m_Lens.FieldOfView = zoomMinMax[1];
            }
            else
            {
                normalCamera.m_Lens.FieldOfView += scrollY;
            }
        }

        public void CameraRotateSwtich(bool switchOn)
        {
            if (switchOn)
            {
                if (_isTargeting) lockOnCamera.gameObject.SetActive(true);
                else normalCamera.gameObject.SetActive(true);
            }
            else
            {
                if (_isTargeting) lockOnCamera.gameObject.SetActive(false);
                else normalCamera.gameObject.SetActive(false);
            }
        }

        public void OnAssignLookOverride(Transform lockTarget)
        {
            _enemy = lockTarget.GetComponent<Enemy>();
            normalCamera.gameObject.SetActive(false);
            lockOnCamera.gameObject.SetActive(true);
            _targetPoint.gameObject.SetActive(true);
            _isTargeting = true;
        }

        public void OnClearLookOverride()
        {
            normalCamera.gameObject.SetActive(true);
            lockOnCamera.gameObject.SetActive(false);
            _targetPoint.gameObject.SetActive(false);
            _enemy = null;
            _isTargeting = false;
        }

        ///<param name="directionNum">0: Forward, 1: Left, 2: Right</param>
        public void LookAtCameraDirection(int directionNum)
        {
            Vector3 targetDir;
            if (directionNum == 0) targetDir = _transform.forward;
            else if (directionNum == 1) targetDir = -_transform.right;
            else targetDir = _transform.right;

            targetDir.y = 0;
            _playerTranform.rotation = Quaternion.LookRotation(targetDir);
        }
    }
}
