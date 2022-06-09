using UnityEngine;
using SK.FSM;
using SK.Data;

namespace SK
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;

        [SerializeField] private bool cursorVisible;

        public bool CusorVisible => cursorVisible;

        [SerializeField] private GameObject[] dontDestroyObjects;

        [Header("Reference")]
        public DataManager DataManager;
        public InputManager InputManager;
        public GrassManager GrassManager;
        public ItemListManager ItemListManager;

        public UI.UIManager UIManager;
        public Player Player;
        
        private void Awake()
        {
            Instance = this;

            DontDestroyOnLoad(gameObject);
            for (int i = 0; i < dontDestroyObjects.Length; i++)            
                DontDestroyOnLoad(dontDestroyObjects[i]);

            // 초기화
            DataManager.Initialize();

            // 타겟 프레임 설정
            // 유니티에디터 Vsync를 해제해야 함
            //Application.targetFrameRate = 40;
        }

        public void ChangeScene(int sceneNum)
        {
            InputManager.SwitchInputMode(InputMode.GamePlay);
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneNum);
        }

        public void SetMainScene()
        {
            Cursor.visible = cursorVisible;
            if (!cursorVisible)
                Cursor.lockState = CursorLockMode.Locked;
        }

        public void SwitchMouseState(bool onVisible)
        {
            cursorVisible = onVisible;
            Cursor.visible = cursorVisible;

            if (cursorVisible)
                Cursor.lockState = CursorLockMode.Confined;
            else
                Cursor.lockState = CursorLockMode.Locked;
        }
    }
}