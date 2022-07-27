using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

namespace SK.Dialog
{
    /* 작성자: 홍상기
     * 내용: NPC와의 대화와 관련된 전반적인 기능의 관리자 클래스
     * 작성일: 22년 6월 9일
     */

    // NPC의 스크립트(대사)를 담고 있는 구조체
    public struct Dialog 
    {
        public List<string> scripts; 
    }

    // 딕셔너리 직렬화 클래스
    [System.Serializable]
    public class SerializableDicDialog : Utilities.SerializableDictionary<string, Dialog> { }

    public class DialogManager : MonoBehaviour
    {
        [Header("Varialbe")]
        public float dialogueAngle; // 대화 가능 각도
        [SerializeField] private float scriptingSpeed; // 대사 표시 속도

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
        public SerializableDicDialog dialogsDic; // 대화 데이터를 담을 딕셔너리(직렬화)

        // 인풋 액션
        private InputAction _Input_Conversation;
        private InputAction _Input_ContinueDialogue;
        private InputAction _Input_CloseDialogue;

        // 레퍼런스
        private InputManager _inputManager;
        private UI.UIManager _uiManager;
        private CameraManager _cameraManager;
        private Quests.QuestManager _questManager;
        private Data.ItemListManager _itemListManager;
        
        // 대사를 담을 스트링빌더
        private StringBuilder _scriptingBuilder;
        
        // 현재 선택된 NPC
        private NPC _currentNPC;

        // '/'가 있는 경우 '\n'으로 변환
        private readonly char _signNewLine = '/';
        private readonly char _changeNewLine = '\n';
        // 퀘스트 수락 시에 "NPC이름" 뒤에 추가하여 대화 키를 찾음
        private readonly string _string_Accept = "_Accept";
        // 퀘스트 거절 시에 "NPC이름" 뒤에 추가하여 대화 키를 찾음
        private readonly string _string_Decline = "_Decline";
        // 퀘스트 완료 시에 "NPC이름" 뒤에 추가하여 대화 키를 찾음
        private readonly string _string_Success = "_Success";

        private string _targetDialogKey, _currentNpcCodeName, _currentScript, _selectedQuestName;

        private int _scriptsIndex, _scriptLength, _scriptingIndex, _assignedNum;
        private float _elapsed;
        private bool _isScripting, _isQuestDialog;

        // UI 매니저를 통해 호출될 초기화 함수
        public void Initialize(Quests.QuestManager questManager)
        {
            _questManager = questManager;
            _inputManager = GameManager.Instance.InputManager;
            _uiManager = UI.UIManager.Instance;
            _cameraManager = GameManager.Instance.Player.cameraManager;
            _itemListManager = GameManager.Instance.ItemListManager;

            dialogsDic = new SerializableDicDialog();

            // 대화 데이터 로드
            GameManager.Instance.DataManager.LoadDialogData(ref dialogsDic);

            // 대화 시작 단축키 초기화
            var _input = _inputManager.playerInput;
            _Input_Conversation = _input.actions["Conversation"];
            _Input_ContinueDialogue = _input.actions["ContinueDialogue"];
            _Input_CloseDialogue = _input.actions["CloseDialogue"];

            // 스트링빌더 초기화
            _scriptingBuilder = new StringBuilder();

            // 단축키 이벤트 할당
            _Input_Conversation.started += StartConversation;
            _Input_ContinueDialogue.started += ContinueDialogue;
            _Input_CloseDialogue.started += CloseDialogue;

            // 버튼 이벤트 할당
            button_Shop.onClick.AddListener(delegate { ShopButton(); });
            button_Quest.onClick.AddListener(delegate { QuestButton(); });
            button_AcceptQuest.onClick.AddListener(delegate { AcceptQuest(); });
            button_DeclineQuest.onClick.AddListener(delegate { DeclineQuest(); });

            Debug.Log("DialogManager 초기화 완료");
        }

        // NPC와 대화 가능 상태에서 호출될 함수(NPC의 코드네임)
        public void AssignNPC(string codeName)
        {
            _currentNpcCodeName = codeName;
            _assignedNum++;
            _scriptsIndex = 0;
        }

        // NPC 선택 해제
        public void UnassignNPC()
        {
            // 단축키 이벤트 할당 해제
            if (--_assignedNum == 0)
                _currentNpcCodeName = null;
        }

