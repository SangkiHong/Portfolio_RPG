using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

namespace SK.Dialog
{
    /* �ۼ���: ȫ���
     * ����: NPC���� ��ȭ�� ���õ� �������� ����� ������ Ŭ����
     * �ۼ���: 22�� 6�� 9��
     */

    // NPC�� ��ũ��Ʈ(���)�� ��� �ִ� ����ü
    public struct Dialog 
    {
        public List<string> scripts; 
    }

    // ��ųʸ� ����ȭ Ŭ����
    [System.Serializable]
    public class SerializableDicDialog : Utilities.SerializableDictionary<string, Dialog> { }

    public class DialogManager : MonoBehaviour
    {
        [Header("Varialbe")]
        public float dialogueAngle; // ��ȭ ���� ����
        [SerializeField] private float scriptingSpeed; // ��� ǥ�� �ӵ�

        [Header("UI")]
        [SerializeField] private CanvasGroup window_Dialog;
        [SerializeField] private Text text_dialog;
        [SerializeField] private Button button_Continue;
        [SerializeField] private Button button_Rewind;
        [SerializeField] private Button button_Quest;
        [SerializeField] private Button button_Shop;
        [SerializeField] private Button button_Exit;

        [SerializeField] private GameObject acceptQuestParent;
        [SerializeField] private Button button_AcceptQuest;
        [SerializeField] private Button button_DeclineQuest;

        [Header("Dialog Data")]
        public SerializableDicDialog dialogsDic; // ��ȭ �����͸� ���� ��ųʸ�(����ȭ)

        // ��ǲ �׼�
        private InputAction _Input_Conversation;
        private InputAction _Input_ContinueDialogue;
        private InputAction _Input_CloseDialogue;

        // ���۷���
        private InputManager _inputManager;
        private UI.UIManager _uiManager;
        private CameraManager _cameraManager;
        private UI.QuestManager _questManager;
        
        // ��縦 ���� ��Ʈ������
        private StringBuilder _scriptingBuilder;
        
        // ���� ���õ� NPC
        private NPC _currentNPC;
                
        // '/'�� �ִ� ��� '\n'���� ��ȯ
        private readonly char _signNewLine = '/';
        private readonly char _changeNewLine = '\n';

        private string _targetDialogKey, _currentNpcCodeName, _currentScript, _selectedQuestName;

        private int _scriptsIndex, _scriptLength, _scriptingIndex, _assignedNum;
        private float _elapsed;
        private bool _isScripting, _isQuestDialog;

        // UI �Ŵ����� ���� ȣ��� �ʱ�ȭ �Լ�
        public void Initialize(UI.QuestManager questManager)
        {
            _questManager = questManager;
            _inputManager = GameManager.Instance.InputManager;
            _uiManager = GameManager.Instance.UIManager;
            _cameraManager = GameManager.Instance.Player.cameraManager;

            dialogsDic = new SerializableDicDialog();

            // ��ȭ ������ �ε�
            GameManager.Instance.DataManager.LoadDialogData(ref dialogsDic);

            // ��ȭ ���� ����Ű �ʱ�ȭ
            var _input = _inputManager.playerInput;
            _Input_Conversation = _input.actions["Conversation"];
            _Input_ContinueDialogue = _input.actions["ContinueDialogue"];
            _Input_CloseDialogue = _input.actions["CloseDialogue"];

            // ��Ʈ������ �ʱ�ȭ
            _scriptingBuilder = new StringBuilder();

            // ����Ű �̺�Ʈ �Ҵ�
            _Input_ContinueDialogue.started += ContinueDialogue;
            _Input_CloseDialogue.started += CloseDialogue;

            // ��ư �̺�Ʈ �Ҵ�
            button_Shop.onClick.AddListener(delegate { ShopButton(); });
            button_Quest.onClick.AddListener(delegate { QuestButton(); });
            button_AcceptQuest.onClick.AddListener(delegate { AcceptQuest(); });
            button_DeclineQuest.onClick.AddListener(delegate { DeclineQuest(); });

            Debug.Log("DialogManager �ʱ�ȭ �Ϸ�");
        }

