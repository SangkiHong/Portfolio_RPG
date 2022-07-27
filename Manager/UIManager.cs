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
        // ������ ���� ǥ�� ��Į UI
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

        // ������ ǥ�� UI ������ Ŭ����
        public DamagePointUIManager damagePointUIManager;

        private PlayerInput _playerInput;
        private List<CanvasGroup> _windowsList; // �����츦 �����ϱ� ���� ĵ�����׷� ����Ʈ
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
            // ���۷��� �ʱ�ȭ
            Instance = this;
            GameManager.Instance.DataManager.InitializeScene();
            _playerInput = GameManager.Instance.InputManager.playerInput;
            _blackCoverColor = blackCover.color;

            // Ŭ���� ����
            damagePointUIManager = new DamagePointUIManager();

            // ĵ�����׷� ����Ʈ �ʱ�ȭ
            _windowsList = new List<CanvasGroup>
            {
                window_CharacterStatus,
                window_Invenroty,
                window_Quest,
                window_Dialog,
                window_Shop,
                window_Skill
            };

            // UI ���� �Ŵ��� �ʱ�ȭ �Լ� ȣ��
            StartCoroutine(InitializeManagers());

            // ��ǲ �ý��� �̺�Ʈ�� ���� Window UI ����
            _playerInput.actions["UI_CharacterStats"].started += x => { 
                SwitchWindow(window_CharacterStatus);
                _characterUICamera.SetActive(window_CharacterStatus.blocksRaycasts);
            };
            _playerInput.actions["UI_Inventory"].started += x => { SwitchWindow(window_Invenroty); };
            _playerInput.actions["UI_Quest"].started += x => { SwitchWindow(window_Quest); };
            _playerInput.actions["UI_Skill"].started += x => { SwitchWindow(window_Skill); };
            _playerInput.actions["UI_CloseAllWindows"].started += x => { CloseAllWindows(); };

            // ���� ȭ�鿡�� ������� ȿ�� ���
            BlackScreenControl(false);

            Debug.Log("UI Manager �ʱ�ȭ �Ϸ�");
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

            Debug.Log("UI Manager �̺�Ʈ ���� �Ϸ�");
        }

        // â ���� �ݴ� �Լ�_220511
        private void SwitchWindow(CanvasGroup targetWindow)
        {
            // â�� ���� �ִ� ��� â�� ����
            if (targetWindow.alpha == 0)
            {
                targetWindow.alpha = 1;
                targetWindow.blocksRaycasts = true;

                // �ֱ� ���� â�� ���� �տ� ���� �� �ֵ��� Sibling ������ ���� �Ʒ� ��
                VisibleWindowAtFront(targetWindow);
                
                GameManager.Instance.InputManager.SwitchInputMode(InputMode.UI);
            }
            // â�� �����ִ� ��� â�� ����
            else
            {
                targetWindow.alpha = 0;
                targetWindow.blocksRaycasts = false;

                // �κ��丮 â�� �ɸ��� â�� ��� ������ ��� ������ ���� ���� â�� �Բ� ����
                if (!window_Invenroty.blocksRaycasts && !window_CharacterStatus.blocksRaycasts)
                    inventoryManager.itemSpecificsPanel.Close();

                // ����Ʈ â�� �������� ��� ����Ʈ ���� ������ ����
                if (!window_Quest.blocksRaycasts)
                    questManager.ClosePanel();

                // ��� â�� �������� �˻��ϴ� �Լ�
                CheckAllWindowsClosed();
            }
        }

        // ���ڷ� ���� ĵ�����׷��� ���� �տ� ���� �� �ֵ��� Sibling ������ ���� �Ʒ� ��
        public void VisibleWindowAtFront(CanvasGroup targetWindow)
            => targetWindow.transform.SetAsLastSibling();

        // ������ ��� â, �г� ����_220511
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

            // ���콺 ��带 ����
            GameManager.Instance.InputManager.SwitchInputMode(InputMode.GamePlay);
            return true;
        }

        // ��� â�� �������� Ȯ�� �� Action Map�� GamePlay�� ����_220525
        // â �ݱ� ��ư�� �̺�Ʈ���� ����Ͽ� ���
        public void CheckAllWindowsClosed()
        {
            // ������ ���� ���� â �ݱ�
            if (inventoryManager.itemSpecificsPanel.IsOpen)
                inventoryManager.itemSpecificsPanel.Close();

            // �ϳ��� â�� �����ִ� ��� return
            for (int i = 0; i < _windowsList.Count; i++)
                if (_windowsList[i].blocksRaycasts)
                    return;

            // ��� â�� �����ִ� ��� Action Map ����
            GameManager.Instance.InputManager.SwitchInputMode(InputMode.GamePlay);
        }

        // ���� ȭ�� ��Ʈ��
        public void BlackScreenControl(bool isOnBlack)
        {
            StartCoroutine(CoveringBlack(isOnBlack));
        }

        // ��� ���� ȭ�� ���� ����
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

            // �÷��̾ ���� ������ ���±��� ���
            while (GameManager.Instance.Player == null)
                yield return ws;

            // �Ŵ��� ���� �ʱ�ȭ
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

            // ĳ���� UI ī�޶� ������
            _characterUICamera = GameObject.FindGameObjectWithTag("CharacterUICamera");
            _characterUICamera.SetActive(false);

            CloseAllWindows(true);
        }

        IEnumerator CoveringBlack(bool isOnBlack)
        {
            // Ŀ�� �̹��� �ʱ�ȭ
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
