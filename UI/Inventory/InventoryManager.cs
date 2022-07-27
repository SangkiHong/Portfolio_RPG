using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using SK.Data;
using System.Collections.Generic;

namespace SK.UI
{
    
    public class InventoryManager : MonoBehaviour
    {
        private readonly struct SelectItem
        {
            public Item Item { get; }
            public int SlotID { get; }
            public uint SelectAmount { get; }

            public SelectItem(Item item, int slotID, uint selectAmount)
            {
                Item = item;
                SlotID = slotID;
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

        // 레퍼런스
        private ItemListManager _itemListManager;

        // 자동 정렬 시 임시로 데이터를 담을 큐 배열
        private Queue<ItemData>[] _itemDataQueues;
        private InventorySlot[] _slots;
        private RectTransform[] _rectTransforms;

        private PlayerItemData _itemData; // 아이템 데이터 클래스 변수
        private InventorySlot _selectedSlot; // 선택한 슬롯을 저장할 변수
        private ItemData _tempData; // 임시 데이터 저장할 변수
        private Item _tempItem; // 임시 아이템 변수
        private QuickSlot _tempQuickSlot; // 임시 퀵 슬롯 변수

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
                _slots[i].OnAssignEvent += UpdateInfoUI; // 슬롯 할당 시 발동 이벤트 등록
                _slots[i].OnBeginDragEvent += OnBeginDrag; // 슬롯 드래그 시 발동 이벤트 등록
                _slots[i].OnLeftClickEvent += OnLeftClickSlot; // 슬롯을 단순 좌클릭 시 발동 이벤트 등록
                _slots[i].OnLeftClickEvent += TurnOffAllHightlight; // 슬롯을 단순 좌클릭 시 발동 이벤트 등록
                _slots[i].OnRightClickEvent += OnRightClickSlot; // 슬롯을 단순 우클릭 시 발동 이벤트 등록
                _slots[i].OnSwapEvent += SwapSlot; // 슬롯 상태 변경 시 발동 이벤트 등록
                _slots[i].OnDragEndEvent += OnDragEnd; // 슬롯 드랍 시 발동 이벤트 등록

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

            // 레퍼런스 가져오기
            _itemListManager = GameManager.Instance.ItemListManager;
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
            _itemData = DataManager.Instance.PlayerItemData;

            for (int i = 0; i < _itemData.items.Count; i++)
            {
                _tempData = _itemData.items[i];
                // 아이템이 슬롯에 할당되지 않았던 경우 빈 슬롯에 아이템 할당
                if (_tempData.slotID == -1)
                {
                    for (int j = 0; j < _slots.Length; j++)
                    {
                        if (!_slots[j].IsAssigned) // 할당되지 않은 빈 슬롯인지 체크
                        {
                            _slots[j].AssignItem(_tempData.item, _tempData.amount, _tempData.isEquiped, true); // 빈 슬롯에 아이템 할당
                            _tempData.slotID = _slots[j].slotID; // 아이템 데이터에 슬롯 ID 지정
                            break;
                        }
                    }
                }
                else // 아이템이 슬롯에 할당되었던 경우 지정된 슬롯에 아이템 할당
                    _slots[_tempData.slotID].AssignItem(_tempData.item, _tempData.amount, _tempData.isEquiped, true);
            }
            UpdateInfoUI();
        }

        // 해당 아이템 추가
        public bool AddNewItem(Item newItem, uint amount = 1, bool applyData = false)
        {
            // 인벤토리가 꽉 찼으면 false를 반환
            if (IsFull()) return false;

            bool isAdded = false;

            // 중첩 수량 가능한 경우 동일한 아이템이 있는지 확인
            if (newItem.IsStackable)
            {
                for (int i = 0; i < _slots.Length; i++)
                {
                    if(_slots[i].IsAssigned && _slots[i].AssignedItem == newItem)
                    {
                        uint changedAmount = _slots[i].GetItemAmount() + amount;
                        _slots[i].SetItemAmount(changedAmount);

                        // 플레이어 데이터에 아이템 추가 반영
                        if (applyData) GameManager.Instance.DataManager.AddNewItemData(_slots[i]);

                        // 퀵 슬롯에 반영
                        uiManager.quickSlotManager.CheckItemState(_slots[i].AssignedItem, changedAmount);

                        isAdded = true;
                        break;
                    }
                }
            }

            if (!isAdded)
            {
                int emptySlotIndex;

                // 수량 중첩 가능 아이템인 경우
                if (amount == 1 || newItem.IsStackable)
                {
                    // 할당되지 않은 빈 슬롯인지 체크
                    emptySlotIndex = _itemData.GetEmptySlotIndex();
                    _selectedSlot = _slots[emptySlotIndex];

                    // 인벤토리에 할당
                    AssignNewItem(_selectedSlot, newItem, amount);

                    // 플레이어 데이터에 아이템 추가 반영
                    if (applyData) _itemData.AddItem(newItem, emptySlotIndex, amount);
                    isAdded = true;
                }
                // 2개 이상의 중첩 불가 아이템인 경우
                else
                {
                    for (int i = 0; i < amount; i++)
                    {
                        // 할당되지 않은 빈 슬롯인지 체크
                        emptySlotIndex = _itemData.GetEmptySlotIndex();
                        _selectedSlot = _slots[emptySlotIndex];

                        // 인벤토리에 할당
                        AssignNewItem(_selectedSlot, newItem, 1);

                        // 플레이어 데이터에 아이템 추가 반영
                        if (applyData) _itemData.AddItem(newItem, emptySlotIndex, 1);
                    }
                    isAdded = true;
                }
            }

            // 아이템 추가된 경우 인벤토리 정보 UI 업데이트
            if (isAdded) UpdateInfoUI();

            return isAdded;
        }

        private void AssignNewItem(InventorySlot slot, Item item, uint amount)
        {
            // 현재 인벤토리 탭에 할당 가능한지 확인
            if (_selectedTabIndex == 0) // 모든 아이템
            {
                slot.AssignItem(item, amount, false);
            }
            else if(_selectedTabIndex == 1) // 무기 아이템
            {
                if (item.ItemType == ItemType.Equipment &&
                    item.EquipmentType == EquipmentType.Weapon)
                    slot.AssignItem(item, amount, false);
            }
            else if (_selectedTabIndex == 2) // 방어구 아이템
            {
                if (item.ItemType == ItemType.Equipment &&
                    item.EquipmentType != EquipmentType.Weapon)
                    slot.AssignItem(item, amount, false);
            }
            else if (_selectedTabIndex == 3) // 사용 가능한 아이템
            {
                if (item.ItemType != ItemType.Equipment && item.IsConsumable)
                    slot.AssignItem(item, amount, false);
            }
            else // 그 외 모든 아이템
            {
                if (item.ItemType != ItemType.Equipment && !item.IsConsumable)
                    slot.AssignItem(item, amount, false);
            }
        }

        // 슬롯ID에 위치한 아이템 삭제
        public void DeleteItem(int slotID, uint deleteAmount)
        {
            // 삭제 수량보다 아이템 수량이 많은 경우
            var currentAmount = _slots[slotID].GetItemAmount();
            if (currentAmount > deleteAmount)
                _slots[slotID].SetItemAmount(currentAmount - deleteAmount);
            else
                _slots[slotID].Unassign();
        }

        // 슬롯이 꽉 찬 상태인지를 반환(꽉 찼으면 True를 반환)_220520
        public bool IsFull() 
        {
            return _itemData.items.Count >=  _slots.Length; 
        }

        // 할당되지 않은 슬롯의 갯수를 반환_220617
        public int GetEmptySlotCount() 
        {
            var count = 0;
            for (int i = 0; i < _slots.Length; i++)
                if (!_slots[i].IsAssigned)
                    count++;

            return count;
        }

        // 아이템을 저장할 공간이 있는 지 여부를 반환_220720
        public bool CanTakeItem(Item item)
        {
            if (item.IsStackable && HasItem(item))
                return true;

            return GetEmptySlotCount() > 0;
        }

        // 보상 아이템들을 저장할 공간이 있는 지 여부를 반환_220716
        public bool CanTakeRewardItems(Quests.Reward reward, List<Item> rewardItemList)
        {
            var rewardItemCount = reward.rewardItems.Length;
            // 필요 슬롯 갯수
            int needSlotCount = 0;
            if (rewardItemCount > 0)
            {
                // 필요한 슬롯의 갯수가 있는지 확인
                for (int i = 0; i < rewardItemCount; i++)
                {
                    // 리스트에 보상 아이템 추가
                    rewardItemList.Add(_itemListManager.GetItembyID((int)reward.rewardItems[i].itemList,
                            reward.rewardItems[i].itemId));
                    // 중첩 수량 가능한 아이템이며, 현재 인벤토리에 동일 아이템이 있는 경우 필요 슬롯 갯수를 올리지 않음
                    if (rewardItemList[i].IsStackable && HasItem(rewardItemList[i]))
                        continue;
                    else
                        needSlotCount++;
                }
            }

            if (GetEmptySlotCount() >= needSlotCount)
                return true;
            else
                return false;
        }

        // 해당 아이템이 슬롯에 있는 지에 대한 여부를 반환_220617
        public bool HasItem(Item item)
        {
            for (int i = 0; i < _slots.Length; i++)
                if (_slots[i].IsAssigned && _slots[i].AssignedItem == item)
                    return true;

            return false;
        }

        // 해당 아이템이 할당된 슬롯을 탐색하여 슬롯 정보를 반환_220512
        public InventorySlot FindSlotByItem(Item item, bool isEquiped = false)
        {
            for (int i = 0; i < _slots.Length; i++)
            {
                if (_slots[i].AssignedItem == item && _slots[i].IsEquiped == isEquiped)
                    return _slots[i];
            }

            return null;
        }
        #endregion

        #region USE ITEM
        public bool UseItem(Item item, uint itemAmount, uint useAmount)
        {
            uint amount;
            _selectedSlot = null;

            // 해당 아이템이 할당된 인벤토리 슬롯을 탐색
            for (int i = 0; i < _slots.Length; i++)
            {
                if (_slots[i].IsAssigned && _slots[i].AssignedItem == item)
                {
                    amount = _slots[i].GetItemAmount();

                    // 동일 수량 아이템 확인
                    if (amount == itemAmount)
                    {
                        _selectedSlot = _slots[i];
                        // 아이템 사용
                        return UseItem(useAmount);
                    }
                }
            }
            // 해당 아이템이 검색에서 발견되지 않으면 FALSE 리턴
            return false;
        }

        private bool UseItem(uint useAmount = 1)
        {
            _tempItem = _selectedSlot.AssignedItem;

            // 슬롯이 장비 아이템으로 할당되어 있는 경우
            if (_selectedSlot.IsAssigned)
            {
                uint amount = _selectedSlot.GetItemAmount();

                // 장비 아이템인 경우 아이템 착용
                if (_tempItem.ItemType == ItemType.Equipment)
                {
                    // 착용할 아이템이 보조 장비(방패 등)이며, 양손 무기 착용 중이거나 무기 착용이 없는 경우 리턴
                    if (!_selectedSlot.IsEquiped && _tempItem.EquipmentType == EquipmentType.Shield &&
                        (GameManager.Instance.Player.equipmentHolder.primaryEquipment == null ||
                        GameManager.Instance.Player.equipmentHolder.primaryEquipment.isTwoHand))
                        return false;

                    // 슬롯 아이템 데이터 착용 여부 업데이트
                    GameManager.Instance.DataManager.UpdateItemData(_tempItem, !_selectedSlot.IsEquiped);

                    // 장비 슬롯 매니저를 통해 캐릭터 상태 창에 아이템 착용 여부 표시
                    if (!_selectedSlot.IsEquiped)
                        uiManager.equipSlotManager.EquipItem(_tempItem);
                    else
                        uiManager.equipSlotManager.UnequipItem(_tempItem);

                    // 인벤토리 슬롯에 아이템 착용 여부 전달
                    _selectedSlot.EquipItem(!_selectedSlot.IsEquiped);

                    // 플레이어 장비 착용
                    if (_tempItem.EquipmentData)
                    {
                        bool isPrimary = _tempItem.EquipmentData.isPrimary;

                        // 장비 착용 또는 해제
                        if (_selectedSlot.IsEquiped)
                            GameManager.Instance.Player.equipmentHolder.AssignEquipment(_tempItem.EquipmentData, isPrimary);
                        else
                            GameManager.Instance.Player.equipmentHolder.UnassignEquipment(_tempItem.EquipmentData);
                    }
                }
                // 아이템 사용가능한 경우 아이템 소모하여 사용
                else if (_tempItem.IsConsumable)
                {
                    // 데이터 적용
                    for (int i = 0; i < _itemData.items.Count; i++)
                    {
                        _tempData = _itemData.items[i];

                        // 아이템과 아이템 수량이 동일한 경우 아이템 사용
                        if (_tempData.item == _tempItem && _tempData.amount == amount)
                        {
                            if (_tempData.amount < useAmount)
                            {
                                // TODO: 사용량할 수 있는 아이템 수량이 적음 안내 UI
                                Debug.Log("아이템 수량이 사용할 수량보다 적음");
                                return false;
                            }

                            // 아이템의 수량이 사용량보다 많은 경우
                            if (_tempData.amount > useAmount)
                                _tempData.amount -= useAmount;
                            // 아이템 수량과 사용량이 같은 경우 아이템 삭제
                            else if (_tempData.amount == useAmount)
                            {
                                uiManager.quickSlotManager.UnassignItem(_tempItem);
                                _itemData.RemoveItem(_tempItem, amount);
                            }

                            // 아이템 사용 효과 적용
                            if (_tempData.item.ItemType == ItemType.Food)
                            {
                                // 회복 효과 실행
                                GameManager.Instance.Player.health.RecoverHp((uint)_tempData.item.RecoverHPAmount);

                                // 회복 파티클 효과 재생
                                EffectManager.Instance.PlayEffect(4001);

                                // 효과음 재생
                                AudioManager.Instance.PlayAudio(Strings.Audio_FX_Player_Heal);
                            }
                            break;
                        }
                    }

                    // 인벤토리 적용
                    // 아이템의 수량이 사용량보다 많은 경우
                    if (amount > useAmount)
                        _selectedSlot.SetItemAmount(amount - useAmount);
                    // 아이템 수량과 사용량이 같은 경우 아이템 삭제
                    else if (amount == useAmount)
                        _selectedSlot.Unassign();
                    // TODO: 사용량할 수 있는 아이템 수량이 적음 안내 UI
                    else
                        Debug.Log("아이템 수량이 사용할 수량보다 적음");
                }

                return true;
            }

            return false;
        }

        public bool UnqeuipItem(Item equipmentItem)
        {
            for (int i = 0; i < _slots.Length; i++)
            {
                if (_slots[i].IsAssigned && _slots[i].AssignedItem == equipmentItem
                    && _slots[i].IsEquiped)
                {
                    _slots[i].EquipItem(false);
                    return true;
                }
            }

            return false;
        }
        #endregion

        #region INFO UI
        private void UpdateInfoUI(int slotID = 0)
        {
            // 인베토리 최대 무게
            weightSlider.maxValue = DataManager.Instance.PlayerData.Str * maxWeightPerStr;

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

            // 아이템 상세 UI 꺼짐
            itemSpecificsPanel.Close();

            // 아이템 삭제모드 중인 경우 
            if (_isSelectMode && _itemData.items.Count != 0)
            {
                // 삭제 수량 입력 패드가 켜져있다면 끔
                if (amountInputPanel.gameObject.activeSelf) 
                    amountInputPanel.gameObject.SetActive(false);

                // 할당된 슬롯 중에 삭제하기로 선택된 아이템을 탐색하여 하이라이트로 표시_220509
                for (int i = 0; i < _selectedItemList.Count; i++)
                {
                    var slotID = _selectedItemList[i].SlotID;

                    // 전체 카테고리 탭인 경우
                    if (_selectedTabIndex == 0)
                    {
                        if (_slots[slotID].IsAssigned)
                            _slots[slotID].highlight.SetActive(true);
                    }
                    // 세부 카테고리 탭인 경우
                    else
                    {
                        for (int j = 0; j < _slots.Length; j++)
                        {
                            if (_slots[j].OriginSlotID == slotID)
                            {
                                _slots[j].highlight.SetActive(true);
                                break;
                            }
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
                _tempData = _itemData.items[i];
                _tempItem = _tempData.item;

                if (tabCategory == 1) // 무기 종류 아이템 리스팅
                {
                    if (_tempItem.ItemType == ItemType.Equipment &&
                        _tempItem.EquipmentType == EquipmentType.Weapon)
                        _slots[slotCount++].AssignItem(_tempItem, _tempData.amount, _tempData.isEquiped, false, _tempData.slotID);
                }
                else if (tabCategory == 2) // 방어구 종류 아이템 리스팅
                {
                    if (_tempItem.ItemType == ItemType.Equipment &&
                        _tempItem.EquipmentType != EquipmentType.Weapon)
                        _slots[slotCount++].AssignItem(_tempItem, _tempData.amount, _tempData.isEquiped, false, _tempData.slotID);
                }
                else if (tabCategory == 3) // 사용 가능한 아이템 리스팅
                {
                    if (_tempItem.ItemType != ItemType.Equipment && _tempItem.IsConsumable)
                        _slots[slotCount++].AssignItem(_tempItem, _tempData.amount, false, false, _tempData.slotID);
                }
                else // 그 외 모든 아이템 리스팅
                {
                    if (_tempItem.ItemType != ItemType.Equipment && !_tempItem.IsConsumable)
                        _slots[slotCount++].AssignItem(_tempItem, _tempData.amount, false, false, _tempData.slotID);
                }
            }
        }

        // 카테고리 별로 자동 정렬 함수_220506
        private void AutoListing()
        {
            // 데이터 없으면 return
            if (_itemData.items.Count == 0) return;

            // 모든 슬롯의 아이템 할당 해제
            ClearAllSlots();

            // 슬롯 ID 변수
            int slotNum = 0;

            // 아이템 타입에 따라 각각의 큐에 보관
            for (int i = 0; i < _itemData.items.Count; i++)
            {
                _tempData = _itemData.items[i];
                _itemDataQueues[(int)_tempData.item.ItemType].Enqueue(_tempData);
            }
            // listOrderby를 통해 Enum의 순회할 순서에 따라서 큐에 접근
            foreach (int itemType in listOrderby)
            {
                // 반복문을 돌며 큐에 있는 데이터를 모두 Dequeue
                while (_itemDataQueues[itemType].Count > 0)
                {
                    // 차례대로 데이터의 슬롯 ID를 할당
                    _tempData = _itemDataQueues[itemType].Dequeue();
                    _tempData.slotID = slotNum++;
                }
            }

            // 정렬이 끝난 후에 다시 인벤토리 슬롯에 아이템을 할당
            TabButton(_selectedTabIndex);
        }

        // 모든 슬롯 할당 해제_220505
        private void ClearAllSlots()
        {
            for (int i = 0; i < _slots.Length; i++)
                _slots[i].Unassign();
        }
        #endregion

        #region SELECT MODE & EREASE ITEM
        // 삭제 모드로 전환하는 함수_220506
        private void SwitchEraseMode()
        {
            _isSelectMode = !_isSelectMode;
            eraseMenuPanel.SetActive(_isSelectMode);

            // 모든 하이라이트를 끔
            TurnOffAllHightlight();

            // 선택된 슬롯 정보 초기화
            if (_selectedItemList.Count > 0) 
                _selectedItemList.Clear();

            // 아이템 상세 UI 꺼짐
            itemSpecificsPanel.Close(); 
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
                // 선택 슬롯 초기화
                _selectedSlot = null;

                // 슬롯 데이터 변경(수량 차감 또는 삭제)
                GameManager.Instance.DataManager.DeleteItemData(selectedItem.Item, selectedItem.SlotID, selectedItem.SelectAmount);

                // 퀵슬롯에 할당되어 있다면 할당 해제
                uiManager.quickSlotManager.UnassignItem(selectedItem.Item);

                // 전체 카테고리 탭인 경우
                if (_selectedTabIndex == 0)
                {
                    _selectedSlot = _slots[selectedItem.SlotID];
                }
                else
                {
                    for (int i = 0; i < _slots.Length; i++)
                    {
                        if (_slots[i].IsAssigned && _slots[i].OriginSlotID == selectedItem.SlotID)
                        {
                            _selectedSlot = _slots[i];
                            break;
                        }
                    }
                }

                if (_selectedSlot != null && _selectedSlot.IsAssigned)
                {
                    var currentAmount = _selectedSlot.GetItemAmount();
                    var changeAmount = currentAmount - selectedItem.SelectAmount;
                    // 변경된 수량이 0보다 크면 슬롯의 정보 변경
                    if (changeAmount > 0)
                        _selectedSlot.SetItemAmount(changeAmount);
                    // 변경된 수량이 0이면 슬롯 할당 해제
                    else
                        _selectedSlot.Unassign();
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
            {
                _selectedSlot = _slots[_selectedSlotID];
                _selectedItemList.Add(
                    new SelectItem(
                        _selectedSlot.AssignedItem,
                        _selectedTabIndex == 0 ? _selectedSlotID // 전체 카테고리 탭인 경우 선택된 슬롯ID
                            : _selectedSlot.OriginSlotID, // 그 외 카테고리 탭인 경우 실제 슬롯ID
                        eraseAmount)); // 삭제할 수량
            }
        }
        #endregion

        #region EVENT FUNCTION
        // 슬롯 드래그를 시작한 경우 호출될 이벤트 함수_220503
        private void OnBeginDrag(PointerEventData eventData)
            => IsDragging = true;

        // 슬롯 드랍 시 다른 슬롯과 충돌 확인_220503
        private void OnDragEnd(int slotID, PointerEventData eventData)
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

            // 퀵슬롯 할당한 경우
            _tempQuickSlot = uiManager.quickSlotManager.TryDropSlot(eventData);
            if (_tempQuickSlot != null && 
                _tempQuickSlot.Assign(_slots[slotID].AssignedItem, _slots[slotID].GetItemAmount(), _slots[slotID].IsEquiped))
                return;

            var dropPos = eventData.position;

            for (int i = 0; i < _rectTransforms.Length; i++)
            {
                var rectPos = _rectTransforms[i].position;
                var width = _rectTransforms[i].rect.width * 0.5f;
                var height = _rectTransforms[i].rect.height * 0.5f;

                if (rectPos.x - width <= dropPos.x && dropPos.x <= rectPos.x + width &&
                    rectPos.y - height <= dropPos.y && dropPos.y <= rectPos.y + height)
                {
                    _slots[slotID].SwapSlot(_slots[i], !_isCategoryListing);
                    return;
                }
            }
        }

        // 슬롯을 좌클릭했을 경우 아이템 정보 표시(삭제모드 시 삭제할 아이템 선택)_220506
        private void OnLeftClickSlot(int slotID)
        {
            // 할당되지 않은 슬롯을 좌클릭하거나 드래그를 시작한 경우
            if (slotID == -1)
            {
                // 아이템 상세 UI 꺼짐
                itemSpecificsPanel.Close();
                return; 
            }

            _selectedSlot = _slots[slotID];

            // 선택 모드인 경우
            if (_isSelectMode)
            {
                // 빈 슬롯 클릭 시 리턴
                if (slotID == -1) return;

                // 선택된 슬롯 ID를 변수에 저장
                _selectedSlotID = slotID;

                // 슬롯 아이템의 수량
                var itemAmount = _selectedSlot.GetItemAmount();

                // 이미 선택한 아이템인지 여부 확인
                if (_selectedItemList.Count > 0)
                {
                    for (int i = 0; i < _selectedItemList.Count; i++)
                    {
                        // 이미 선택되었던 슬롯인 경우에는 선택 취소
                        if (_selectedItemList[i].SlotID 
                            == (_selectedTabIndex == 0 ? _selectedSlot.slotID : _selectedSlot.OriginSlotID))
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
                    _selectedSlot = _slots[_selectedSlotID];

                    // 리스트에 추가
                    _selectedItemList.Add(new SelectItem(
                        _selectedSlot.AssignedItem,
                        _selectedTabIndex == 0 ? _selectedSlotID // 전체 카테고리 탭인 경우 선택된 슬롯ID
                            : _selectedSlot.OriginSlotID, 1));// 그 외 카테고리 탭인 경우 실제 슬롯ID
                }
            }
            else
            {
                // 할당된 슬롯을 클릭하였을 경우 세부 정보 패널 표시_220508
                if (slotID >= 0 && _selectedSlot.IsAssigned)
                    itemSpecificsPanel.SetPanel(_selectedSlot.AssignedItem, _selectedSlot.transform.position.x);
                else
                    itemSpecificsPanel.Close(); // 아이템 상세 UI 꺼짐
            }
        }

        // 슬롯을 우클릭했을 경우 장비 아이템인 경우 착용_220512
        private void OnRightClickSlot(int slotNum)
        {
            _selectedSlot = _slots[slotNum];

            // 상점 판매모드인 경우 아이템 즉시 판매 기능 작동
            if (uiManager.shopManager.ShopMode == 1)
            {
                uiManager.shopManager.TrySellItem(_selectedSlot);
                return;
            }
            
            // 아이템 사용 또는 착용,해제
            UseItem();
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

        private void OnDestroy()
        {
            // Event 해제
            for (int i = 0; i < _slots.Length; i++)
            {
                // 등록 이벤트 해제
                _slots[i].OnAssignEvent -= UpdateInfoUI;
                _slots[i].OnBeginDragEvent -= OnBeginDrag;
                _slots[i].OnLeftClickEvent -= OnLeftClickSlot;
                _slots[i].OnLeftClickEvent -= TurnOffAllHightlight;
                _slots[i].OnRightClickEvent -= OnRightClickSlot;
                _slots[i].OnSwapEvent -= SwapSlot;
                _slots[i].OnDragEndEvent -= OnDragEnd;
            }
        }
    }
}