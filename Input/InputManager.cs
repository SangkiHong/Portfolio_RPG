using UnityEngine;
using UnityEngine.InputSystem;

namespace SK
{
    public class InputManager : MonoBehaviour
    {
        public PlayerInputAction playerInputAction;

        private void Awake() {
            playerInputAction = new PlayerInputAction();
        }

        private void OnEnable()
        {
            playerInputAction.Enable();
        }

        private void OnDisable()
        {
            playerInputAction.Disable();
        }
    }
}
