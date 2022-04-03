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
        
        [Header("value")]
        [SerializeField] private float targetingLimitDistance = 30;

        private Transform _playerTranform;
        private Enemy _enemy;
        private bool _isTargeting;

        public void Init(Transform cameraTarget)
        {
            _playerTranform = GameManager.Instance.player.transform;

            if (normalCamera)
            {
                normalCamera.m_Follow = cameraTarget;
                normalCamera.m_LookAt = cameraTarget;
            }
            if (lockOnCamera)
                lockOnCamera.m_Follow = cameraTarget;
        }

        public void OnAssignLookOverride(Transform lockTarget)
        {
            normalCamera.gameObject.SetActive(false);
            lockOnCamera.gameObject.SetActive(true);
            lockOnCamera.m_LookAt = lockTarget;
            _enemy = lockTarget.GetComponent<Enemy>();
            _isTargeting = true;
        }

        public void OnClearLookOverride()
        {
            normalCamera.gameObject.SetActive(true);
            lockOnCamera.gameObject.SetActive(false);
            _enemy = null;
            _isTargeting = false;
        }

        void LateUpdate()
        {
            if (_isTargeting && _enemy)
            {
                bool isTargetOnDist = Vector3.Distance(_playerTranform.position, _enemy.transform.position) < targetingLimitDistance;

                if (_enemy.isDead || !isTargetOnDist)
                {
                    OnClearLookOverride();
                    GameManager.Instance.player.isTargeting = false;
                    GameManager.Instance.player.targetEnemy = null;
                }
            }
        }
    }
}
