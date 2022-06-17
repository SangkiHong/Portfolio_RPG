using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using SK.Dialog;

namespace SK.UI
{
    public class UIManager : MonoBehaviour
    {
        [Header("Reference")]
        [SerializeField] internal CharacterStatusManager characterStatusManager;
        [SerializeField] internal InventoryManager inventoryManager;
        [SerializeField] internal EquipSlotManager equipSlotManager;
        [SerializeField] internal QuestManager questManager;
        [SerializeField] internal DialogManager dialogManager;
        [SerializeField] internal ShopManager shopManager;
        [SerializeField] internal CurrencySyncHandler currencySyncHandler;

        [Header("Canvas Group")]
        public CanvasGroup window_CharacterStatus;
        public CanvasGroup window_Invenroty;
        public CanvasGroup window_Quest;
        public CanvasGroup window_Dialog;
        public CanvasGroup window_Shop;

        private PlayerInput _playerInput;
        [SerializeField] private List<CanvasGroup> _windowsList; // �����츦 �����ϱ� ���� ĵ�����׷� ����Ʈ

        private void Start()
        {
            // ���۷��� �ʱ�ȭ
            GameManager.Instance.UIManager = this;
            GameManager.Instance.DataManager.InitializeScene();
            _playerInput = GameManager.Instance.InputManager.playerInput;

            // ĵ�����׷� ����Ʈ �ʱ�ȭ
            _windowsList = new List<CanvasGroup>();
            _windowsList.Add(window_CharacterStatus);
            _windowsList.Add(window_Invenroty);
            _windowsList.Add(window_Quest);
            _windowsList.Add(window_Dialog);
            _windowsList.Add(window_Shop);

            // �ʱ�ȭ
            StartCoroutine(InitializeManagers());
            CloseAllWindows();

            // ��ǲ �ý��� �̺�Ʈ�� ���� Window UI ����
            _playerInput.actions["GamePlay_CharacterStats"].started += x => { SwitchWindow(window_CharacterStatus); };
            _playerInput.actions["UI_CharacterStats"].started += x => { SwitchWindow(window_CharacterStatus); };
            _playerInput.actions["GamePlay_Inventory"].started += x => { SwitchWindow(window_Invenroty); }; 
            _playerInput.actions["UI_Inventory"].started += x => { SwitchWindow(window_Invenroty); };
            _playerInput.actions["GamePlay_Quest"].started += x => { SwitchWindow(window_Quest); };
            _playerInput.actions["UI_Quest"].started += x => { SwitchWindow(window_Quest); };
            _playerInput.actions["GamePlay_CloseAll"].started += x => { CloseAllWindows(); };
            _playerInput.actions["UI_CloseAllWindows"].started += x => { CloseAllWindows(); };

            Debug.Log("UI Manager �ʱ�ȭ �Ϸ�");
        }

        public void UnassignInputActions()
        {
            _playerInput.actions["GamePlay_CharacterStats"].started -= x => { SwitchWindow(window_CharacterStatus); };
            _playerInput.actions["UI_CharacterStats"].started -= x => { SwitchWindow(window_CharacterStatus); };
            _playerInput.actions["GamePlay_Inventory"].started -= x => { SwitchWindow(window_Invenroty); };
            _playerInput.actions["UI_Inventory"].started -= x => { SwitchWindow(window_Invenroty); };
            _playerInput.actions["GamePlay_Quest"].started -= x => { SwitchWindow(window_Quest); };
            _playerInput.actions["UI_Quest"].started -= x => { SwitchWindow(window_Quest); };
            _playerInput.actions["GamePlay_CloseAll"].started -= x => { CloseAllWindows(); };
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
                    inventoryManager.itemSpecificsPanel.gameObject.SetActive(false);

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
            // �ϳ��� â�� �����ִ� ��� return
            for (int i = 0; i < _windowsList.Count; i++)
                if (_windowsList[i].blocksRaycasts)
                    return;

            // ��� â�� �����ִ� ��� Action Map ����
            GameManager.Instance.InputManager.SwitchInputMode(InputMode.GamePlay);
        }

        private IEnumerator InitializeManagers()
        {
            var ws = new WaitForEndOfFrame();

            // �÷��̾ ������ �� ���� ������ ��ٸ�
            while (GameManager.Instance.Player == null)            
                yield return ws;
            
            // ��� ���� �ʱ�ȭ
            characterStatusManager.Initialize();
            inventoryManager.Initialize();
            equipSlotManager.Initialize();
            questManager.Initialize();
            shopManager.Initialize();
            dialogManager.Initialize(questManager);
            currencySyncHandler.Initialize();
        }
    }
}
