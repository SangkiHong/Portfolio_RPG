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
            // 초기화
            _thisTransform = transform;
            _playerTransform = GameManager.Instance.Player.transform;
            _minimapCamera = GetComponent<Camera>();

            // 초기 값 저장
            _position = _thisTransform.position;

            // 버튼 이벤트 할당
            button_ZoomIn.onClick.AddListener(ZoomIn);
            button_ZoomOut.onClick.AddListener(ZoomOut);

            // 단축키 할당
            GameManager.Instance.InputManager.playerInput.actions["UI_ZoomIn"].performed += InputZoomIn;
            GameManager.Instance.InputManager.playerInput.actions["UI_ZoomOut"].performed += InputZoomOut;

            FollowPlayer();
            // 업데이트 추가
            SceneManager.Instance.OnFixedUpdate += UpdateMiniMap;
        }

        // 미니맵 업데이트
        private void UpdateMiniMap()
            => FollowPlayer();

        private void FollowPlayer()
        {
            // 위치 업데이트
            _position.x = _playerTransform.position.x;
            _position.z = _playerTransform.position.z;
            _thisTransform.position = _position;
        }

        // 인풋 이벤트 함수(줌 인)
        private void InputZoomIn(InputAction.CallbackContext obj) => ZoomIn();

        // 인풋 이벤트 함수(줌 아웃)
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
