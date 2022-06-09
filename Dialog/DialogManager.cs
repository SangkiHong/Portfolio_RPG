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

        [Header("Dialog Data")] // ��ȭ �����͸� ���� ��ųʸ�
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

            // ��ȭ ������ �ε�
            GameManager.Instance.DataManager.LoadDialogData(ref dialogsDic);

            // ��ȭ ���� ����Ű �ʱ�ȭ
            var _input = GameManager.Instance.InputManager.playerInput;
            _Input_Conversation = _input.actions["Conversation"];
            _Input_ContinueDialogue = _input.actions["ContinueDialogue"];
            _Input_CloseDialogue = _input.actions["CloseDialogue"];

            // ��Ʈ������ �ʱ�ȭ
            _scriptingBuilder = new StringBuilder();

            // ����Ű �̺�Ʈ �Ҵ�
            _Input_ContinueDialogue.started += ContinueDialogue;
            _Input_CloseDialogue.started += CloseDialogue;

            Debug.Log("DialogManager �ʱ�ȭ �Ϸ�");
        }

        public void AssignNPC(string codeName)
        {
            // ����Ű �̺�Ʈ �Ҵ�
            _Input_Conversation.started += StartConversation;

            _currentNpcCodeName = codeName;
            _scriptsIndex = 0;
            Debug.Log($"{codeName} NPC�� �Ҵ� �Ϸ�");
        }

        public void UnassignNPC()
        {
            // ����Ű �̺�Ʈ �Ҵ� ����
            _Input_Conversation.started -= StartConversation;
            Debug.Log($"NPC�� �Ҵ� ����");
        }

        private void StartConversation(InputAction.CallbackContext context)
        {
            // ��ȭ â ���� ���� ��� â �ݱ�
            GameManager.Instance.UIManager.CloseAllWindows();
            // ��ǲ ��带 ��ȭ�� ����
            GameManager.Instance.InputManager.SwitchInputMode(InputMode.Conversation);

            // ī�޷� ȸ�� ����
            GameManager.Instance.Player.cameraManager.CameraRotatingHold(true);
            // ���콺 ȭ�鿡 ǥ�� ��ȯ
            GameManager.Instance.SwitchMouseState(true);

            // ��ȭ â ����
            window_Dialog.alpha = 1;
            window_Dialog.blocksRaycasts = true;

            // �ش� ��ȭ ��ũ�����ϴ� �Լ� ȣ��
            Scripting(dialogsDic[_currentNpcCodeName].scripts[_scriptsIndex++]);
        }

        // ��ȭ�� ó������ �ٽ� �ϴ� �Լ�
        public void RewindDialogue()
        {
            _scriptsIndex = 0;
            Scripting(dialogsDic[_currentNpcCodeName].scripts[_scriptsIndex++]);
        }

        // ��ȭ�� �̾�ų� ��ȭ�� ��� ǥ���ϴ� �Լ�
        public void ContinueDialogue()
        {
            // ���� ��ũ���� ���� ��� ��ũ������ ��� �Ϸ�
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

        // ���� ��ǲ �̺�Ʈ ȣ��� �Լ�
        private void ContinueDialogue(InputAction.CallbackContext context)
            => ContinueDialogue();

        // ��ȭ â�� �ݴ� �Լ�
        public void CloseDialogue()
        {
            // ��ȭ â �ݱ�
            window_Dialog.alpha = 0;
            window_Dialog.blocksRaycasts = false;

            // �ε��� �ʱ�ȭ
            _scriptsIndex = 0;

            // ī�޷� ȸ�� ����
            GameManager.Instance.Player.cameraManager.CameraRotatingHold(false);
            // ���콺 ȭ�鿡 ǥ�� ��ȯ
            GameManager.Instance.SwitchMouseState(false);

            // ��ǲ ��带 �����÷��̷� ����
            GameManager.Instance.InputManager.SwitchInputMode(InputMode.GamePlay);
        }

        // ���� ��ǲ �̺�Ʈ ȣ��� �Լ�
        private void CloseDialogue(InputAction.CallbackContext context)
            => CloseDialogue();

        // ȭ�鿡 ��ȭ ��ũ��Ʈ�� ǥ�õǵ��� �ʱ� �������ִ� �Լ�
        private void Scripting(string script)
        {
            // �ʱ�ȭ
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
            // ��ũ���� ���̶�� ���� �������� ��ȭ ��Ʈ�� ���� �߰��ϸ� ���
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

                    // ��ȭ ��ũ��Ʈ�� ��� ����� ���
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
