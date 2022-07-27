using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

namespace SK
{
    public class MiniMapHandler : MonoBehaviour
    {
        [SerializeField] private Button button_ZoomIn;
        [SerializeField] private Button button_ZoomOut;
        [SerializeField] private float zoomMax;
        [SerializeField] private float zoomMin;
        [SerializeField] private float zoomIntensity = 5;
        
        private Camera _minimapCamera;
        private Transform _thisTransform;
        private Transform _playerTransform;

        private Vector3 _position;

        private void Awake()
            => Invoke("Initialize", 0.5f);

        private void Initialize()
        {
            // �ʱ�ȭ
            _thisTransform = transform;
            _playerTransform = GameManager.Instance.Player.transform;
            _minimapCamera = GetComponent<Camera>();

            // �ʱ� �� ����
            _position = _thisTransform.position;

            // ��ư �̺�Ʈ �Ҵ�
            button_ZoomIn.onClick.AddListener(ZoomIn);
            button_ZoomOut.onClick.AddListener(ZoomOut);

            // ����Ű �Ҵ�
            GameManager.Instance.InputManager.playerInput.actions["UI_ZoomIn"].performed += InputZoomIn;
            GameManager.Instance.InputManager.playerInput.actions["UI_ZoomOut"].performed += InputZoomOut;

            FollowPlayer();
            // ������Ʈ �߰�
            SceneManager.Instance.OnFixedUpdate += UpdateMiniMap;
        }

        // �̴ϸ� ������Ʈ
        private void UpdateMiniMap()
            => FollowPlayer();

        private void FollowPlayer()
        {
            // ��ġ ������Ʈ
            _position.x = _playerTransform.position.x;
            _position.z = _playerTransform.position.z;
            _thisTransform.position = _position;
        }

        // ��ǲ �̺�Ʈ �Լ�(�� ��)
        private void InputZoomIn(InputAction.CallbackContext obj) => ZoomIn();

        // ��ǲ �̺�Ʈ �Լ�(�� �ƿ�)
        private void InputZoomOut(InputAction.CallbackContext obj) => ZoomOut();

        public void ZoomIn()
        {
            float currentSize = _minimapCamera.orthographicSize;
            if (currentSize - zoomIntensity >= zoomMin)
                _minimapCamera.orthographicSize -= zoomIntensity;
        }

        public void ZoomOut()
        {
            float currentSize = _minimapCamera.orthographicSize;
            if (currentSize + zoomIntensity <= zoomMax)
                _minimapCamera.orthographicSize += zoomIntensity;
        }

        private void OnApplicationQuit()
        {
            GameManager.Instance.InputManager.playerInput.actions["UI_ZoomIn"].performed -= InputZoomIn;
            GameManager.Instance.InputManager.playerInput.actions["UI_ZoomOut"].performed -= InputZoomOut;
        }
    }
}