        #region 버튼 이벤트 함수
        // 대화를 처음부터 다시 하는 함수
        public void RewindDialogue()
        {
            _scriptsIndex = 0;
            _targetDialogKey = _currentNpcCodeName;
            Scripting(dialogsDic[_targetDialogKey].scripts[_scriptsIndex++]);

            // 버튼 초기화
            acceptQuestParent.SetActive(false);

            // 수락 가능 퀘스트 확인
            HasNpcAcceptableQuest();
        }

        // 대화를 이어가거나 대화를 즉시 표시하는 함수
        public void ContinueDialogue()
        {
            // 현재 스크립팅 중인 경우 스크립팅을 즉시 완료
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

            // 퀘스트 관련 대화 중이라면 퀘스트 수락 표시
            if (_isQuestDialog) DisplayQuestInfo();
        }

        // 대화 창을 닫는 함수
        public void CloseButton()
        {
            // 대화 창 닫기
            if (_uiManager.CloseAllWindows())
            {
                // 인덱스 초기화
                _scriptsIndex = 0;

                // 인터렉팅 카메라 해제
                _cameraManager.ChangeInteractingCamera(false);

                // 인풋 모드를 게임플레이로 변경
                _inputManager.SwitchInputMode(InputMode.GamePlay);
            }
        }

        // 상점 버튼의 함수
        private void ShopButton()
        {
            // 일반 상점 오픈
            if (_currentNPC.NpcType == NPC.NPCType.PropShop)
                _uiManager.shopManager.OpenShop(UI.ShopType.Props);
            // 장비 상점 오픈
            else
                _uiManager.shopManager.OpenShop(UI.ShopType.Equipments);
        }

        #region 퀘스트 관련 함수
        // 수락 가능한 퀘스트가 있는 지 확인
        private void HasNpcAcceptableQuest()
        {
            // NPC에게 Quest가 있는 경우
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

            // 퀘스트가 없다면 변수 할당 해제
            _selectedQuestName = null;
            button_Quest.gameObject.SetActive(false);
        }

        // 퀘스트 대화 버튼의 함수
        private void QuestButton()
        {
            // 수락 거절 버튼 숨김
            acceptQuestParent.SetActive(false);

            _scriptsIndex = 0;
            int questCount = _currentNPC.NpcQuests.Count;
            Quests.Quest selectedQuest;

            for (int i = 0; i < _currentNPC.NpcQuests.Count; i++)
            {
                selectedQuest = _currentNPC.NpcQuests[i];

                // 이미 NPC에게 받은 퀘스트를 받은 경우
                if (_questManager.IsActivated(selectedQuest))
                {
                    // 아직 완료하지 않은 경우
                    if (!selectedQuest.IsCompletable)
                        _targetDialogKey = _selectedQuestName + _string_Accept;
                    // 완료한 경우
                    else
                    {
                        // 퀘스트 완료 가능한 경우
                        if (UI.UIManager.Instance.questManager.CompleteQuest(selectedQuest))
                        {
                            // 보상 아이템 목록 표시
                            _uiManager.questManager.OpenRewardInfo(selectedQuest);

                            _targetDialogKey = _selectedQuestName + _string_Success;

                            // 수락 가능 퀘스트 확인
                            HasNpcAcceptableQuest();

                            // 사운드 효과
                            AudioManager.Instance.PlayAudio(Strings.Audio_UI_CompleteQuest);
                        }
                        // 보상 수락이 불가한 경우
                        else 
                            _targetDialogKey = Strings.Dialog_InventoryFull;
                    }
                    Scripting(dialogsDic[_targetDialogKey].scripts[_scriptsIndex++]);
                    return;
                }
                // 아직 NPC에게 퀘스트를 받지 않은 경우
                else
                {
                    _isQuestDialog = true;
                    // 퀘스트 관련 대화 스크립팅 시작
                    _targetDialogKey = _selectedQuestName;
                    Scripting(dialogsDic[_targetDialogKey].scripts[_scriptsIndex++]);

                    // 'R' 버튼의 이벤트 함수를 퀘스트 대화로 전환
                    _Input_ContinueDialogue.started -= ContinueDialogue;
                    _Input_ContinueDialogue.started += QuestDialogue;

                    // 퀘스트 정보 표시
                    DisplayQuestInfo();
                    break;
                }
            }
        }

