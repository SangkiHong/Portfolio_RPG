using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using SK.Data;
using Object = UnityEngine.Object;
using System.Collections.Generic;

namespace SK.UI
{
    
    public class InventoryManager : MonoBehaviour
    {
        private readonly struct SelectItem
        {
            public Item Item { get; }
            public uint ItemAmount { get; }
            public uint SelectAmount { get; }

            public SelectItem(Item item, uint itemAmount, uint selectAmount)
            {
                Item = item;
                ItemAmount = itemAmount;
                SelectAmount = selectAmount;
            }
        }

        [Header("Reference")]
        [SerializeField] private UIManager uiManager;

        [Header("Slot")]
        [SerializeField] private GameObject slotParent;

        [Header("Tab")]
        [SerializeField] private Transform tabFocus;
        [SerializeField] private Button[] tabs;

        [Header("Info")]
        [SerializeField] private float maxWeightPerStr;
        [SerializeField] private Slider weightSlider;
        [SerializeField] private Text text_GoldAmount;
        [SerializeField] private Text text_GemAmount;
        [SerializeField] private Text text_CurrentItemAmount;

        [Header("Erase")]
        [SerializeField] private Button button_EraseMode;
        [SerializeField] private Button button_Erase;
        [SerializeField] private Button button_CancelSelection;

        [Header("Listing")]
        [SerializeField] private Button autoListingButton;
        [SerializeField] private ItemType[] listOrderby; // 슬롯 자동 정렬 순서

        [Header("Panel")]
        [SerializeField] private GameObject eraseMenuPanel;
        [SerializeField] private AmountInputPanel amountInputPanel;
        [SerializeField] internal ItemSpecificsPanel itemSpecificsPanel;

        // 선택 모드 시 임시저장용 리스트
        private List<SelectItem> _selectedItemList;

        // 자동 정렬 시 임시로 데이터를 담을 큐 배열
        private Queue<ItemData>[] _itemDataQueues;
        private InventorySlot[] _slots;
        private RectTransform[] _rectTransforms;

        private PlayerItemData _itemData; // 아이템 데이터 클래스 변수
        private InventorySlot _selectedSlot; // 선택한 슬롯을 저장할 변수
        private ItemData _tempData; // 슬롯 교환 시 임시 데이터 저장할 변수

        private Vector3 _focusLocalPos;

        public bool IsDragging { get; private set; }
        private bool _isCategoryListing, // 카테고리 정렬 모드 여부
                     _isSelectMode; // 아이템 선택 모드 여부

        // 선택한 슬롯과 탭의 인덱스를 저장할 변수
        private int _selectedSlotID, _selectedTabIndex;

        private void Awake()
        {
            // 초기화
            _slots = slotParent.GetComponentsInChildren<InventorySlot>();
            _rectTransforms = new RectTransform[_slots.Length];
            _selectedItemList = new List<SelectItem>();
            _itemDataQueues = new Queue<ItemData>[Enum.GetValues(typeof(ItemType)).Length];
            for (int i = 0; i < _itemDataQueues.Length; i++)
                _itemDataQueues[i] = new Queue<ItemData>();

            _focusLocalPos = tabFocus.localPosition;

            // slot ID 할당 및 슬롯의 포인터 이벤트 등록_220504
            for (int i = 0; i < _slots.Length; i++)
            {
                _slots[i].SetSlotID(i);
                _slots[i].OnBeginDragEvent += OnBeginDrag; // 슬롯 드래그 시 발동 이벤트 등록
                _slots[i].OnLeftClickEvent += OnLeftClickSlot; // 슬롯을 단순 좌클릭 시 발동 이벤트 등록
                _slots[i].OnLeftClickEvent += TurnOffAllHightlight; // 슬롯을 단순 좌클릭 시 발동 이벤트 등록
                _slots[i].OnRightClickEvent += OnRightClickSlot; // 슬롯을 단순 우클릭 시 발동 이벤트 등록
                _slots[i].OnAssignEvent += NewAssignItem; // 슬롯에 새로운 아이템 할당 시 발동 이벤트 등록
                _slots[i].OnSwapEvent += SwapSlot; // 슬롯 상태 변경 시 발동 이벤트 등록
                _slots[i].OnDragEndEvent += CheckSlotCollision; // 슬롯 드랍 시 발동 이벤트 등록

                _rectTransforms[i] = _slots[i].transform as RectTransform;
            }

            // Tab 버튼 이벤트 할당_220506
            for (int i = 0; i < tabs.Length; i++)
            {
                // Closuer problem 으로 인해 임시 인덱스 값을 생성
                int tempIndex = i;

                // 탭 인덱스에 따른 아이템 리스팅
                tabs[i].onClick.AddListener(delegate { TabButton(tempIndex); });
            }

            // 버튼 이벤트 할당_220506
            autoListingButton.onClick.AddListener(AutoListing); // 자동 정렬 버튼 이벤트 할당
            button_EraseMode.onClick.AddListener(SwitchEraseMode); // 아이템 삭제 모드 버튼 이벤트 할당
            button_Erase.onClick.AddListener(EraseAllSelectedItem); // 선택한 아이템을 모두 삭제하는 버튼 이벤트 할당
            button_CancelSelection.onClick.AddListener(CancelSelection); // 삭제할 아이템 선택을 모두 취소하는 버튼 이벤트 할당
        }