        // NPC�� ��ȭ ���� ���¿��� ȣ��� �Լ�(NPC�� �ڵ����)
        public void AssignNPC(string codeName)
        {
            // ����Ű �̺�Ʈ �Ҵ�
            _Input_Conversation.started += StartConversation;

            _currentNpcCodeName = codeName;
            _assignedNum++;
            _scriptsIndex = 0;
        }

        // NPC ���� ����
        public void UnassignNPC()
        {
            // ����Ű �̺�Ʈ �Ҵ� ����
            if (--_assignedNum == 0)
                _Input_Conversation.started -= StartConversation;
        }

        #region ��ư �̺�Ʈ �Լ�
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

                if (dialogsDic[_targetDialogKey].scripts.Count > _scriptsIndex)
                    button_Continue.gameObject.SetActive(true);
            }
            else
            {
                if (dialogsDic[_targetDialogKey].scripts.Count > _scriptsIndex)
                    Scripting(dialogsDic[_targetDialogKey].scripts[_scriptsIndex++]);
            }

            // ����Ʈ ���� ��ȭ ���̶�� ����Ʈ ���� ǥ��
            if (_isQuestDialog) DisplayQuestInfo();
        }

        // ��ȭ â�� �ݴ� �Լ�
        public void CloseButton()
        {
            // ��ȭ â �ݱ�
            window_Dialog.alpha = 0;
            window_Dialog.blocksRaycasts = false;

            // �ε��� �ʱ�ȭ
            _scriptsIndex = 0;

            // ī�޷� ȸ�� ����
            _cameraManager.CameraRotatingHold(false);
            // ���콺 ȭ�鿡 ǥ�� ��ȯ
            GameManager.Instance.SwitchMouseState(false);

            // ��ǲ ��带 �����÷��̷� ����
            _inputManager.SwitchInputMode(InputMode.GamePlay);
        }

        // ���� ��ư�� �Լ�
        private void ShopButton()
        {
            // �Ϲ� ���� ����
            if (_currentNPC.NpcType == NPC.NPCType.PropShop)
            {

            }
            // ��� ���� ����
            else
            {

            }
        }

        #region ����Ʈ ���� �Լ�

        // ����Ʈ ��ư�� �Լ�
        private void QuestButton()
        {
            _isQuestDialog = true;
            _scriptsIndex = 0;
            // ����Ʈ ���� ��ȭ ��ũ���� ����
            _targetDialogKey = _selectedQuestName;
            Scripting(dialogsDic[_targetDialogKey].scripts[_scriptsIndex++]);

            // 'R' ��ư�� �̺�Ʈ �Լ��� ����Ʈ ��ȭ�� ��ȯ
            _Input_ContinueDialogue.started -= ContinueDialogue;
            _Input_ContinueDialogue.started += QuestDialogue;

            // ����Ʈ ���� ǥ��
            DisplayQuestInfo();
        }

        // ����Ʈ ���� ǥ�� �� ���� ��ư ǥ��
        private void DisplayQuestInfo()
        {
            // ����Ʈ ���� ǥ��
            if (dialogsDic[_selectedQuestName].scripts.Count <= _scriptsIndex)
            {
                _questManager.OpenQuestInfo();

                // ���� ���� ��ư ǥ��
                acceptQuestParent.SetActive(true);
            }
        }

        // ����Ʈ ���� ��ư�� �Լ�
        private void AcceptQuest()
        {
            _isQuestDialog = false;
            _scriptsIndex = 0;
            acceptQuestParent.SetActive(false);

            // ������ ���� ��ȭ ǥ��
            _targetDialogKey = _selectedQuestName + "_Accept";
            Scripting(dialogsDic[_targetDialogKey].scripts[_scriptsIndex++]);

            // 'R' ��ư�� �̺�Ʈ �Լ��� �Ϲ� ��ȭ�� ��ȯ
            _Input_ContinueDialogue.started += ContinueDialogue;
            _Input_ContinueDialogue.started -= QuestDialogue;
        }

        // ����Ʈ ���� ��ư�� �Լ�
        private void DeclineQuest()
        {
            _isQuestDialog = false;
            _scriptsIndex = 0;
            acceptQuestParent.SetActive(false);

            // ������ ���� ��ȭ ǥ��
            _targetDialogKey = _selectedQuestName + "_Decline";
            Scripting(dialogsDic[_targetDialogKey].scripts[_scriptsIndex++]);

            // 'R' ��ư�� �̺�Ʈ �Լ��� �Ϲ� ��ȭ�� ��ȯ
            _Input_ContinueDialogue.started += ContinueDialogue;
            _Input_ContinueDialogue.started -= QuestDialogue;
        }
        #endregion
        #endregion

        #region Input �̺�Ʈ �Լ�
        // ��ȭ�� ����
        private void StartConversation(InputAction.CallbackContext context)
        {
            // ��ȭ â ���� ���� ��� â �ݱ�
            _uiManager.CloseAllWindows();
            // ��ǲ ��带 ��ȭ�� ����
            _inputManager.SwitchInputMode(InputMode.Conversation);

            // ī�޷� ȸ�� ����
            _cameraManager.CameraRotatingHold(true);
            // ���콺 ȭ�鿡 ǥ�� ��ȯ
            GameManager.Instance.SwitchMouseState(true);

            // npc���� ����Ʈ�� ���� �� �ִ� ��� ����Ʈ ��ư ǥ��
            _currentNPC = SceneManager.Instance.GetNPC(_currentNpcCodeName);

            // ��ư �ʱ�ȭ
            button_Quest.gameObject.SetActive(false);
            button_Shop.gameObject.SetActive(false);

            // NPC���� Quest�� �ִ� ���
            if (_currentNPC.NpcQuests != null)
            {
                for (int i = 0; i < _currentNPC.NpcQuests.Count; i++)
                {
                    if (_questManager.IsAcceptable(_currentNPC.NpcQuests[i]))
                    {
                        _selectedQuestName = _currentNPC.NpcQuests[i].name;
                        button_Quest.gameObject.SetActive(true);
                        break;
                    }
                }
            }

            // ���� �̿� ���� NPC�� ��� ���� �̿� ��ư ǥ��
            if (_currentNPC.NpcType == NPC.NPCType.PropShop ||
                _currentNPC.NpcType == NPC.NPCType.EquipmentShop)
                button_Shop.gameObject.SetActive(true);

            // ��ȭ â ����
            window_Dialog.alpha = 1;
            window_Dialog.blocksRaycasts = true;

            // �ش� ��ȭ ��ũ�����ϴ� �Լ� ȣ��
            _targetDialogKey = _currentNpcCodeName;
            Scripting(dialogsDic[_targetDialogKey].scripts[_scriptsIndex++]);
        }

        // ���� ��ȭ�� ǥ��
        private void ContinueDialogue(InputAction.CallbackContext context)
        {
            _targetDialogKey = _currentNpcCodeName;
            ContinueDialogue(); 
        }

        // ����Ʈ ���� ��ȭ ǥ��
        private void QuestDialogue(InputAction.CallbackContext context)
        {
            _targetDialogKey = _selectedQuestName;
            ContinueDialogue();

            // ����Ʈ ���� ǥ��
            DisplayQuestInfo();
        }

        // ��ȭ â�� ����
        private void CloseDialogue(InputAction.CallbackContext context)
            => CloseButton();
        #endregion

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

        private void FixedUpdate()
        {
            // ��ũ���� ���̶�� ���� �������� ��ȭ ��Ʈ�� ���� �߰��ϸ� ���
            if (_isScripting)
            {
                _elapsed += Time.fixedDeltaTime;

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
                        if (dialogsDic[_targetDialogKey].scripts.Count > _scriptsIndex)
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
