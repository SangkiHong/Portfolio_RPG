using UnityEngine;
using SK.FSM;
using SK.Data;
using System.Collections;
using System.Xml;

namespace SK
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] private bool cursorVisible;

        public bool CusorVisible => cursorVisible;

        [SerializeField] private GameObject[] dontDestroyObjects;

        [Header("Reference")]
        public DataManager DataManager;
        public InputManager InputManager;
        public ItemListManager ItemListManager;

        public Player Player;

        private bool _isPlayerDead;


        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(this);
            }
            else
                Destroy(gameObject);

            DontDestroyOnLoad(gameObject);
            for (int i = 0; i < dontDestroyObjects.Length; i++)            
                DontDestroyOnLoad(dontDestroyObjects[i]);

            // 초기화
            DataManager.Initialize();
            _isPlayerDead = false; 
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.Confined;

            // 타겟 프레임 설정
            // 유니티에디터 Vsync를 해제해야 함
            Application.targetFrameRate = 60;
        }

        public void SetMainScene()
        {
            Cursor.visible = cursorVisible;
            if (!cursorVisible)
                Cursor.lockState = CursorLockMode.Locked;
        }

        public void SwitchMouseState(bool onVisible)
        {
            if (_isPlayerDead) return;

            cursorVisible = onVisible;
            Cursor.visible = cursorVisible;

            // 타겟팅 중인 경우 마우스 락 모드 해제
            if (Player.targeting.isTargeting)
            { 
                Cursor.lockState = CursorLockMode.Confined;
                return;
            }

            if (cursorVisible)
                Cursor.lockState = CursorLockMode.Confined;
            else
                Cursor.lockState = CursorLockMode.Locked;
        }

        public void MouseVisibleAndFixCamera(bool isOn)
        {
            SwitchMouseState(isOn);
            Player.cameraManager.CameraRotatingHold(isOn);
        }

        public void PlayerDead()
        {
            // 마우스 UI 모드 전환 및 모든 버튼 홀딩
            InputManager.SwitchInputMode(InputMode.Disable);
            _isPlayerDead = true;
        }

        public void Respawn(bool respawnImmediately = false)
        {
            // 즉시 화면이 어두워짐
            UI.UIManager.Instance.SetScreenBlack(true);

            Player.gameObject.SetActive(false);
            System.GC.Collect();

            // 제자리에서 즉시 리스폰이 아닌 경우 가까운 마을로 리스폰
            if (!respawnImmediately)
            {
                // 마을의 지정된 위치로 플레이어 위치시킴
                Player.mTransform.position = SceneManager.Instance.locationManager.respawnPoint;
                // 마을로 플레이어 위치 업데이트
                SceneManager.Instance.locationManager.UpdatePlayerLocation(Location.Location.HYDREA_Village);
                // 마을 테마곡 재생
                AudioManager.Instance.PlayBackGroundMusic(Strings.Audio_BGM_Village, 2);
            }

            Player.gameObject.SetActive(true);

            // 인풋 사용 재개
            InputManager.SwitchInputMode(InputMode.GamePlay);
            _isPlayerDead = false;
        }
    }
}