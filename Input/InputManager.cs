using UnityEngine;
using UnityEngine.InputSystem;

namespace SK
{
    // 인풋 모드 Enum
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
            // GamePlay Action Map 사용 가능
            playerInput.actions.actionMaps[0].Enable();
            // UI Action Map 사용 중지
            playerInput.actions.actionMaps[1].Disable();
            // 퀵 슬롯 Action Map 사용 가능
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
            // 인풋 시스템 Action Map을 UI로 변경_220525
            switch (inputMode)
            {
                case InputMode.GamePlay:
                    // 마우스 화면에 표시 전환
                    GameManager.Instance.SwitchMouseState(false);

                    playerInput.SwitchCurrentActionMap(_ActionMapGamePlay);

                    _input_UI_Quest.Enable();
                    _input_UI_Skill.Enable();
                    _input_UI_Inventory.Enable();
                    _input_UI_CharacterStats.Enable();
                    _input_UI_CloseAllWindows.Enable();
                    _input_MiniMapZoomIn.Enable();
                    _input_MiniMapZoomOut.Enable();

                    // 퀵 슬롯 액션맵 사용 가능
                    playerInput.actions.actionMaps[3].Enable();
                    break;
                case InputMode.UI:
                    if (playerInput.currentActionMap.name != _ActionMapUI)
                    {
                        // 마우스 화면에 표시 전환
                        GameManager.Instance.SwitchMouseState(true);

                        // 인풋액션 세팅
                        playerInput.SwitchCurrentActionMap(_ActionMapUI);
                        _input_UI_Inventory.Enable();
                        _input_UI_CharacterStats.Enable();
                        _input_UI_Quest.Enable();
                        _input_UI_Skill.Enable();
                        _input_UI_CloseAllWindows.Enable();
                        _input_ContinueDialogue.Disable();
                        _input_CloseDialogue.Disable();

                        // 퀵 슬롯 액션맵 사용 가능
                        playerInput.actions.actionMaps[3].Enable();
                    }
                    break;
                case InputMode.Conversation:
                    // 마우스 화면에 표시 전환
                    GameManager.Instance.SwitchMouseState(true);

                    // 인풋액션 세팅
                    playerInput.SwitchCurrentActionMap(_ActionMapUI);
                    _input_UI_Inventory.Disable();
                    _input_UI_CharacterStats.Disable();
                    _input_UI_Quest.Disable();
                    _input_UI_Skill.Disable();
                    _input_UI_CloseAllWindows.Disable();
                    _input_ContinueDialogue.Enable();
                    _input_CloseDialogue.Enable();

                    // 퀵 슬롯 액션맵 사용 중지
                    playerInput.actions.actionMaps[3].Disable();
                    break;
                case InputMode.Disable:
                    // 마우스 화면에 표시 전환
                    GameManager.Instance.SwitchMouseState(true);

                    // 인풋액션 세팅
                    playerInput.SwitchCurrentActionMap(_ActionMapUI);
                    _input_UI_Inventory.Disable();
                    _input_UI_CharacterStats.Disable();
                    _input_UI_Quest.Disable();
                    _input_UI_Skill.Disable();
                    _input_UI_CloseAllWindows.Disable();
                    _input_ContinueDialogue.Disable();
                    _input_CloseDialogue.Disable();

                    // 퀵 슬롯 액션맵 사용 중지
                    playerInput.actions.actionMaps[3].Disable();
                    break;
            }
        }


        private void OnApplicationQuit()
        {
            // 플레이어 인풋 이벤트 할당 해제
            if (GameManager.Instance.Player)
                GameManager.Instance.Player.inputActions.UnassignInputEvent();
            // UI 관련 인풋 이벤트 할당 해제
            if (UI.UIManager.Instance)
                UI.UIManager.Instance.UnassignInputActions();

            playerInput.actions.Disable();
        }
    }
}
