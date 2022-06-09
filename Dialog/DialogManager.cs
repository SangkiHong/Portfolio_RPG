using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

namespace SK.Dialog
{
    public struct Dialog 
    {
        public List<string> scripts; 
    }

    [System.Serializable]
    public class SerializableDicDialog : Utilities.SerializableDictionary<string, Dialog> { }

    public class DialogManager : MonoBehaviour
    {
        [Header("Varialbe")]
        public float dialogueAngle;
        [SerializeField] private float scriptingSpeed;

        [Header("UI")]
        [SerializeField] private CanvasGroup window_Dialog;
        [SerializeField] private Text text_dialog;
        [SerializeField] private Button button_Continue;
        [SerializeField] private Button button_Rewind;
        [SerializeField] private Button button_Quest;
        [SerializeField] private Button button_Shop;
        [SerializeField] private Button button_Exit;

        [Header("Dialog Data")] // 대화 데이터를 담을 딕셔너리
        public SerializableDicDialog dialogsDic;

        private InputAction _Input_Conversation;
        private InputAction _Input_ContinueDialogue;
        private InputAction _Input_CloseDialogue;

        private StringBuilder _scriptingBuilder;

        private string _currentNpcCodeName, _currentScript;
        private char _signNewLine = '/';
        private char _changeNewLine = '\n';
        private int _scriptsIndex, _scriptLength, _scriptingIndex;
        private float _elapsed;
        private bool _isScripting;

        public void Initialize()
        {
            dialogsDic = new SerializableDicDialog();

            // 대화 데이터 로드
            GameManager.Instance.DataManager.LoadDialogData(ref dialogsDic);

            // 대화 시작 단축키 초기화
            var _input = GameManager.Instance.InputManager.playerInput;
            _Input_Conversation = _input.actions["Conversation"];
            _Input_ContinueDialogue = _input.actions["ContinueDialogue"];
            _Input_CloseDialogue = _input.actions["CloseDialogue"];

            // 스트링빌더 초기화
            _scriptingBuilder = new StringBuilder();

            // 단축키 이벤트 할당
            _Input_ContinueDialogue.started += ContinueDialogue;
            _Input_CloseDialogue.started += CloseDialogue;

            Debug.Log("DialogManager 초기화 완료");
        }

        public void AssignNPC(string codeName)
        {
            // 단축키 이벤트 할당
            _Input_Conversation.started += StartConversation;

            _currentNpcCodeName = codeName;
            _scriptsIndex = 0;
            Debug.Log($"{codeName} NPC의 할당 완료");
        }

        public void UnassignNPC()
        {
            // 단축키 이벤트 할당 해제
            _Input_Conversation.started -= StartConversation;
            Debug.Log($"NPC의 할당 해제");
        }

        private void StartConversation(InputAction.CallbackContext context)
        {
            // 대화 창 열기 전에 모든 창 닫기
            GameManager.Instance.UIManager.CloseAllWindows();
            // 인풋 모드를 대화로 변경
            GameManager.Instance.InputManager.SwitchInputMode(InputMode.Conversation);

            // 카메로 회전 고정
            GameManager.Instance.Player.cameraManager.CameraRotatingHold(true);
            // 마우스 화면에 표시 전환
            GameManager.Instance.SwitchMouseState(true);

            // 대화 창 열기
            window_Dialog.alpha = 1;
            window_Dialog.blocksRaycasts = true;

            // 해당 대화 스크립팅하는 함수 호출
            Scripting(dialogsDic[_currentNpcCodeName].scripts[_scriptsIndex++]);
        }

        // 대화를 처음부터 다시 하는 함수
        public void RewindDialogue()
        {
            _scriptsIndex = 0;
            Scripting(dialogsDic[_currentNpcCodeName].scripts[_scriptsIndex++]);
        }

        // 대화를 이어가거나 대화를 즉시 표시하는 함수
        public void ContinueDialogue()
        {
            // 현재 스크립팅 중인 경우 스크립팅을 즉시 완료
            if (_isScripting)
            {
                _isScripting = false;

                _scriptingBuilder.Clear();
                _scriptingBuilder.Append(_currentScript);
                _scriptingBuilder.Replace(_signNewLine, _changeNewLine);
                text_dialog.text = _scriptingBuilder.ToString();

                if (dialogsDic[_currentNpcCodeName].scripts.Count > _scriptsIndex)
                    button_Continue.gameObject.SetActive(true);
            }
            else
            {
                if (dialogsDic[_currentNpcCodeName].scripts.Count > _scriptsIndex)
                    Scripting(dialogsDic[_currentNpcCodeName].scripts[_scriptsIndex++]);
            }
        }

        // 내부 인풋 이벤트 호출용 함수
        private void ContinueDialogue(InputAction.CallbackContext context)
            => ContinueDialogue();

        // 대화 창을 닫는 함수
        public void CloseDialogue()
        {
            // 대화 창 닫기
            window_Dialog.alpha = 0;
            window_Dialog.blocksRaycasts = false;

            // 인덱스 초기화
            _scriptsIndex = 0;

            // 카메로 회전 고정
            GameManager.Instance.Player.cameraManager.CameraRotatingHold(false);
            // 마우스 화면에 표시 전환
            GameManager.Instance.SwitchMouseState(false);

            // 인풋 모드를 게임플레이로 변경
            GameManager.Instance.InputManager.SwitchInputMode(InputMode.GamePlay);
        }

        // 내부 인풋 이벤트 호출용 함수
        private void CloseDialogue(InputAction.CallbackContext context)
            => CloseDialogue();

        // 화면에 대화 스크립트가 표시되도록 초기 세팅해주는 함수
        private void Scripting(string script)
        {
            // 초기화
            if (_scriptingBuilder.Length > 0)
            {
                text_dialog.text = string.Empty;
                _scriptingBuilder.Clear();
            }

            button_Continue.gameObject.SetActive(false);

            _currentScript = script;
            _scriptLength = script.Length;
            _scriptingIndex = 0;
            _elapsed = 0;
            _isScripting = true;
        }

        private void Update()
        {
            // 스크립팅 중이라면 일정 간격으로 대화 스트링 값을 추가하며 출력
            if (_isScripting)
            {
                _elapsed += Time.deltaTime;

                if (_elapsed >= scriptingSpeed)
                {
                    if (_currentScript[_scriptingIndex] != _signNewLine)
                        _scriptingBuilder.Append(_currentScript[_scriptingIndex++]);
                    else
                    {
                        _scriptingBuilder.AppendLine();
                        _scriptingIndex++;
                        return;
                    }

                    text_dialog.text = _scriptingBuilder.ToString();

                    // 대화 스크립트를 모두 출력한 경우
                    if (_scriptingIndex >= _scriptLength)
                    {
                        _isScripting = false;
                        if (dialogsDic[_currentNpcCodeName].scripts.Count > _scriptsIndex)
                            button_Continue.gameObject.SetActive(true);
                    }
                }
            }
        }

        private void OnApplicationQuit()
        {
            _Input_Conversation.started -= StartConversation;
            _Input_ContinueDialogue.started -= ContinueDialogue;
            _Input_CloseDialogue.started -= CloseDialogue;
        }
    }
}