        // 퀘스트 정보 표시 및 수락 버튼 표시
        private void DisplayQuestInfo()
        {
            // 퀘스트 정보 표시
            if (dialogsDic[_selectedQuestName].scripts.Count <= _scriptsIndex)
            {
                _questManager.OpenQuestInfo();

                // 수락 거절 버튼 표시
                acceptQuestParent.SetActive(true);
            }
        }

        // 퀘스트 수락 버튼의 함수
        private void AcceptQuest()
        {
            _isQuestDialog = false;
            _scriptsIndex = 0;
            acceptQuestParent.SetActive(false);

            // 수락에 대한 대화 표시
            _targetDialogKey = _selectedQuestName + _string_Accept;
            Scripting(dialogsDic[_targetDialogKey].scripts[_scriptsIndex++]);

            // 'R' 버튼의 이벤트 함수를 일반 대화로 전환
            _Input_ContinueDialogue.started += ContinueDialogue;
            _Input_ContinueDialogue.started -= QuestDialogue;

            // 해당 퀘스트를 활성화
            _questManager.AddNewQuest();

            // 사운드 효과
            AudioManager.Instance.PlayAudio(Strings.Audio_UI_QuestAccept);
        }

        // 퀘스트 거절 버튼의 함수
        private void DeclineQuest()
        {
            _isQuestDialog = false;
            _scriptsIndex = 0;
            acceptQuestParent.SetActive(false);

            // 거절에 대한 대화 표시
            _targetDialogKey = _selectedQuestName + _string_Decline;
            Scripting(dialogsDic[_targetDialogKey].scripts[_scriptsIndex++]);

            // 'R' 버튼의 이벤트 함수를 일반 대화로 전환
            _Input_ContinueDialogue.started += ContinueDialogue;
            _Input_ContinueDialogue.started -= QuestDialogue;
        }
        #endregion
        #endregion

        #region Input 이벤트 함수
        // 대화를 시작
        private void StartConversation(InputAction.CallbackContext context)
        {
            if (_currentNpcCodeName == null) return;

            // 대화 창 열기 전에 모든 창 닫기
            _uiManager.CloseAllWindows(true);

            // 수락 거절 버튼 숨김
            acceptQuestParent.SetActive(false);

            // 인풋 모드를 대화로 변경
            _inputManager.SwitchInputMode(InputMode.Conversation);

            // 씬매니저를 통해 NPC 정보를 가져옴
            _currentNPC = SceneManager.Instance.GetNPC(_currentNpcCodeName);

            // NPC 이름 할당
            if (_currentNPC) text_NPCName.text = _currentNPC.DisplayName;
            else text_NPCName.text = string.Empty;

            // 인터렉팅 카메라 켜짐
            _cameraManager.ChangeInteractingCamera(true, _currentNPC);

            // NPC에게 대화 시작을 알리는 함수 호출
            _currentNPC.StartConversation();

            // 버튼 초기화
            button_Shop.gameObject.SetActive(false);

            // 수락 가능 퀘스트 확인
            HasNpcAcceptableQuest();

            // 상점 이용 가능 NPC인 경우 상점 이용 버튼 표시
            if (_currentNPC.NpcType == NPC.NPCType.PropShop ||
                _currentNPC.NpcType == NPC.NPCType.EquipmentShop)
                button_Shop.gameObject.SetActive(true);

            // 대화 창 열기
            window_Dialog.alpha = 1;
            window_Dialog.blocksRaycasts = true;

            // 해당 대화 스크립팅하는 함수 호출
            _targetDialogKey = _currentNpcCodeName;
            Scripting(dialogsDic[_targetDialogKey].scripts[_scriptsIndex++]);
        }

        // 다음 대화를 표시
        private void ContinueDialogue(InputAction.CallbackContext context)
        {
            _targetDialogKey = _currentNpcCodeName;
            ContinueDialogue(); 
        }

        // 퀘스트 관련 대화 표시
        private void QuestDialogue(InputAction.CallbackContext context)
        {
            // 선택된 퀘스트이름을 타겟 대화 키로 저장
            _targetDialogKey = _selectedQuestName;
            ContinueDialogue();

            // 퀘스트 정보 표시
            DisplayQuestInfo();
        }

        // 대화 창을 닫음
        private void CloseDialogue(InputAction.CallbackContext context)
            => CloseButton();
        #endregion

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
            SceneManager.Instance.OnFixedUpdate += FixedTick;
        }

        private void FixedTick()
        {
            // 일정 간격으로 대화 스트링 값을 추가하며 출력
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

                // 대화 스크립트를 모두 출력한 경우
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
