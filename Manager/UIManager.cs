using UnityEngine;
using UnityEngine.UI;

namespace SK.UI
{
    public class UIManager : MonoBehaviour
    {
        [Header("Reference")]
        [SerializeField] internal CharacterStatusManager characterStatusManager;
        [SerializeField] internal InventoryManager inventoryManager;
        [SerializeField] internal EquipSlotManager equipSlotManager;

        [Header("Canvas Group")]
        public CanvasGroup window_CharacterStatus;
        public CanvasGroup window_Invenroty;

        private PlayerInputAction _inputActions;

        private void Start()
        {
            _inputActions = GameManager.Instance.InputManager.playerInputAction;

            // 초기화
            CloseAllWindows();

            _inputActions.GamePlay.UI_CharacterStats.performed += x => { SwitchWindow(window_CharacterStatus); };
            _inputActions.GamePlay.UI_Inventory.performed += x => { SwitchWindow(window_Invenroty); };
            _inputActions.GamePlay.UI_CloseAll.performed += x => { CloseAllWindows(); };
        }

        // 창 열고 닫는 함수_220511
        private void SwitchWindow(CanvasGroup targetWindow)
        {
            // 창이 닫혀 있는 경우 창을 열음
            if (targetWindow.alpha == 0)
            {
                targetWindow.alpha = 1;
                targetWindow.blocksRaycasts = true;
            }
            // 창이 열려있는 경우 창을 닫음
            else
            {
                targetWindow.alpha = 0;
                targetWindow.blocksRaycasts = false;
            }
        }

        private void CloseAllWindows()
        {
            window_CharacterStatus.alpha = 0;
            window_CharacterStatus.blocksRaycasts = false;
            window_Invenroty.alpha = 0;
            window_Invenroty.blocksRaycasts = false;

            inventoryManager.CancelEraseMode();
            inventoryManager.itemSpecificsPanel.gameObject.SetActive(false);
        }
    }
}
