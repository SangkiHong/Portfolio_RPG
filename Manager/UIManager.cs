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

        [Header("Canvas Group")]
        public CanvasGroup window_CharacterStatus;
        public CanvasGroup window_Invenroty;
        public CanvasGroup window_Quest;
        public CanvasGroup window_Dialog;

        private PlayerInput _playerInput;
        private List<CanvasGroup> _windowsList; // �����츦 �����ϱ� ���� ĵ�����׷� ����Ʈ

        private void Start()
        {
            GameManager.Instance.UIManager = this;
            _playerInput = GameManager.Instance.InputManager.playerInput;

            // ĵ�����׷� ����Ʈ �ʱ�ȭ
            _windowsList = new List<CanvasGroup>();
            _windowsList.Add(window_CharacterStatus);
            _windowsList.Add(window_Invenroty);
            _windowsList.Add(window_Quest);
            _windowsList.Add(window_Dialog);

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
            }
            // â�� �����ִ� ��� â�� ����
            else
            {
                targetWindow.alpha = 0;
                targetWindow.blocksRaycasts = false;

                // ��� â�� �������� �˻��ϴ� �Լ�
                CheckAllWindowsClosed();
            }
        }

        // ������ ��� â, �г� ����_220511
        public void CloseAllWindows()
        {
            foreach (var window in _windowsList)
            {
                window.alpha = 0;
                window.blocksRaycasts = false;
            }

            inventoryManager.CancelEraseMode();
            inventoryManager.itemSpecificsPanel.gameObject.SetActive(false);

            // ���콺 ��带 ����
            GameManager.Instance.InputManager.SwitchInputMode(InputMode.GamePlay);
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
            dialogManager.Initialize();
        }
    }
}
