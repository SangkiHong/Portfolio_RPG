using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using SK.Dialog;
using SK.Quests;

namespace SK.UI
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("Reference")]
        public QuickSlotManager quickSlotManager;
        public CharacterStatusManager characterStatusManager;
        public InventoryManager inventoryManager;
        public EquipSlotManager equipSlotManager;
        public QuestManager questManager;
        public DialogManager dialogManager;
        public ShopManager shopManager;
        public SkillManager skillManager;
        public CurrencySyncHandler currencySyncHandler;
        public PlayerStateUIHandler playerStateUIHandler;
        public EnemyHpStateUIHandler enemyHpStateUIHandler;
        public RespawnMenuHandler respawnMenuHandler;
        public ConfirmPanel confirmPanel;
        // 데미지 범위 표시 데칼 UI
        public DamageRangeHandler damageRangeHandler;

        [Header("Canvas Group")]
        public CanvasGroup window_CharacterStatus;
        public CanvasGroup window_Invenroty;
        public CanvasGroup window_Quest;
        public CanvasGroup window_Dialog;
        public CanvasGroup window_Shop;
        public CanvasGroup window_Skill;

        [Header("UI")]
        [SerializeField] private Image blackCover;
        [SerializeField] internal float changeDuration;

        [Header("Sprite Resource")]
        public Sprite sprite_Exp;
        public Sprite sprite_Gold;
        public Sprite sprite_Gem;

        // 데미지 표시 UI 관리자 클래스
        public DamagePointUIManager damagePointUIManager;

        private PlayerInput _playerInput;
        private List<CanvasGroup> _windowsList; // 윈도우를 관리하기 위한 캔버스그룹 리스트
        private GameObject _characterUICamera;
        private Color _blackCoverColor;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(this);
            }
            else
                Destroy(gameObject);
        }

        private void Start()
        {
            // 레퍼런스 초기화
            Instance = this;
            GameManager.Instance.DataManager.InitializeScene();
            _playerInput = GameManager.Instance.InputManager.playerInput;
            _blackCoverColor = blackCover.color;

            // 클래스 생성
            damagePointUIManager = new DamagePointUIManager();

            // 캔버스그룹 리스트 초기화
            _windowsList = new List<CanvasGroup>
            {
                window_CharacterStatus,
                window_Invenroty,
                window_Quest,
                window_Dialog,
                window_Shop,
                window_Skill
            };

            // UI 관련 매니저 초기화 함수 호출
            StartCoroutine(InitializeManagers());

            // 인풋 시스템 이벤트를 통해 Window UI 제어
            _playerInput.actions["UI_CharacterStats"].started += x => { 
                SwitchWindow(window_CharacterStatus);
                _characterUICamera.SetActive(window_CharacterStatus.blocksRaycasts);
            };
            _playerInput.actions["UI_Inventory"].started += x => { SwitchWindow(window_Invenroty); };
            _playerInput.actions["UI_Quest"].started += x => { SwitchWindow(window_Quest); };
            _playerInput.actions["UI_Skill"].started += x => { SwitchWindow(window_Skill); };
            _playerInput.actions["UI_CloseAllWindows"].started += x => { CloseAllWindows(); };

            // 검은 화면에서 밝아지는 효과 재생
            BlackScreenControl(false);

            Debug.Log("UI Manager 초기화 완료");
        }

        public void UnassignInputActions()
        {
            _playerInput.actions["UI_CharacterStats"].started -= x => {
                SwitchWindow(window_CharacterStatus);
                _characterUICamera.SetActive(window_CharacterStatus.blocksRaycasts);
            };
            _playerInput.actions["UI_Inventory"].started -= x => { SwitchWindow(window_Invenroty); };
            _playerInput.actions["UI_Quest"].started -= x => { SwitchWindow(window_Quest); };
            _playerInput.actions["UI_Skill"].started -= x => { SwitchWindow(window_Skill); };
            _playerInput.actions["UI_CloseAllWindows"].started -= x => { CloseAllWindows(); };

            Debug.Log("UI Manager 이벤트 해제 완료");
        }

        // 창 열고 닫는 함수_220511
        private void SwitchWindow(CanvasGroup targetWindow)
        {
            // 창이 닫혀 있는 경우 창을 열음
            if (targetWindow.alpha == 0)
            {
                targetWindow.alpha = 1;
                targetWindow.blocksRaycasts = true;

                // 최근 열은 창이 가장 앞에 보일 수 있도록 Sibling 순서를 제일 아래 둠
                VisibleWindowAtFront(targetWindow);
                
                GameManager.Instance.InputManager.SwitchInputMode(InputMode.UI);
            }
            // 창이 열려있는 경우 창을 닫음
            else
            {
                targetWindow.alpha = 0;
                targetWindow.blocksRaycasts = false;

                // 인벤토리 창과 케릭터 창이 모두 닫혀진 경우 아이템 세부 정보 창도 함께 닫음
                if (!window_Invenroty.blocksRaycasts && !window_CharacterStatus.blocksRaycasts)
                    inventoryManager.itemSpecificsPanel.Close();

                // 퀘스트 창이 닫혀지는 경우 퀘스트 세부 정보도 닫음
                if (!window_Quest.blocksRaycasts)
                    questManager.ClosePanel();

                // 모든 창이 닫혔는지 검사하는 함수
                CheckAllWindowsClosed();
            }
        }

        // 인자로 받은 캔버스그룹이 가장 앞에 보일 수 있도록 Sibling 순서를 제일 아래 둠
        public void VisibleWindowAtFront(CanvasGroup targetWindow)
            => targetWindow.transform.SetAsLastSibling();

        // 열려진 모든 창, 패널 닫음_220511
        public bool CloseAllWindows(bool forceClose = false)
        {
            if (inventoryManager.CloseAllPanel() && !forceClose) return false;
            if (questManager.ClosePanel() && !forceClose) return false;
            if (shopManager.ClosePanelAndWindow() && !forceClose) return false;
            inventoryManager.itemSpecificsPanel.Close();

            foreach (var window in _windowsList)
            {
                window.alpha = 0;
                window.blocksRaycasts = false;
            }

            // 마우스 모드를 해제
            GameManager.Instance.InputManager.SwitchInputMode(InputMode.GamePlay);
            return true;
        }

        // 모든 창이 닫혔는지 확인 후 Action Map을 GamePlay로 변경_220525
        // 창 닫기 버튼의 이벤트에도 등록하여 사용
        public void CheckAllWindowsClosed()
        {
            // 아이템 세부 정보 창 닫기
            if (inventoryManager.itemSpecificsPanel.IsOpen)
                inventoryManager.itemSpecificsPanel.Close();

            // 하나라도 창이 열려있는 경우 return
            for (int i = 0; i < _windowsList.Count; i++)
                if (_windowsList[i].blocksRaycasts)
                    return;

            // 모든 창이 닫혀있는 경우 Action Map 변경
            GameManager.Instance.InputManager.SwitchInputMode(InputMode.GamePlay);
        }

        // 검은 화면 컨트롤
        public void BlackScreenControl(bool isOnBlack)
        {
            StartCoroutine(CoveringBlack(isOnBlack));
        }

        // 즉시 검은 화면 상태 변경
        public void SetScreenBlack(bool isOnBlack)
        {
            blackCover.gameObject.SetActive(true);
            if (isOnBlack) _blackCoverColor.a = 1;
            else _blackCoverColor.a = 0;

            blackCover.color = _blackCoverColor;
        }

        IEnumerator InitializeManagers()
        {
            var ws = new WaitForEndOfFrame();

            // 플레이어에 접근 가능한 상태까지 대기
            while (GameManager.Instance.Player == null)
                yield return ws;

            // 매니저 정보 초기화
            quickSlotManager.Initialize(_playerInput);
            characterStatusManager.Initialize();
            inventoryManager.Initialize();
            equipSlotManager.Initialize();
            questManager.Initialize();
            shopManager.Initialize();
            dialogManager.Initialize(questManager);
            skillManager.Initialize();
            currencySyncHandler.Initialize();
            playerStateUIHandler.Initialize(Data.DataManager.Instance.PlayerData);

            // 캐릭터 UI 카메라 가져옴
            _characterUICamera = GameObject.FindGameObjectWithTag("CharacterUICamera");
            _characterUICamera.SetActive(false);

            CloseAllWindows(true);
        }

        IEnumerator CoveringBlack(bool isOnBlack)
        {
            // 커버 이미지 초기화
            blackCover.gameObject.SetActive(true);
            if (isOnBlack) _blackCoverColor.a = 0;
            else _blackCoverColor.a = 1;
            blackCover.color = _blackCoverColor;

            float elapsed = 0;
            float updateTime = 0.05f;

            WaitForSeconds ws = new WaitForSeconds(updateTime);

            while (changeDuration > elapsed)
            {
                elapsed += updateTime;

                if (isOnBlack) _blackCoverColor.a += updateTime;
                else _blackCoverColor.a -= updateTime;

                blackCover.color = _blackCoverColor;
                yield return ws;
            }
            blackCover.gameObject.SetActive(false);

            yield return null;
        }
    }
}