        public void Initialize()
        {
            // 보유 아이템 데이터를 토대로 인벤토리에 슬롯 정보 불러오기(초기화)_220503
            LoadSlotData();
        }

        public bool CloseAllPanel()
        {
            if (eraseMenuPanel.gameObject.activeSelf)
            {
                eraseMenuPanel.SetActive(false);
                amountInputPanel.gameObject.SetActive(false);
                amountInputPanel.OnConfirmAmount -= ConfirmEraseAmount;
                // 삭제 모드 시 모드 전환
                if (_isSelectMode) SwitchEraseMode();
                return true;
            }
            else
                return false;
        }

        #region SLOT DATA
        // 플레이어 아이템 데이터 정보 불러오기_220504
        private void LoadSlotData()
        {
            _itemData = GameManager.Instance.Player.playerItemData;

            for (int i = 0; i < _itemData.items.Count; i++)
            {
                // 아이템이 슬롯에 할당되었던 경우 지정된 슬롯에 아이템 할당
                if (_itemData.items[i].slotID != -1)
                    _slots[_itemData.items[i].slotID].AssignItem(_itemData.items[i].item, _itemData.items[i].amount, _itemData.items[i].isEquiped); 
                // 아이템이 슬롯에 할당되지 않았던 경우 빈 슬롯에 아이템 할당
                else
                {
                    for (int j = 0; j < _slots.Length; j++)
                    {
                        if (!_slots[j].IsAssigned) // 할당되지 않은 빈 슬롯인지 체크
                        {
                            _slots[j].AssignItem(_itemData.items[i].item, _itemData.items[i].amount, _itemData.items[i].isEquiped); // 빈 슬롯에 아이템 할당
                            _itemData.items[i].slotID = _slots[j].slotID; // 아이템 데이터에 슬롯 ID 지정
                            break;
                        }    
                    }
                }
            }
            UpdateInfoUI();
        }

