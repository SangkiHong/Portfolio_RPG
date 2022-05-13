using UnityEngine;
using System.Collections;

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

            // �ʱ�ȭ
            StartCoroutine(InitializeManagers());
            CloseAllWindows();

            // ��ǲ �ý��� Ʈ���ſ� �̺�Ʈ �Լ� �Ҵ�
            _inputActions.GamePlay.UI_CharacterStats.performed += x => { SwitchWindow(window_CharacterStatus); };
            _inputActions.GamePlay.UI_Inventory.performed += x => { SwitchWindow(window_Invenroty); };
            _inputActions.GamePlay.UI_CloseAll.performed += x => { CloseAllWindows(); };
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
            }
        }

        // ������ ��� â, �г� ����_220511
        private void CloseAllWindows()
        {
            window_CharacterStatus.alpha = 0;
            window_CharacterStatus.blocksRaycasts = false;
            window_Invenroty.alpha = 0;
            window_Invenroty.blocksRaycasts = false;

            inventoryManager.CancelEraseMode();
            inventoryManager.itemSpecificsPanel.gameObject.SetActive(false);
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
        }
    }
}
