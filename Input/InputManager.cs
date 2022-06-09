using UnityEngine;
using UnityEngine.InputSystem;

namespace SK
{
    // ��ǲ ��� Enum
    public enum InputMode
    {
        GamePlay,
        UI,
        Conversation
    }

    public class InputManager : MonoBehaviour
    {
        public PlayerInput playerInput;

        private InputAction _input_UI_Inventory;
        private InputAction _input_UI_CharacterStats;
        private InputAction _input_UI_Quest;
        private InputAction _input_UI_CloseAllWindows;
        private InputAction _input_ContinueDialogue;
        private InputAction _input_CloseDialogue;

        private const string _ActionMapGamePlay = "GamePlay";
        private const string _ActionMapUI = "UI";

        private void OnEnable()
        {
            // GamePlay Action Map ��
            playerInput.actions.actionMaps[0].Enable();
            // UI Action Map ��
            playerInput.actions.actionMaps[1].Disable();
        }

        private void Start()
        {
            _input_UI_Inventory = playerInput.actions["UI_Inventory"];
            _input_UI_CharacterStats = playerInput.actions["UI_CharacterStats"];
            _input_UI_Quest = playerInput.actions["UI_Quest"];
            _input_UI_CloseAllWindows = playerInput.actions["UI_CloseAllWindows"];
            _input_ContinueDialogue = playerInput.actions["ContinueDialogue"];
            _input_CloseDialogue = playerInput.actions["CloseDialogue"];
        }

        public void SwitchInputMode(InputMode inputMode)
        {
            // ��ǲ �ý��� Action Map�� UI�� ����_220525
            if (inputMode == InputMode.GamePlay)
            {
                playerInput.SwitchCurrentActionMap(_ActionMapGamePlay);
            }
            else if (inputMode == InputMode.UI)
            {
                if (playerInput.currentActionMap.name != _ActionMapUI)
                { 
                    playerInput.SwitchCurrentActionMap(_ActionMapUI);
                    _input_UI_Inventory.Enable();
                    _input_UI_CharacterStats.Enable();
                    _input_UI_Quest.Enable();
                    _input_UI_CloseAllWindows.Enable();
                    _input_ContinueDialogue.Disable();
                    _input_CloseDialogue.Disable();
                }
            }
            else if (inputMode == InputMode.Conversation)
            {
                playerInput.SwitchCurrentActionMap(_ActionMapUI);
                _input_UI_Inventory.Disable();
                _input_UI_CharacterStats.Disable();
                _input_UI_Quest.Disable();
                _input_UI_CloseAllWindows.Disable();
                _input_ContinueDialogue.Enable();
                _input_CloseDialogue.Enable();
            }
        }


        private void OnApplicationQuit()
        {
            // �÷��̾� ��ǲ �̺�Ʈ �Ҵ� ����
            if (GameManager.Instance.Player)
                GameManager.Instance.Player.inputActions.UnassignInputEvent();
            // UI ���� ��ǲ �̺�Ʈ �Ҵ� ����
            if (GameManager.Instance.UIManager)
                GameManager.Instance.UIManager.UnassignInputActions();

            playerInput.actions.Disable();
        }
    }
}