        public bool AddNewItem(Item newItem, uint amount = 1, bool applyData = false)
        {
            // 인벤토리가 꽉 찼으면 false를 반환
            if (IsFull()) return false;

            for (int i = 0; i < _slots.Length; i++)
            {
                // 할당되지 않은 빈 슬롯인지 체크
                if (!_slots[i].IsAssigned) 
                {
                    // 중복 수량 추가 불가한 아이템을 2개 이상 추가하는 경우
                    if (amount > 1 && !newItem.IsStackable)
                    {
                        // 빈 슬롯에 아이템 할당
                        _slots[i].AssignItem(newItem, 1, false, true);
                        amount--;

                        // 플레이어 데이터에 아이템 추가 반영
                        if (applyData) GameManager.Instance.DataManager.AddNewItemData(_slots[i]);
                        continue;
                    }

                    // 빈 슬롯에 아이템 할당
                    _slots[i].AssignItem(newItem, amount, false, true);

                    // 플레이어 데이터에 아이템 추가 반영
                    if (applyData) GameManager.Instance.DataManager.AddNewItemData(_slots[i]);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 새로운 슬롯 할당_220504
        /// </summary>
        /// <param name="slot">슬롯 정보</param>
        /// <param name="slotID">슬롯 ID</param>
        /// <param name="amount">아이템 수량</param>
        private void NewAssignItem(SlotBase slot, uint amount)
        {
            _selectedSlot = slot as InventorySlot;
            GameManager.Instance.DataManager.AddNewItemData(_selectedSlot);
            UpdateInfoUI();
        }

        // 슬롯이 꽉 찬 상태인지를 반환(꽉 찼으면 True를 반환)_220520
        public bool IsFull() { return _itemData.items.Count < _slots.Length; }
        #endregion

        #region INFO UI
        private void UpdateInfoUI()
        {
            // 인베토리 최대 무게
            weightSlider.maxValue = GameManager.Instance.Player.playerData.Str * maxWeightPerStr;

            // 아이템 총 무게 변수
            float totalWeight = 0;
            for (int i = 0; i < _itemData.items.Count; i++)            
                totalWeight += _itemData.items[i].item.Weight;

            // 아이템 총 무게 슬라이드에 표시
            weightSlider.value = totalWeight;

            // 아이템 갯수 텍스트 표시
            text_CurrentItemAmount.text = _itemData.items.Count.ToString();
        }
        #endregion

        #region LISTING & TAB BUTTON
        // 전체 슬롯을 데이터에 기반해 업데이트하는 함수_220520
        public void UpdateSlots()
        {
            // 아이템 리스팅 함수 호출
            LoadSlotByCategory(_selectedTabIndex);

            // 인벤토리 정보 UI를 업데이트
            UpdateInfoUI();
        }

        // 카테고리 별로 아이템 정렬(탭 인덱스)_220506
        private void TabButton(int tabIndex)
        {
            // 탭 인덱스 값을 변수에 저장_220520
            _selectedTabIndex = tabIndex;

            // 아이템 리스팅 함수 호출
            LoadSlotByCategory(_selectedTabIndex); 

            // 선택된 탭 버튼 비활성화
            tabs[tabIndex].interactable = false;
            for (int i = 0; i < tabs.Length; i++)
            {
                // 선택된 탭을 제외한 모든 탭의 활성화
                if (i != tabIndex)
                    tabs[i].interactable = true;
            }

            // 포커싱 이미지가 선택된 탭에 아래 오게 함
            _focusLocalPos.x = tabs[tabIndex].transform.localPosition.x;
            tabFocus.localPosition = _focusLocalPos;

            // 아이템 세부 정보 패널이 켜져있는 경우 끔
            if (itemSpecificsPanel.gameObject.activeSelf) 
                itemSpecificsPanel.gameObject.SetActive(false);

            // 아이템 삭제모드 중인 경우 
            if (_isSelectMode && _itemData.items.Count != 0)
            {
                // 삭제 수량 입력 패드가 켜져있다면 끔
                if (amountInputPanel.gameObject.activeSelf) 
                    amountInputPanel.gameObject.SetActive(false);

                // 할당된 슬롯 중에 삭제하기로 선택된 아이템을 탐색하여 하이라이트로 표시_220509
                for (int i = 0; i < _slots.Length; i++)
                {
                    if (_slots[i].IsAssigned)
                    {
                        for (int j = 0; j < _selectedItemList.Count; j++)
                        {
                            
                            if (_slots[i].AssignedItem.Equals(_selectedItemList[j].Item) &&
                                _slots[i].GetItemAmount() == _selectedItemList[j].ItemAmount)
                                _slots[i].highlight.SetActive(true);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 카테고리 별로 아이템 슬롯에 할당_220506
        /// </summary>
        /// <param name="tabCategory">0: All, 1: Weapon, 2: Equipment, 3: Usable Item, 4: etc</param>
        private void LoadSlotByCategory(int tabCategory)
        {
            // 모든 슬롯을 비움
            ClearAllSlots();

            // 모든 종류의 아이템 리스팅
            if (tabCategory == 0)
            {
                LoadSlotData();
                // 모든 아이템 리스팅 중일 때만 슬롯 이동을 데이터에 반영 함
                _isCategoryListing = false;
                return;
            }

            // 카테고리 리스팅 여부를 판단하게 하여 아이템 슬롯 이동 시 데이터 수정을 건너뛰게 함
            _isCategoryListing = true;

            int slotCount = 0;
            for (int i = 0; i < _itemData.items.Count; i++)
            {
                if (tabCategory == 1) // 무기 종류 아이템 리스팅
                {
                    if (_itemData.items[i].item.ItemType == ItemType.Equipment &&
                        _itemData.items[i].item.EquipmentType == EquipmentType.Weapon)
                        _slots[slotCount++].AssignItem(_itemData.items[i].item, _itemData.items[i].amount, _itemData.items[i].isEquiped);
                }
                else if (tabCategory == 2) // 방어구 종류 아이템 리스팅
                {
                    if (_itemData.items[i].item.ItemType == ItemType.Equipment &&
                        _itemData.items[i].item.EquipmentType != EquipmentType.Weapon)
                        _slots[slotCount++].AssignItem(_itemData.items[i].item, _itemData.items[i].amount, _itemData.items[i].isEquiped);
                }
                else if (tabCategory == 3) // 사용 가능한 아이템 리스팅
                {
                    if (_itemData.items[i].item.ItemType != ItemType.Equipment && _itemData.items[i].item.IsConsumable)
                        _slots[slotCount++].AssignItem(_itemData.items[i].item, _itemData.items[i].amount, false);
                }
                else // 그 외 모든 아이템 리스팅
                {
                    _slots[slotCount++].AssignItem(_itemData.items[i].item, _itemData.items[i].amount, false);
                }
            }
        }

        // 카테고리 별로 자동 정렬 함수_220506
        private void AutoListing()
        {
            // 데이터 없으면 return
            if (_itemData.items.Count == 0) return;

            // 전체 아이템 표시 탭으로 이동_220616
            TabButton(0);

            // 슬롯 ID 변수
            int slotNum = 0;

            // 아이템 타입에 따라 각각의 큐에 보관
            for (int i = 0; i < _itemData.items.Count; i++)            
                _itemDataQueues[(int)_itemData.items[i].item.ItemType].Enqueue(_itemData.items[i]);

            // listOrderby를 통해 Enum의 순회할 순서에 따라서 큐에 접근
            foreach (int itemType in listOrderby)
            {
                // 반복문을 돌며 큐에 있는 데이터를 모두 Dequeue 함
                while (_itemDataQueues[itemType].Count > 0)
                {
                    // 0번 슬롯부터 차례로 데이터의 슬롯 ID를 할당
                    _tempData = _itemDataQueues[itemType].Dequeue();
                    _tempData.slotID = slotNum;
                    // 데이터에 따라서 인벤토리에 아이템 할당
                    _slots[slotNum++].AssignItem(_tempData.item, _tempData.amount, _tempData.isEquiped);
                }
            }
        }

        // 모든 슬롯 할당 해제_220505
        private void ClearAllSlots()
        {
            for (int i = 0; i < _slots.Length; i++)
                _slots[i].Unassign();
        }
        #endregion

        #region SELECT MODE
        public void SelectModeOn()
        {
            _isSelectMode = true;
        }

        // 삭제 모드로 전환하는 함수_220506
        private void SwitchEraseMode()
        {
            _isSelectMode = !_isSelectMode;
            eraseMenuPanel.SetActive(_isSelectMode);

            // 모든 하이라이트를 끔
            TurnOffAllHightlight();

            // 선택된 슬롯 정보 초기화
            if (_selectedItemList.Count > 0) _selectedItemList.Clear();

            // 아이템 정보 끔
            itemSpecificsPanel.gameObject.SetActive(false);
        }

        // 선택한 아이템을 취소하는 함수_220506
        private void CancelSelection()
        {
            // 리스트 초기화
            _selectedItemList.Clear();
            
            // 선택되었던 슬롯 하이라이트를 모두 끔
            for (int i = 0; i < _slots.Length; i++)            
                _slots[i].highlight.SetActive(false);            
        }

        // 선택한 아이템을 모두 삭제하는 함수_220506
        private void EraseAllSelectedItem()
        {
            foreach (var selectedItem in _selectedItemList)
            {
                uint changedAmount = selectedItem.ItemAmount - selectedItem.SelectAmount;

                // 슬롯 데이터 변경(수량이 0이면 삭제, 0보다 많으면 수량 감소)
                if (changedAmount > 0)
                    GameManager.Instance.DataManager.UpdateItemData(selectedItem.Item, selectedItem.ItemAmount, changedAmount);
                else
                    GameManager.Instance.DataManager.DeleteItemData(selectedItem.Item, selectedItem.ItemAmount);

                for (int i = 0; i < _slots.Length; i++)
                {
                    if (_slots[i].IsAssigned && _slots[i].AssignedItem.Equals(selectedItem.Item) && 
                        _slots[i].GetItemAmount() == selectedItem.ItemAmount)
                    {
                        // 변경된 수량이 0보다 크면 슬롯의 정보 변경
                        if (changedAmount > 0)
                            _slots[i].SetItemAmount(changedAmount);
                        // 변경된 수량이 0이면 슬롯 할당 해제
                        else
                            _slots[i].Unassign();
                        break;
                    }
                }
            }

            _selectedItemList.Clear(); // 리스트 초기화
            TurnOffAllHightlight(); // 선택된 모든 슬롯의 하이라이트 꺼짐
            SwitchEraseMode(); // 삭제 모드에서 일반 모드로 변경
            UpdateInfoUI(); // 전체적인 인벤토리 UI 업데이트
        }

        // 입력 패드 화면을 통해 삭제할 아이템 수량을 전달받는 함수_220507
        private void ConfirmEraseAmount(uint eraseAmount)
        {
            // 삭제할 수량이 0이면 삭제 취소
            if (eraseAmount == 0)
                _slots[_selectedSlotID].highlight.SetActive(false);
            else // 리스트에 슬롯 ID, 삭제 수량 추가
                _selectedItemList.Add(new SelectItem(
                    _slots[_selectedSlotID].AssignedItem,
                    eraseAmount,
                    _slots[_selectedSlotID].GetItemAmount()
                ));
        }
        #endregion

        #region EVENT FUNCTION
        private void OnBeginDrag(PointerEventData eventData)
            => IsDragging = true;

        // 슬롯 드랍 시 다른 슬롯과 충돌 확인_220503
        private void CheckSlotCollision(int slotID, PointerEventData eventData)
        {
            IsDragging = false;

            // 아이템 선택 모드 시 드래그 앤 드랍 불가
            if (_isSelectMode) return;

            // 상점 판매 모드 시 아이템 판매
            if (uiManager.shopManager.ShopMode == 1)
            {
                uiManager.shopManager.CheckSellItemDropArea(eventData, _slots[slotID]);
                return;
            }

            var dropPos = eventData.position;

            for (int i = 0; i < _rectTransforms.Length; i++)
            {
                var rectPos = _rectTransforms[i].position;
                var width = _rectTransforms[i].rect.width * 0.5f;
                var height = _rectTransforms[i].rect.height * 0.5f;

                if (rectPos.x - width <= dropPos.x && dropPos.x <= rectPos.x + width && 
                    rectPos.y - height <= dropPos.y && dropPos.y <= rectPos.y + height)
                    _slots[slotID].SwapSlot(_slots[i], !_isCategoryListing);
            }
        }

        // 슬롯을 좌클릭했을 경우 아이템 정보 표시(삭제모드 시 삭제할 아이템 선택)_220506
        private void OnLeftClickSlot(int slotNum)
        {
            // 할당되지 않은 슬롯을 좌클릭하거나 드래그를 시작한 경우
            if (slotNum == -1)
            {
                // 아이템 세부 정보 창 닫기
                itemSpecificsPanel.gameObject.SetActive(false);
                return; 
            }

            _selectedSlot = _slots[slotNum];

            // 삭제 모드인 경우
            if (_isSelectMode)
            {
                // 빈 슬롯 클릭 시 리턴
                if (slotNum == -1) return;

                // 선택된 슬롯 ID를 변수에 저장
                _selectedSlotID = slotNum;

                // 슬롯 아이템의 수량
                var itemAmount = _selectedSlot.GetItemAmount();

                // 이미 선택한 아이템인지 여부 확인
                if (_selectedItemList.Count > 0)
                {
                    for (int i = 0; i < _selectedItemList.Count; i++)
                    {
                        // 이미 선택되었던 경우에는 선택을 취소
                        if (_selectedItemList[i].Item.Equals(_selectedSlot.AssignedItem) &&
                            _selectedItemList[i].ItemAmount == itemAmount)
                        {
                            _selectedItemList.RemoveAt(i);
                            _selectedSlot.highlight.SetActive(false);
                            return;
                        }
                    }
                }

                // 선택이 안되었던 경우에는 선택
                if (itemAmount > 1) // 다중 수량일 경우
                {
                    // 삭제할 수량 입력 패드 켜짐
                    amountInputPanel.SetPanel(0, itemAmount);
                    // 이벤트에 함수 할당
                    amountInputPanel.OnConfirmAmount += ConfirmEraseAmount;
                }
                else // 단일 수량일 경우
                {
                    // 리스트에 추가
                    _selectedItemList.Add(new SelectItem(
                        _slots[_selectedSlotID].AssignedItem,
                        itemAmount,
                        itemAmount
                    ));
                }
            }

            // 할당된 슬롯을 클릭하였을 경우 세부 정보 패널 표시_220508
            if (slotNum >= 0 && _selectedSlot.IsAssigned)
                itemSpecificsPanel.SetPanel(_selectedSlot.AssignedItem, _selectedSlot.transform.position.x);
            else
                itemSpecificsPanel.gameObject.SetActive(false);
        }

        // 슬롯을 우클릭했을 경우 장비 아이템인 경우 착용_220512
        private void OnRightClickSlot(int slotNum)
        {
            _selectedSlot = _slots[slotNum];

            // 슬롯이 장비 아이템으로 할당되어 있는 경우
            if (_selectedSlot.IsAssigned &&
                _selectedSlot.AssignedItem.ItemType.Equals(ItemType.Equipment))
            {
                // 슬롯 아이템 데이터 착용 여부 업데이트
                GameManager.Instance.DataManager.UpdateItemData(_selectedSlot.AssignedItem, !_selectedSlot.IsEquiped);

                // 장비 슬롯 매니저를 통해 케릭터 상태 창에 아이템 착용 여부 표시
                if (!_selectedSlot.IsEquiped)
                    uiManager.equipSlotManager.EquipItem(_selectedSlot.AssignedItem);
                else
                    uiManager.equipSlotManager.UnequipItem(_selectedSlot.AssignedItem);

                // 인벤토리 슬롯에 아이템 착용 여부 전달
                _selectedSlot.EquipItem(!_slots[slotNum].IsEquiped);
            }
        }

        // 두 슬롯 정보 스왑_220504
        private void SwapSlot(int aSlotID, int bSlotID)
        {
            GameManager.Instance.DataManager.SwapSlot(aSlotID, bSlotID);
            UpdateInfoUI();
        }

        // 삭제 모드가 아닌 경우 해당 슬롯 외에 다른 슬롯 하이라이트 꺼짐_220506
        private void TurnOffAllHightlight(int exceptSlot = -1)
        {
            if (_isSelectMode) return;

            for (int i = 0; i < _slots.Length; i++)
            {
                if (i != exceptSlot && _slots[i].highlight.activeSelf)
                    _slots[i].highlight.SetActive(false);
            }
        }
        #endregion

        internal InventorySlot FindSlotByItem(Item item, bool isEquiped = false)
        {
            for (int i = 0; i < _slots.Length; i++)
            {
                if (_slots[i].AssignedItem == item && _slots[i].IsEquiped == isEquiped)
                    return _slots[i];
            }

            return null;
        }

        private void OnDestroy()
        {
            // Event 해제
            for (int i = 0; i < _slots.Length; i++)
            {
                // 등록 이벤트 해제
                _slots[i].OnBeginDragEvent -= OnBeginDrag;
                _slots[i].OnLeftClickEvent -= OnLeftClickSlot;
                _slots[i].OnLeftClickEvent -= TurnOffAllHightlight;
                _slots[i].OnRightClickEvent -= OnRightClickSlot;
                _slots[i].OnAssignEvent -= NewAssignItem;
                _slots[i].OnSwapEvent -= SwapSlot;
                _slots[i].OnDragEndEvent -= CheckSlotCollision;
            }
        }
    }
}