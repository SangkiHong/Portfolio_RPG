using UnityEngine;
using UnityEngine.SceneManagement;
using SK.FSM;


namespace SK
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;

        [SerializeField] private bool cursorVisible;

        public bool CusorVisible => cursorVisible;

        [SerializeField] private GameObject[] dontDestroyObjects;

        [Header("Reference")]
        public Data.DataManager DataManager;
        public InputManager InputManager;
        public PlayerStateManager Player;
        
        private void Awake()
        {
            Instance = this;

            DontDestroyOnLoad(gameObject);
            for (int i = 0; i < dontDestroyObjects.Length; i++)            
                DontDestroyOnLoad(dontDestroyObjects[i]);

            // 타겟 프레임 설정
            // 유니티에디터 Vsync를 해제해야 함
            //Application.targetFrameRate = 40;
        }

        public void ChangeScene(int sceneNum)
        {
            SceneManager.LoadScene(sceneNum);
        }

        public void SetMainScene()
        {
            Cursor.visible = cursorVisible;
            if (!cursorVisible)
                Cursor.lockState = CursorLockMode.Locked;
        }

        public bool SwitchMouseState()
        {
            cursorVisible = !cursorVisible;
            Cursor.visible = cursorVisible;

            if (cursorVisible)
                Cursor.lockState = CursorLockMode.Confined;
            else
                Cursor.lockState = CursorLockMode.Locked;

            return !cursorVisible; // Unvisible 상태 시 카메라 회전 가능하기 위해 반전 값 리턴
        }
    }
}