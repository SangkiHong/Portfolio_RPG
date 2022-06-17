using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace SK.UI
{
    /* 작성자: 홍상기
     * 내용: 상점에 대한 전박적인 기능의 관리자 클래스
     * 작성일: 22년 6월 14일
     */

    // 상점 타입
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

        public int ShopMode { get; private set; } // 0: 구매, 1: 판매

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
            // 이벤트 할당 해제
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

        // 상점 데이터 정보 초기화
        public void Initialize()
        {
            // 리스트 초기화
            if (_propsItemList == null) _propsItemList = new List<Item>();
            if (_equipmentsItemList == null) _equipmentsItemList = new List<Item>();

            // 잡화 상점의 판매 아이템 리스트 데이터를 가져와서 리스트에 저장
            GameManager.Instance.DataManager.GetShopList(0, ref _propsItemList);
            // 장비 상점의 판매 아이템 리스트 데이터를 가져와서 리스트에 저장
            GameManager.Instance.DataManager.GetShopList(1, ref _equipmentsItemList);

            _currentShopType = ShopType.None;

            // 인벤토리 창 변수 저장
            _inventoryWindow = GameManager.Instance.UIManager.window_Invenroty;

            // 슬롯 초기화
            var slotArr = slotParent.GetComponentsInChildren<ShopItemSlot>();
            shopItemSlots = new List<ShopItemSlot>();
            for (int i = 0; i < slotArr.Length; i++)
            {
                // 슬롯 초기화
                slotArr[i].Initialize(highlightColor);
                slotArr[i].SetSlotID(_addSlotIndex++);
                // 이벤트 할당
                slotArr[i].OnSelectSlotEvent += OnSelectSlot;
                slotArr[i].OnBeginDragEvent += OnDragBegin;
                slotArr[i].OnDragEvent += OnDrag;
                slotArr[i].OnDragEndEvent += OnDragEnd;
                // 리스트에 슬롯을 추가
                shopItemSlots.Add(slotArr[i]);
            }

            // 미리 배치한 슬롯보다 판매 아이템 리스트 갯수가 많은지 확인
            _propsItemCount = _propsItemList.Count;
            _equipmentsItemCount = _equipmentsItemList.Count;
            int maxCount = _propsItemCount > _equipmentsItemCount ? _propsItemCount : _equipmentsItemCount;
            int needCount = maxCount - shopItemSlots.Count;

            // 부족한 슬롯이 있으면 생성
            if (needCount > 0)
            {
                for (int i = 0; i < needCount; i++)
                {
                    // 부족한 갯수만큼 슬롯 생성과 동시에 슬롯 컴포넌트를 리스트에 추가
                    ShopItemSlot tempSlot = Instantiate(slotPrefab, slotParent).GetComponent<ShopItemSlot>();
                    // 슬롯 초기화
                    tempSlot.Initialize(highlightColor);
                    tempSlot.SetSlotID(_addSlotIndex++);
                    // 이벤트 할당
                    tempSlot.OnSelectSlotEvent += OnSelectSlot;
                    tempSlot.OnBeginDragEvent += OnDragBegin;
                    tempSlot.OnDragEvent += OnDrag;
                    tempSlot.OnDragEndEvent += OnDragEnd;
                    shopItemSlots.Add(tempSlot);
                }
            }

            // 탭 버튼 이벤트 함수 할당
            tabHandler.OnChanged += ChangeTab;
        }

        // 상점 관련 창, 패널 UI 닫음
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
                // 상점 창 닫음
                shopWindow.alpha = 0;
                shopWindow.blocksRaycasts = false;

                // 인벤토리 창도 함께 닫음
                _inventoryWindow.alpha = 0;
                _inventoryWindow.blocksRaycasts = false;
                    
                // 상점 모드 초기화
                tabHandler.Reset();
                return true;
            }
            else
                return false;
        }

        // 상점 창 여는 함수
        public void OpenShop(ShopType shopType)
        {
            shopWindow.alpha = 1f;
            shopWindow.blocksRaycasts = true;

            // 선택 슬롯 인덱스 초기화
            _selectedSlotIndex = -1;

            // 인벤토리 창도 함께 열림
            _inventoryWindow.alpha = 1;
            _inventoryWindow.blocksRaycasts = true;

            // 상점 모드 초기화
            if (ShopMode != 0)
                tabHandler.Reset();

            // 이전에 열었던 상점과 동일하면 상점 UI 업데이트를 하지 않음
            if (_currentShopType == shopType) 
                return;
            else
            {
                // 변수에 현재 지정된 상점 타입을 저장
                _currentShopType = shopType;

                // 슬롯 초기화
                foreach (var slot in shopItemSlots)
                    slot.Unassign();

                // 잡화 상점인 경우
                if (shopType == ShopType.Props)
                {
                    // 슬롯에 잡화 상점 판매 아이템을 할당
                    for (int i = 0; i < _propsItemCount; i++)
                        shopItemSlots[i].Assign(_propsItemList[i]);
                }
                // 장비 상점인 경우
                else
                {
                    // 슬롯에 장비 상점 판매 아이템을 할당
                    for (int i = 0; i < _equipmentsItemCount; i++)
                        shopItemSlots[i].Assign(_equipmentsItemList[i]);
                }
            }
        }

        // 구매, 판매 수량 확정_220615
        private void ConfirmAmount(uint amount)
        {
            // 수량이 0 이상인 경우
            if (amount != 0)
            {
                // 구매 모드
                if (ShopMode == 0)
                {
                    Debug.Log($"구매 아이템: {shopItemSlots[_selectedSlotIndex].AssignedItem.ItemName}, 수량: {amount}");
                }
                // 판매 모드
                else
                {
                    Debug.Log($"아이템 판매: {_sellItemSlot.AssignedItem.ItemName}, 수량: {amount}");
                    // TODO: 판매 확정 안내 UI 표시

                }
            }
            else
            {
                Debug.Log("구매 또는 판매 취소");
            }
            if (_selectedSlotIndex != -1)
                shopItemSlots[_selectedSlotIndex].Highlight(false);
        }

        // 구매 판매 탭 전환
        public void ChangeTab(int index)
        {
            ShopMode = index;

            // 구매 모드
            if (ShopMode == 0)
            {
                scrollRect.gameObject.SetActive(true);
                sellArea.SetActive(false);
            }
            // 판매 모드
            else
            {
                scrollRect.gameObject.SetActive(false);
                sellArea.SetActive(true);
            }
        }

        // 판매할 아이템을 드랍영역에 떨어뜨렸는지 확인하는 함수_220616
        public void CheckSellItemDropArea(PointerEventData eventData, InventorySlot itemSlot)
        {
            var dropPos = eventData.position;

            var rectPos = _sellAreaRT.position;
            var width = _sellAreaRT.rect.width * 0.5f;
            var height = _sellAreaRT.rect.height * 0.5f;

            // 드랍영역에 아이템을 드랍한 경우 아이템 판매
            if (rectPos.x - width <= dropPos.x && dropPos.x <= rectPos.x + width &&
                rectPos.y - height <= dropPos.y && dropPos.y <= rectPos.y + height)
            {
                // 아이템을 착용한 상태인 경우
                if (itemSlot.IsEquiped)
                {
                    // TODO: 착용 해제 요구 안내 UI 표시

                    return;
                }

                _sellItemSlot = itemSlot;

                var itemAmount = _sellItemSlot.GetItemAmount();
                // 아이템 갯수가 2개 이상인 경우
                if (itemAmount > 1)
                {
                    // 판매 모드로 수량 확인 패널 열기
                    inputPanel.SetPanel(2, itemAmount);
                    // 이벤트 할당
                    inputPanel.OnConfirmAmount += ConfirmAmount;
                }
                else
                {
                    // TODO: 판매 확정 안내 UI 표시

                }
            }
        }

        #region Event
        // 슬롯을 선택한 경우 호출되는 이벤트 함수
        private void OnSelectSlot(int slotID)
        {
            // 이전 슬롯 하이라이트 꺼짐
            if (_selectedSlotIndex != -1)
                shopItemSlots[_selectedSlotIndex].Highlight(false);

            _selectedSlotIndex = slotID;

            _selectedSlot = shopItemSlots[slotID];
            _selectedItem = _selectedSlot.AssignedItem;
            int itemPrice = _selectedItem.ItemPrice;
            uint currentGold = GameManager.Instance.DataManager.PlayerData.Gold;

            // 현재 플레이어가 소유한 골드가 아이템 가격보다 적은 경우
            if (currentGold < itemPrice)
            {
                Debug.Log("현재 골드가 부족하여 구매할 수 없습니다.");
                // TODO: 금액 부족 UI 표시
            }
            else
            {
                // 아이템이 장비가 아닌 경우 복수 구매 가능
                if (_selectedItem.ItemType != ItemType.Equipment)
                {
                    // 현재 소유 골드가 아이템 2개 이상 구매 가능한 경우 복수 구매 패널 열기
                    if (currentGold >= itemPrice * 2)
                    {
                        // 복수 구매 패널 열기(최대 구매 수량 전달)
                        inputPanel.SetPanel(1, (uint)(currentGold / itemPrice));
                        // 이벤트 할당
                        inputPanel.OnConfirmAmount += ConfirmAmount;
                    }

                    // 슬롯 선택 효과
                    _selectedSlot.Highlight(true);
                }
                else
                    ConfirmAmount(1);
            }
        }

        // 슬롯 드래그 시작 시 이벤트 함수
        private void OnDragBegin(PointerEventData eventData)
        {
            scrollRect.OnBeginDrag(eventData);
        }

        // 슬롯을 드래그 시 스크롤을 움직이도록 하게 하는 이벤트 함수
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
