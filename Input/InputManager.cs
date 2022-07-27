using UnityEngine;
using UnityEngine.InputSystem;

namespace SK
{
    // ��ǲ ��� Enum
    public enum InputMode
    {
        Disable,
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
        private InputAction _input_UI_Skill;
        private InputAction _input_UI_CloseAllWindows;
        private InputAction _input_ContinueDialogue;
        private InputAction _input_CloseDialogue;
        private InputAction _input_MiniMapZoomIn;
        private InputAction _input_MiniMapZoomOut;

        private const string _ActionMapGamePlay = "GamePlay";
        private const string _ActionMapUI = "UI";

        private void OnEnable()
        {
            // GamePlay Action Map ��� ����
            playerInput.actions.actionMaps[0].Enable();
            // UI Action Map ��� ����
            playerInput.actions.actionMaps[1].Disable();
            // �� ���� Action Map ��� ����
            playerInput.actions.actionMaps[3].Enable();
        }

        private void Start()
        {
            _input_UI_Inventory = playerInput.actions["UI_Inventory"];
            _input_UI_CharacterStats = playerInput.actions["UI_CharacterStats"];
            _input_UI_Quest = playerInput.actions["UI_Quest"];
            _input_UI_Skill = playerInput.actions["UI_Skill"];
            _input_UI_CloseAllWindows = playerInput.actions["UI_CloseAllWindows"];
            _input_ContinueDialogue = playerInput.actions["ContinueDialogue"];
            _input_CloseDialogue = playerInput.actions["CloseDialogue"];
            _input_MiniMapZoomIn = playerInput.actions["UI_ZoomIn"];
            _input_MiniMapZoomOut = playerInput.actions["UI_ZoomOut"];

        }

        public void SwitchInputMode(InputMode inputMode)
        {
            // ��ǲ �ý��� Action Map�� UI�� ����_220525
            switch (inputMode)
            {
                case InputMode.GamePlay:
                    // ���콺 ȭ�鿡 ǥ�� ��ȯ
                    GameManager.Instance.SwitchMouseState(false);

                    playerInput.SwitchCurrentActionMap(_ActionMapGamePlay);

                    _input_UI_Quest.Enable();
                    _input_UI_Skill.Enable();
                    _input_UI_Inventory.Enable();
                    _input_UI_CharacterStats.Enable();
                    _input_UI_CloseAllWindows.Enable();
                    _input_MiniMapZoomIn.Enable();
                    _input_MiniMapZoomOut.Enable();

                    // �� ���� �׼Ǹ� ��� ����
                    playerInput.actions.actionMaps[3].Enable();
                    break;
                case InputMode.UI:
                    if (playerInput.currentActionMap.name != _ActionMapUI)
                    {
                        // ���콺 ȭ�鿡 ǥ�� ��ȯ
                        GameManager.Instance.SwitchMouseState(true);

                        // ��ǲ�׼� ����
                        playerInput.SwitchCurrentActionMap(_ActionMapUI);
                        _input_UI_Inventory.Enable();
                        _input_UI_CharacterStats.Enable();
                        _input_UI_Quest.Enable();
                        _input_UI_Skill.Enable();
                        _input_UI_CloseAllWindows.Enable();
                        _input_ContinueDialogue.Disable();
                        _input_CloseDialogue.Disable();

                        // �� ���� �׼Ǹ� ��� ����
                        playerInput.actions.actionMaps[3].Enable();
                    }
                    break;
                case InputMode.Conversation:
                    // ���콺 ȭ�鿡 ǥ�� ��ȯ
                    GameManager.Instance.SwitchMouseState(true);

                    // ��ǲ�׼� ����
                    playerInput.SwitchCurrentActionMap(_ActionMapUI);
                    _input_UI_Inventory.Disable();
                    _input_UI_CharacterStats.Disable();
                    _input_UI_Quest.Disable();
                    _input_UI_Skill.Disable();
                    _input_UI_CloseAllWindows.Disable();
                    _input_ContinueDialogue.Enable();
                    _input_CloseDialogue.Enable();

                    // �� ���� �׼Ǹ� ��� ����
                    playerInput.actions.actionMaps[3].Disable();
                    break;
                case InputMode.Disable:
                    // ���콺 ȭ�鿡 ǥ�� ��ȯ
                    GameManager.Instance.SwitchMouseState(true);

                    // ��ǲ�׼� ����
                    playerInput.SwitchCurrentActionMap(_ActionMapUI);
                    _input_UI_Inventory.Disable();
                    _input_UI_CharacterStats.Disable();
                    _input_UI_Quest.Disable();
                    _input_UI_Skill.Disable();
                    _input_UI_CloseAllWindows.Disable();
                    _input_ContinueDialogue.Disable();
                    _input_CloseDialogue.Disable();

                    // �� ���� �׼Ǹ� ��� ����
                    playerInput.actions.actionMaps[3].Disable();
                    break;
            }
        }


        private void OnApplicationQuit()
        {
            // �÷��̾� ��ǲ �̺�Ʈ �Ҵ� ����
            if (GameManager.Instance.Player)
                GameManager.Instance.Player.inputActions.UnassignInputEvent();
            // UI ���� ��ǲ �̺�Ʈ �Ҵ� ����
            if (UI.UIManager.Instance)
                UI.UIManager.Instance.UnassignInputActions();

            playerInput.actions.Disable();
        }
    }
}
