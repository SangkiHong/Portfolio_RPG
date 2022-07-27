using UnityEngine;
using Cinemachine;
using UnityEngine.Animations.Rigging;
using UnityEngine.Rendering;
using SK.Utilities;

namespace SK
{
    public class CameraManager : MonoBehaviour
    {
        [Header("Reference")]
        public Transform mainCameraTr;
        public CinemachineFreeLook normalCamera;
        public CinemachineFreeLook lockOnCamera;
        public CinemachineFreeLook interactingCamera;

        [Header("Zoom")]
        public float cameraZoomSpeed = 20f;
        [SerializeField] private float changeSpeed = 4;
        [SerializeField] private Vector2 zoomMinMax;
        [SerializeField] private float zoomEffectIntensity = 45;

        [Header("PostProcessing")]
        [SerializeField] private Volume postProcessingVolume;
        [SerializeField] private float HitEffectDuration = 0.5f;

        [Header("Targeting")]
        [Range(0, 1)]
        [SerializeField] private float targetingPositionRatio = 0.5f;
        [SerializeField] private float targetingLimitDistance = 30;
        [SerializeField] private Transform _targetingPointUI;
        internal float targetPointOffsetX;

        public Camera MainCamera { get; private set; }
        private Enemy _enemy;
        private Transform _transform, _playerTranform;
        private Transform _targetTransform, _targetPoint;

        private Rig _rig;
        private RigBuilder _rigBuilder;
        private MultiAimConstraint _aimConstraint;

        private float _targetFOV, _currentFOV;
        private float _cameraZoomElapsed, _effectElapsed;
        private float _targetDistance;
        private float _defaultAxisSpeedX, _defaultAxisSpeedY;
        private bool _isTargeting, _isOnZoomEffect;

        public void Init(Transform cameraTarget)
        {
            if (GameManager.Instance.Player)
                _playerTranform = GameManager.Instance.Player.transform;
            else
                _playerTranform = GameObject.FindGameObjectWithTag("Player").transform;

            _transform = transform;
            MainCamera = Camera.main;
            _rigBuilder = _playerTranform.GetComponentInChildren<RigBuilder>();
            _rig = _playerTranform.GetComponentInChildren<Rig>();
            _aimConstraint = _playerTranform.GetComponentInChildren<MultiAimConstraint>();

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

            // 포스트 프로세싱 초기화
            DamageEffect(false);

            targetingLimitDistance *= targetingLimitDistance;
            _targetFOV = normalCamera.m_Lens.FieldOfView;
            _currentFOV = _targetFOV;

            _cameraZoomElapsed = 1;

            _defaultAxisSpeedX = normalCamera.m_XAxis.m_MaxSpeed;
            _defaultAxisSpeedY = normalCamera.m_YAxis.m_MaxSpeed;
        }

        void LateUpdate()
        {
            #region Targeting
            if (_isTargeting && _enemy)
            {
                // 타겟과의 거리 업데이트하여 변수에 저장
                _targetDistance = MyMath.Instance.GetDistance(_playerTranform.position, _enemy.transform.position);

                // 타겟 상태 확인
                if (_enemy.isDead || _targetDistance > targetingLimitDistance)
                {
                    GameManager.Instance.Player.targeting.OnClearLookOverride();
                    return;
                }

                Vector3 dir;
                if (_enemy.targetingPoint != null)
                    dir = _enemy.targetingPoint.position - _playerTranform.position;
                else
                    dir = _enemy.transform.position - _playerTranform.position;

                // 타겟팅 지점 업데이트(타겟팅 거리 비율 + 오프셋 + 플레이어 위치값)
                _targetPoint.position = (targetingPositionRatio * dir) + (_playerTranform.right * targetPointOffsetX) + _playerTranform.position;
                
                // 타겟팅 UI 업데이트
                _targetingPointUI.position = MainCamera.WorldToScreenPoint(_targetTransform.position);
            }
            #endregion

            #region Camera
            // 카메라 줌 변경
            if (normalCamera && _cameraZoomElapsed >= 0 && _cameraZoomElapsed < 1)
            {
                _cameraZoomElapsed += Time.deltaTime * changeSpeed;
                if (_cameraZoomElapsed > 0.99f) _cameraZoomElapsed = 1;
                normalCamera.m_Lens.FieldOfView = Mathf.Lerp(_currentFOV, _targetFOV, _cameraZoomElapsed);

                // Clmap fov
                var fov = normalCamera.m_Lens.FieldOfView;
                if (fov < zoomMinMax.x) normalCamera.m_Lens.FieldOfView = zoomMinMax.x;
                if (fov > zoomMinMax.y) normalCamera.m_Lens.FieldOfView = zoomMinMax.y;
            }
            #endregion

            #region Damage Effect
            if (_effectElapsed > 0)
            {
                _effectElapsed -= Time.deltaTime;
                if (_effectElapsed <= 0) DamageEffect(false);
            }
            #endregion
        }

        #region Camera Control
        public void ZoomUpdate(float scrollY)
        {
            if (_isOnZoomEffect) return;

            _cameraZoomElapsed = 0;
            _currentFOV = normalCamera.m_Lens.FieldOfView;
            _targetFOV -= scrollY * cameraZoomSpeed;

            // Clmap fov
            if (_targetFOV < zoomMinMax.x) _targetFOV = zoomMinMax.x;
            if (_targetFOV > zoomMinMax.y) _targetFOV = zoomMinMax.y;
        }

        public void CameraRotatingHold(bool isHold)
        {
            if (!isHold)
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

        public void ChangeInteractingCamera(bool isOn, NPC targetNPC = null)
        {
            // 인터렉팅 카메라로 전환
            if (isOn)
            {
                // 타겟팅 중인 경우 해제
                if (_isTargeting)
                    GameManager.Instance.Player.targeting.OnClearLookOverride();

                // 인터렉팅 카메라 켜짐
                interactingCamera.Follow = targetNPC.transform;
                interactingCamera.LookAt = targetNPC.lookTarget;
                interactingCamera.gameObject.SetActive(true);
            }
            else
                interactingCamera.gameObject.SetActive(false);
        }

        public void ZoomEffect(bool onZooming)
        {
            _isOnZoomEffect = onZooming;
            _cameraZoomElapsed = 0;

            if (onZooming) _targetFOV = zoomEffectIntensity;
            else _targetFOV = _currentFOV;
        }
        #endregion

        #region Targeting
        // 타겟 할당 지정
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

        // 타겟 할당 해제
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

        /// <summary>
        /// // 카메라 방향에 따른 플레이어 회전
        /// </summary>
        /// <param name="direction">0: forward, 180: backward, 270: left, 90: right, 225: backward left, 135: backward right</param>
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

        public void DamageEffect(bool isOn)
        {
            postProcessingVolume.sharedProfile.components[2].active = isOn;
            postProcessingVolume.sharedProfile.components[8].active = isOn;
            postProcessingVolume.sharedProfile.components[15].active = isOn;
            _effectElapsed = HitEffectDuration;
        }
    }
}