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
        [SerializeField] private List<CanvasGroup> _windowsList; // 윈도우를 관리하기 위한 캔버스그룹 리스트

        private void Start()
        {
            // 레퍼런스 초기화
            GameManager.Instance.UIManager = this;
            GameManager.Instance.DataManager.InitializeScene();
            _playerInput = GameManager.Instance.InputManager.playerInput;

            // 캔버스그룹 리스트 초기화
            _windowsList = new List<CanvasGroup>();
            _windowsList.Add(window_CharacterStatus);
            _windowsList.Add(window_Invenroty);
            _windowsList.Add(window_Quest);
            _windowsList.Add(window_Dialog);
            _windowsList.Add(window_Shop);

            // 초기화
            StartCoroutine(InitializeManagers());
            CloseAllWindows();

            // 인풋 시스템 이벤트를 통해 Window UI 제어
            _playerInput.actions["GamePlay_CharacterStats"].started += x => { SwitchWindow(window_CharacterStatus); };
            _playerInput.actions["UI_CharacterStats"].started += x => { SwitchWindow(window_CharacterStatus); };
            _playerInput.actions["GamePlay_Inventory"].started += x => { SwitchWindow(window_Invenroty); }; 
            _playerInput.actions["UI_Inventory"].started += x => { SwitchWindow(window_Invenroty); };
            _playerInput.actions["GamePlay_Quest"].started += x => { SwitchWindow(window_Quest); };
            _playerInput.actions["UI_Quest"].started += x => { SwitchWindow(window_Quest); };
            _playerInput.actions["GamePlay_CloseAll"].started += x => { CloseAllWindows(); };
            _playerInput.actions["UI_CloseAllWindows"].started += x => { CloseAllWindows(); };

            Debug.Log("UI Manager 초기화 완료");
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
                    inventoryManager.itemSpecificsPanel.gameObject.SetActive(false);

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
            // 하나라도 창이 열려있는 경우 return
            for (int i = 0; i < _windowsList.Count; i++)
                if (_windowsList[i].blocksRaycasts)
                    return;

            // 모든 창이 닫혀있는 경우 Action Map 변경
            GameManager.Instance.InputManager.SwitchInputMode(InputMode.GamePlay);
        }

        private IEnumerator InitializeManagers()
        {
            var ws = new WaitForEndOfFrame();

            // 플레이어에 접근할 수 있을 때까지 기다림
            while (GameManager.Instance.Player == null)            
                yield return ws;
            
            // 모든 정보 초기화
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
