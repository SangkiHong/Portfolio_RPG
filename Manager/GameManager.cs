using UnityEngine;

namespace SK
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private bool cursorVisible;
        
        private void Awake()
        {
            Cursor.visible = cursorVisible;
            if (!cursorVisible)
                Cursor.lockState = CursorLockMode.Locked;
        }
    }
}