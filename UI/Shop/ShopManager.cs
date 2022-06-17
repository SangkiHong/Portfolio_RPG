using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace SK.UI
{
    /* �ۼ���: ȫ���
     * ����: ������ ���� �������� ����� ������ Ŭ����
     * �ۼ���: 22�� 6�� 14��
     */

    // ���� Ÿ��
    public enum ShopType
    {
        None,
        Props,
        Equipments
    }

    public class ShopManager : MonoBehaviour
    {
        [Header("Reference")]
        [SerializeField] private AmountInputPanel inputPanel;
        [SerializeField] private ShopTabHandler tabHandler;

        [Header("Component")]
        [SerializeField] private CanvasGroup shopWindow;
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private Transform slotParent;
        [SerializeField] private GameObject sellArea;

        [Header("Prefab")]
        [SerializeField] private GameObject slotPrefab;

        [Header("UI Setting")]
        [SerializeField] private Color highlightColor;

        public int ShopMode { get; private set; } // 0: ����, 1: �Ǹ�

        private List<ShopItemSlot> shopItemSlots;
        private List<Item> _propsItemList;
        private List<Item> _equipmentsItemList;

        private ShopType _currentShopType;

        private Item _selectedItem;
        private ShopItemSlot _selectedSlot;
        private InventorySlot _sellItemSlot;

        private CanvasGroup _inventoryWindow;
        private RectTransform _sellAreaRT;

        private int _propsItemCount, _equipmentsItemCount;
        private int _addSlotIndex, _selectedSlotIndex = -1;

        private void OnDisable()
        {
            // �̺�Ʈ �Ҵ� ����
            for (int i = 0; i < shopItemSlots.Count; i++)
            {
                shopItemSlots[i].OnSelectSlotEvent -= OnSelectSlot;
                shopItemSlots[i].OnBeginDragEvent -= OnDragBegin;
                shopItemSlots[i].OnDragEvent -= OnDrag;
                shopItemSlots[i].OnDragEndEvent -= OnDragEnd;
            }
        }

        private void Awake()
        {
            _sellAreaRT = sellArea.transform as RectTransform;
        }

        // ���� ������ ���� �ʱ�ȭ
        public void Initialize()
        {
            // ����Ʈ �ʱ�ȭ
            if (_propsItemList == null) _propsItemList = new List<Item>();
            if (_equipmentsItemList == null) _equipmentsItemList = new List<Item>();

            // ��ȭ ������ �Ǹ� ������ ����Ʈ �����͸� �����ͼ� ����Ʈ�� ����
            GameManager.Instance.DataManager.GetShopList(0, ref _propsItemList);
            // ��� ������ �Ǹ� ������ ����Ʈ �����͸� �����ͼ� ����Ʈ�� ����
            GameManager.Instance.DataManager.GetShopList(1, ref _equipmentsItemList);

            _currentShopType = ShopType.None;

            // �κ��丮 â ���� ����
            _inventoryWindow = GameManager.Instance.UIManager.window_Invenroty;

            // ���� �ʱ�ȭ
            var slotArr = slotParent.GetComponentsInChildren<ShopItemSlot>();
            shopItemSlots = new List<ShopItemSlot>();
            for (int i = 0; i < slotArr.Length; i++)
            {
                // ���� �ʱ�ȭ
                slotArr[i].Initialize(highlightColor);
                slotArr[i].SetSlotID(_addSlotIndex++);
                // �̺�Ʈ �Ҵ�
                slotArr[i].OnSelectSlotEvent += OnSelectSlot;
                slotArr[i].OnBeginDragEvent += OnDragBegin;
                slotArr[i].OnDragEvent += OnDrag;
                slotArr[i].OnDragEndEvent += OnDragEnd;
                // ����Ʈ�� ������ �߰�
                shopItemSlots.Add(slotArr[i]);
            }

            // �̸� ��ġ�� ���Ժ��� �Ǹ� ������ ����Ʈ ������ ������ Ȯ��
            _propsItemCount = _propsItemList.Count;
            _equipmentsItemCount = _equipmentsItemList.Count;
            int maxCount = _propsItemCount > _equipmentsItemCount ? _propsItemCount : _equipmentsItemCount;
            int needCount = maxCount - shopItemSlots.Count;

            // ������ ������ ������ ����
            if (needCount > 0)
            {
                for (int i = 0; i < needCount; i++)
                {
                    // ������ ������ŭ ���� ������ ���ÿ� ���� ������Ʈ�� ����Ʈ�� �߰�
                    ShopItemSlot tempSlot = Instantiate(slotPrefab, slotParent).GetComponent<ShopItemSlot>();
                    // ���� �ʱ�ȭ
                    tempSlot.Initialize(highlightColor);
                    tempSlot.SetSlotID(_addSlotIndex++);
                    // �̺�Ʈ �Ҵ�
                    tempSlot.OnSelectSlotEvent += OnSelectSlot;
                    tempSlot.OnBeginDragEvent += OnDragBegin;
                    tempSlot.OnDragEvent += OnDrag;
                    tempSlot.OnDragEndEvent += OnDragEnd;
                    shopItemSlots.Add(tempSlot);
                }
            }

            // �� ��ư �̺�Ʈ �Լ� �Ҵ�
            tabHandler.OnChanged += ChangeTab;
        }

        // ���� ���� â, �г� UI ����
        public bool ClosePanelAndWindow()
        {
            if (inputPanel.gameObject.activeSelf)
            {
                shopItemSlots[_selectedSlotIndex].Highlight(false);
                inputPanel.gameObject.SetActive(false);
                inputPanel.OnConfirmAmount -= ConfirmAmount;
                return true;
            }
            else if (shopWindow.blocksRaycasts)
            {
                // ���� â ����
                shopWindow.alpha = 0;
                shopWindow.blocksRaycasts = false;

                // �κ��丮 â�� �Բ� ����
                _inventoryWindow.alpha = 0;
                _inventoryWindow.blocksRaycasts = false;
                    
                // ���� ��� �ʱ�ȭ
                tabHandler.Reset();
                return true;
            }
            else
                return false;
        }

        // ���� â ���� �Լ�
        public void OpenShop(ShopType shopType)
        {
            shopWindow.alpha = 1f;
            shopWindow.blocksRaycasts = true;

            // ���� ���� �ε��� �ʱ�ȭ
            _selectedSlotIndex = -1;

            // �κ��丮 â�� �Բ� ����
            _inventoryWindow.alpha = 1;
            _inventoryWindow.blocksRaycasts = true;

            // ���� ��� �ʱ�ȭ
            if (ShopMode != 0)
                tabHandler.Reset();

            // ������ ������ ������ �����ϸ� ���� UI ������Ʈ�� ���� ����
            if (_currentShopType == shopType) 
                return;
            else
            {
                // ������ ���� ������ ���� Ÿ���� ����
                _currentShopType = shopType;

                // ���� �ʱ�ȭ
                foreach (var slot in shopItemSlots)
                    slot.Unassign();

                // ��ȭ ������ ���
                if (shopType == ShopType.Props)
                {
                    // ���Կ� ��ȭ ���� �Ǹ� �������� �Ҵ�
                    for (int i = 0; i < _propsItemCount; i++)
                        shopItemSlots[i].Assign(_propsItemList[i]);
                }
                // ��� ������ ���
                else
                {
                    // ���Կ� ��� ���� �Ǹ� �������� �Ҵ�
                    for (int i = 0; i < _equipmentsItemCount; i++)
                        shopItemSlots[i].Assign(_equipmentsItemList[i]);
                }
            }
        }

        // ����, �Ǹ� ���� Ȯ��_220615
        private void ConfirmAmount(uint amount)
        {
            // ������ 0 �̻��� ���
            if (amount != 0)
            {
                // ���� ���
                if (ShopMode == 0)
                {
                    Debug.Log($"���� ������: {shopItemSlots[_selectedSlotIndex].AssignedItem.ItemName}, ����: {amount}");
                }
                // �Ǹ� ���
                else
                {
                    Debug.Log($"������ �Ǹ�: {_sellItemSlot.AssignedItem.ItemName}, ����: {amount}");
                    // TODO: �Ǹ� Ȯ�� �ȳ� UI ǥ��

                }
            }
            else
            {
                Debug.Log("���� �Ǵ� �Ǹ� ���");
            }
            if (_selectedSlotIndex != -1)
                shopItemSlots[_selectedSlotIndex].Highlight(false);
        }

        // ���� �Ǹ� �� ��ȯ
        public void ChangeTab(int index)
        {
            ShopMode = index;

            // ���� ���
            if (ShopMode == 0)
            {
                scrollRect.gameObject.SetActive(true);
                sellArea.SetActive(false);
            }
            // �Ǹ� ���
            else
            {
                scrollRect.gameObject.SetActive(false);
                sellArea.SetActive(true);
            }
        }

        // �Ǹ��� �������� ��������� ����߷ȴ��� Ȯ���ϴ� �Լ�_220616
        public void CheckSellItemDropArea(PointerEventData eventData, InventorySlot itemSlot)
        {
            var dropPos = eventData.position;

            var rectPos = _sellAreaRT.position;
            var width = _sellAreaRT.rect.width * 0.5f;
            var height = _sellAreaRT.rect.height * 0.5f;

            // ��������� �������� ����� ��� ������ �Ǹ�
            if (rectPos.x - width <= dropPos.x && dropPos.x <= rectPos.x + width &&
                rectPos.y - height <= dropPos.y && dropPos.y <= rectPos.y + height)
            {
                // �������� ������ ������ ���
                if (itemSlot.IsEquiped)
                {
                    // TODO: ���� ���� �䱸 �ȳ� UI ǥ��

                    return;
                }

                _sellItemSlot = itemSlot;

                var itemAmount = _sellItemSlot.GetItemAmount();
                // ������ ������ 2�� �̻��� ���
                if (itemAmount > 1)
                {
                    // �Ǹ� ���� ���� Ȯ�� �г� ����
                    inputPanel.SetPanel(2, itemAmount);
                    // �̺�Ʈ �Ҵ�
                    inputPanel.OnConfirmAmount += ConfirmAmount;
                }
                else
                {
                    // TODO: �Ǹ� Ȯ�� �ȳ� UI ǥ��

                }
            }
        }

        #region Event
        // ������ ������ ��� ȣ��Ǵ� �̺�Ʈ �Լ�
        private void OnSelectSlot(int slotID)
        {
            // ���� ���� ���̶���Ʈ ����
            if (_selectedSlotIndex != -1)
                shopItemSlots[_selectedSlotIndex].Highlight(false);

            _selectedSlotIndex = slotID;

            _selectedSlot = shopItemSlots[slotID];
            _selectedItem = _selectedSlot.AssignedItem;
            int itemPrice = _selectedItem.ItemPrice;
            uint currentGold = GameManager.Instance.DataManager.PlayerData.Gold;

            // ���� �÷��̾ ������ ��尡 ������ ���ݺ��� ���� ���
            if (currentGold < itemPrice)
            {
                Debug.Log("���� ��尡 �����Ͽ� ������ �� �����ϴ�.");
                // TODO: �ݾ� ���� UI ǥ��
            }
            else
            {
                // �������� ��� �ƴ� ��� ���� ���� ����
                if (_selectedItem.ItemType != ItemType.Equipment)
                {
                    // ���� ���� ��尡 ������ 2�� �̻� ���� ������ ��� ���� ���� �г� ����
                    if (currentGold >= itemPrice * 2)
                    {
                        // ���� ���� �г� ����(�ִ� ���� ���� ����)
                        inputPanel.SetPanel(1, (uint)(currentGold / itemPrice));
                        // �̺�Ʈ �Ҵ�
                        inputPanel.OnConfirmAmount += ConfirmAmount;
                    }

                    // ���� ���� ȿ��
                    _selectedSlot.Highlight(true);
                }
                else
                    ConfirmAmount(1);
            }
        }

        // ���� �巡�� ���� �� �̺�Ʈ �Լ�
        private void OnDragBegin(PointerEventData eventData)
        {
            scrollRect.OnBeginDrag(eventData);
        }

        // ������ �巡�� �� ��ũ���� �����̵��� �ϰ� �ϴ� �̺�Ʈ �Լ�
        private void OnDrag(PointerEventData eventData)
        {
            scrollRect.OnDrag(eventData);
        }

        private void OnDragEnd(int _, PointerEventData eventData)
        {
            scrollRect.OnEndDrag(eventData);
        }
        #endregion
    }
}
