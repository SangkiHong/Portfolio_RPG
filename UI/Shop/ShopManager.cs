using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using SK.Data;

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
        [SerializeField] private ShopTabHandler tabHandler;
        [SerializeField] private AmountInputPanel inputPanel;

        [Header("Component")]
        [SerializeField] private CanvasGroup shopWindow;
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private Transform slotParent;
        [SerializeField] private GameObject sellArea;

        [Header("Prefab")]
        [SerializeField] private GameObject slotPrefab;

        [Header("UI Setting")]
        [SerializeField] private Color highlightColor;

        public int ShopMode { get; private set; } // -1: None, 0: ����, 1: �Ǹ�

        private List<ShopItemSlot> shopItemSlots;
        private List<Item> _propsItemList;
        private List<Item> _equipmentsItemList;

        private ShopType _currentShopType;

        private Item _selectedItem;
        private ShopItemSlot _selectedSlot;
        private InventorySlot _sellItemSlot;

        private DataManager _dataManager;
        private UIManager _uiManager;
        private InventoryManager _inventoryManager;

        private CanvasGroup _inventoryWindow;
        private RectTransform _sellAreaRT;

        // ���� ������ ������ ������ ����
        private int _propsItemCount, _equipmentsItemCount;
        // ���� �ε���
        private int _addSlotIndex, _selectedSlotIndex = -1;
        // �Ǹ� ������ ����
        private uint _confirmAmount;

        private void Awake()
        {
            _sellAreaRT = sellArea.transform as RectTransform;
        }

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

        // ���� ������ ���� �ʱ�ȭ
        public void Initialize()
        {
            // ���۷��� ����
            _dataManager = GameManager.Instance.DataManager;
            _uiManager = UI.UIManager.Instance;
            _inventoryManager = _uiManager.inventoryManager;

            // ����Ʈ �ʱ�ȭ
            if (_propsItemList == null) _propsItemList = new List<Item>();
            if (_equipmentsItemList == null) _equipmentsItemList = new List<Item>();

            // ��ȭ ������ �Ǹ� ������ ����Ʈ �����͸� �����ͼ� ����Ʈ�� ����
            _dataManager.GetShopList(0, ref _propsItemList);
            // ��� ������ �Ǹ� ������ ����Ʈ �����͸� �����ͼ� ����Ʈ�� ����
            _dataManager.GetShopList(1, ref _equipmentsItemList);

            _currentShopType = ShopType.None;

            // �κ��丮 â ���� ����
            _inventoryWindow = UI.UIManager.Instance.window_Invenroty;

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

        // ���� ���� â, �г� ����
        public void ClosePanelAndWindowButton()
        {
            if(_uiManager.confirmPanel.IsShow)
                _uiManager.confirmPanel.Cancel();

            if (_uiManager.inventoryManager.itemSpecificsPanel.IsOpen)
                _uiManager.inventoryManager.itemSpecificsPanel.Close();

            if (inputPanel.gameObject.activeSelf)
            {
                shopItemSlots[_selectedSlotIndex].Highlight(false);
                inputPanel.gameObject.SetActive(false);
                inputPanel.OnConfirmAmount -= ConfirmAmount;
            }

            if (shopWindow.blocksRaycasts)
            {
                // ���� â ����
                shopWindow.alpha = 0;
                shopWindow.blocksRaycasts = false;

                // �κ��丮 â�� �Բ� ����
                _inventoryWindow.alpha = 0;
                _inventoryWindow.blocksRaycasts = false;

                // ���� ��� �ʱ�ȭ
                ShopMode = -1;
                tabHandler.Reset();
            }
        }

        // ���� ���� â, �г� UI ����
        public bool ClosePanelAndWindow()
        {
            if (_uiManager.confirmPanel.IsShow)
            {
                _uiManager.confirmPanel.Cancel();
                return true;
            }
            else if (inputPanel.gameObject.activeSelf)
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
                ShopMode = -1;
                tabHandler.Reset();
                return true;
            }

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
            {
                ShopMode = 0;
                tabHandler.Reset();
            }

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
                _confirmAmount = amount;
                // ���� ���
                if (ShopMode == 0)
                {
                    BuyItem();
                }
                // �Ǹ� ���
                else
                {
                    // �Ǹ� Ȯ�� �ȳ� UI ǥ��
                    _uiManager.confirmPanel.ShowInfo(InfoType.SellItem);
                    _uiManager.confirmPanel.OnConfirmed += SellItem;
                }
            }
            else
            {
                Debug.Log("���� �Ǵ� �Ǹ� ���");
            }
            if (_selectedSlotIndex != -1)
                shopItemSlots[_selectedSlotIndex].Highlight(false);
        }

        // ������ ����_220617
        private void BuyItem()
        {
            Debug.Log($"���� ������: {shopItemSlots[_selectedSlotIndex].AssignedItem.ItemName}, ����: {_confirmAmount}");

            _selectedItem = shopItemSlots[_selectedSlotIndex].AssignedItem;
            // ������ ������ ������, �κ��丮 �߰�
            if (_dataManager.AddItem(_selectedItem, _confirmAmount, true))
                _dataManager.SubtractGold(_selectedItem.ItemPrice * _confirmAmount); // ���� �ݾ��� �÷��̾� ���� �ݾ׿��� ����
            else // ������ ������ �ȵ� ���
                _uiManager.confirmPanel.ShowInfo(InfoType.NotEnoughSlot);

            // ȿ���� ���
            AudioManager.Instance.PlayAudio(Strings.Audio_UI_BuyItem);
        }

        // ������ �Ǹ� �õ�_220617
        public void TrySellItem(InventorySlot itemSlot)
        {
            // �������� ������ ������ ���
            if (itemSlot.IsEquiped)
            {
                // ���� ���� �䱸 �ȳ� UI ǥ��
                _uiManager.confirmPanel.ShowInfo(InfoType.UnequipItem);
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
                _confirmAmount = itemAmount;
                // �Ǹ� Ȯ�� �ȳ� UI ǥ��
                _uiManager.confirmPanel.ShowInfo(InfoType.SellItem);
                // �̺�Ʈ �Ҵ�
                _uiManager.confirmPanel.OnConfirmed += SellItem;
            }
        }

        // ������ �Ǹ�_220617
        private void SellItem()
        {
            Debug.Log($"������ �Ǹ�: {_sellItemSlot.AssignedItem.ItemName}, ����: {_confirmAmount}");

            // �Ǹ� �ݾ� �÷��̾� ���� �ݾ׿� �߰�
            _dataManager.AddGold(_sellItemSlot.AssignedItem.ItemPrice * _confirmAmount);
            // �����Ϳ��� ������ ���� ���� �Ǵ� ����
            _dataManager.DeleteItemData(_sellItemSlot, _sellItemSlot.OriginSlotID == -1 ? 
                            _sellItemSlot.slotID : _sellItemSlot.OriginSlotID, _confirmAmount);
            // �κ��丮 ������ ���� ���� �Ǵ� ���� �Ҵ� ����
            _inventoryManager.DeleteItem(_sellItemSlot.slotID, _confirmAmount);

            // ȿ���� ���
            AudioManager.Instance.PlayAudio(Strings.Audio_UI_SellItem);
        }

        // ���� �Ǹ� �� ��ȯ
        public void ChangeTab(int index)
        {
            ShopMode = index;

            // ������ �� UI ����
            _uiManager.inventoryManager.itemSpecificsPanel.Close();

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
            // �Ҵ���� ���� ������ ��� ��� ��ȯ
            if (!itemSlot.IsAssigned) return;

            var dropPos = eventData.position;

            var rectPos = _sellAreaRT.position;
            var width = _sellAreaRT.rect.width * 0.5f;
            var height = _sellAreaRT.rect.height * 0.5f;

            // ��������� �������� ����� ��� ������ �Ǹ�
            if (rectPos.x - width <= dropPos.x && dropPos.x <= rectPos.x + width &&
                    rectPos.y - height <= dropPos.y && dropPos.y <= rectPos.y + height)
                TrySellItem(itemSlot);
        }

        #region Event
        // ������ ������ ��� ȣ��Ǵ� �̺�Ʈ �Լ�
        private void OnSelectSlot(int slotID)
        {
            // ���� ���� ���̶���Ʈ ����
            if (_selectedSlotIndex != -1)
                shopItemSlots[_selectedSlotIndex].Highlight(false);

            // ������ �� UI ����
            _uiManager.inventoryManager.itemSpecificsPanel.Close();

            _selectedSlotIndex = slotID;

            _selectedSlot = shopItemSlots[slotID];
            _selectedItem = _selectedSlot.AssignedItem;
            uint itemPrice = _selectedItem.ItemPrice;
            uint currentGold = _dataManager.PlayerData.Gold;

            // ���� �÷��̾ ������ ��尡 ������ ���ݺ��� ���� ���
            if (currentGold < itemPrice)
            {
                Debug.Log("���� ��尡 �����Ͽ� ������ �� �����ϴ�.");
                // �ݾ� ���� UI ǥ��
                _uiManager.confirmPanel.ShowInfo(InfoType.NotEnoughCurruncy);
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
                        inputPanel.SetPanel(1, currentGold / itemPrice);
                        // �̺�Ʈ �Ҵ�
                        inputPanel.OnConfirmAmount += ConfirmAmount;

                        // ���� ���� ȿ��
                        _selectedSlot.Highlight(true);
                        return;
                    }
                }

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
