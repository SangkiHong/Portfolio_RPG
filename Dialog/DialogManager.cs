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
        [SerializeField] private Text text_NPCName;
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
        private Quests.QuestManager _questManager;
        private Data.ItemListManager _itemListManager;
        
        // ��縦 ���� ��Ʈ������
        private StringBuilder _scriptingBuilder;
        
        // ���� ���õ� NPC
        private NPC _currentNPC;

        // '/'�� �ִ� ��� '\n'���� ��ȯ
        private readonly char _signNewLine = '/';
        private readonly char _changeNewLine = '\n';
        // ����Ʈ ���� �ÿ� "NPC�̸�" �ڿ� �߰��Ͽ� ��ȭ Ű�� ã��
        private readonly string _string_Accept = "_Accept";
        // ����Ʈ ���� �ÿ� "NPC�̸�" �ڿ� �߰��Ͽ� ��ȭ Ű�� ã��
        private readonly string _string_Decline = "_Decline";
        // ����Ʈ �Ϸ� �ÿ� "NPC�̸�" �ڿ� �߰��Ͽ� ��ȭ Ű�� ã��
        private readonly string _string_Success = "_Success";

        private string _targetDialogKey, _currentNpcCodeName, _currentScript, _selectedQuestName;

        private int _scriptsIndex, _scriptLength, _scriptingIndex, _assignedNum;
        private float _elapsed;
        private bool _isScripting, _isQuestDialog;

        // UI �Ŵ����� ���� ȣ��� �ʱ�ȭ �Լ�
        public void Initialize(Quests.QuestManager questManager)
        {
            _questManager = questManager;
            _inputManager = GameManager.Instance.InputManager;
            _uiManager = UI.UIManager.Instance;
            _cameraManager = GameManager.Instance.Player.cameraManager;
            _itemListManager = GameManager.Instance.ItemListManager;

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
            _Input_Conversation.started += StartConversation;
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
            _currentNpcCodeName = codeName;
            _assignedNum++;
            _scriptsIndex = 0;
        }

        // NPC ���� ����
        public void UnassignNPC()
        {
            // ����Ű �̺�Ʈ �Ҵ� ����
            if (--_assignedNum == 0)
                _currentNpcCodeName = null;
        }

        #region ��ư �̺�Ʈ �Լ�
        // ��ȭ�� ó������ �ٽ� �ϴ� �Լ�
        public void RewindDialogue()
        {
            _scriptsIndex = 0;
            _targetDialogKey = _currentNpcCodeName;
            Scripting(dialogsDic[_targetDialogKey].scripts[_scriptsIndex++]);

            // ��ư �ʱ�ȭ
            acceptQuestParent.SetActive(false);

            // ���� ���� ����Ʈ Ȯ��
            HasNpcAcceptableQuest();
        }

        // ��ȭ�� �̾�ų� ��ȭ�� ��� ǥ���ϴ� �Լ�
        public void ContinueDialogue()
        {
            // ���� ��ũ���� ���� ��� ��ũ������ ��� �Ϸ�
            if (_isScripting)
            {
                _isScripting = false;
                SceneManager.Instance.OnFixedUpdate -= FixedTick;

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
            if (_uiManager.CloseAllWindows())
            {
                // �ε��� �ʱ�ȭ
                _scriptsIndex = 0;

                // ���ͷ��� ī�޶� ����
                _cameraManager.ChangeInteractingCamera(false);

                // ��ǲ ��带 �����÷��̷� ����
                _inputManager.SwitchInputMode(InputMode.GamePlay);
            }
        }

        // ���� ��ư�� �Լ�
        private void ShopButton()
        {
            // �Ϲ� ���� ����
            if (_currentNPC.NpcType == NPC.NPCType.PropShop)
                _uiManager.shopManager.OpenShop(UI.ShopType.Props);
            // ��� ���� ����
            else
                _uiManager.shopManager.OpenShop(UI.ShopType.Equipments);
        }

        #region ����Ʈ ���� �Լ�
        // ���� ������ ����Ʈ�� �ִ� �� Ȯ��
        private void HasNpcAcceptableQuest()
        {
            // NPC���� Quest�� �ִ� ���
            if (_currentNPC.NpcQuests != null)
            {
                for (int i = 0; i < _currentNPC.NpcQuests.Count; i++)
                {
                    if (_questManager.IsActivated(_currentNPC.NpcQuests[i]) || _questManager.IsAcceptable(_currentNPC.NpcQuests[i]))
                    {
                        _selectedQuestName = _currentNPC.NpcQuests[i].name;
                        button_Quest.gameObject.SetActive(true);
                        return;
                    }
                }
            }

            // ����Ʈ�� ���ٸ� ���� �Ҵ� ����
            _selectedQuestName = null;
            button_Quest.gameObject.SetActive(false);
        }

        // ����Ʈ ��ȭ ��ư�� �Լ�
        private void QuestButton()
        {
            // ���� ���� ��ư ����
            acceptQuestParent.SetActive(false);

            _scriptsIndex = 0;
            int questCount = _currentNPC.NpcQuests.Count;
            Quests.Quest selectedQuest;

            for (int i = 0; i < _currentNPC.NpcQuests.Count; i++)
            {
                selectedQuest = _currentNPC.NpcQuests[i];

                // �̹� NPC���� ���� ����Ʈ�� ���� ���
                if (_questManager.IsActivated(selectedQuest))
                {
                    // ���� �Ϸ����� ���� ���
                    if (!selectedQuest.IsCompletable)
                        _targetDialogKey = _selectedQuestName + _string_Accept;
                    // �Ϸ��� ���
                    else
                    {
                        // ����Ʈ �Ϸ� ������ ���
                        if (UI.UIManager.Instance.questManager.CompleteQuest(selectedQuest))
                        {
                            // ���� ������ ��� ǥ��
                            _uiManager.questManager.OpenRewardInfo(selectedQuest);

                            _targetDialogKey = _selectedQuestName + _string_Success;

                            // ���� ���� ����Ʈ Ȯ��
                            HasNpcAcceptableQuest();

                            // ���� ȿ��
                            AudioManager.Instance.PlayAudio(Strings.Audio_UI_CompleteQuest);
                        }
                        // ���� ������ �Ұ��� ���
                        else 
                            _targetDialogKey = Strings.Dialog_InventoryFull;
                    }
                    Scripting(dialogsDic[_targetDialogKey].scripts[_scriptsIndex++]);
                    return;
                }
                // ���� NPC���� ����Ʈ�� ���� ���� ���
                else
                {
                    _isQuestDialog = true;
                    // ����Ʈ ���� ��ȭ ��ũ���� ����
                    _targetDialogKey = _selectedQuestName;
                    Scripting(dialogsDic[_targetDialogKey].scripts[_scriptsIndex++]);

                    // 'R' ��ư�� �̺�Ʈ �Լ��� ����Ʈ ��ȭ�� ��ȯ
                    _Input_ContinueDialogue.started -= ContinueDialogue;
                    _Input_ContinueDialogue.started += QuestDialogue;

                    // ����Ʈ ���� ǥ��
                    DisplayQuestInfo();
                    break;
                }
            }
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
            _targetDialogKey = _selectedQuestName + _string_Accept;
            Scripting(dialogsDic[_targetDialogKey].scripts[_scriptsIndex++]);

            // 'R' ��ư�� �̺�Ʈ �Լ��� �Ϲ� ��ȭ�� ��ȯ
            _Input_ContinueDialogue.started += ContinueDialogue;
            _Input_ContinueDialogue.started -= QuestDialogue;

            // �ش� ����Ʈ�� Ȱ��ȭ
            _questManager.AddNewQuest();

            // ���� ȿ��
            AudioManager.Instance.PlayAudio(Strings.Audio_UI_QuestAccept);
        }

        // ����Ʈ ���� ��ư�� �Լ�
        private void DeclineQuest()
        {
            _isQuestDialog = false;
            _scriptsIndex = 0;
            acceptQuestParent.SetActive(false);

            // ������ ���� ��ȭ ǥ��
            _targetDialogKey = _selectedQuestName + _string_Decline;
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
            if (_currentNpcCodeName == null) return;

            // ��ȭ â ���� ���� ��� â �ݱ�
            _uiManager.CloseAllWindows(true);

            // ���� ���� ��ư ����
            acceptQuestParent.SetActive(false);

            // ��ǲ ��带 ��ȭ�� ����
            _inputManager.SwitchInputMode(InputMode.Conversation);

            // ���Ŵ����� ���� NPC ������ ������
            _currentNPC = SceneManager.Instance.GetNPC(_currentNpcCodeName);

            // NPC �̸� �Ҵ�
            if (_currentNPC) text_NPCName.text = _currentNPC.DisplayName;
            else text_NPCName.text = string.Empty;

            // ���ͷ��� ī�޶� ����
            _cameraManager.ChangeInteractingCamera(true, _currentNPC);

            // NPC���� ��ȭ ������ �˸��� �Լ� ȣ��
            _currentNPC.StartConversation();

            // ��ư �ʱ�ȭ
            button_Shop.gameObject.SetActive(false);

            // ���� ���� ����Ʈ Ȯ��
            HasNpcAcceptableQuest();

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
            // ���õ� ����Ʈ�̸��� Ÿ�� ��ȭ Ű�� ����
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
            SceneManager.Instance.OnFixedUpdate += FixedTick;
        }

        private void FixedTick()
        {
            // ���� �������� ��ȭ ��Ʈ�� ���� �߰��ϸ� ���
            _elapsed += Time.fixedDeltaTime;

            if (_scriptingIndex < _currentScript.Length && _elapsed >= scriptingSpeed)
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
                    SceneManager.Instance.OnFixedUpdate -= FixedTick;
                    if (dialogsDic[_targetDialogKey].scripts.Count > _scriptsIndex)
                        button_Continue.gameObject.SetActive(true);
                }
            }
        }

        private void OnDisable()
        {
            _Input_Conversation.started -= StartConversation;
            _Input_ContinueDialogue.started -= ContinueDialogue;
            _Input_CloseDialogue.started -= CloseDialogue;
        }
    }
}
