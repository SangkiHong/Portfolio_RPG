using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using SK.Data;
using Object = UnityEngine.Object;
using System.Collections.Generic;

namespace SK.UI
{
    internal struct EraseItem
    {
        public Item item;
        public uint itemAmount;
        public uint eraseAmount;
    }

    public class InventoryManager : MonoBehaviour
    {
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
        [SerializeField] private GameObject eraseMenuPanel;
        [SerializeField] private EraseInputPanel eraseAmountPanel;

        [Space]
        [SerializeField] private Button autoListingButton;
        [SerializeField] private ItemType[] listOrderby;

        [Header("Item Specifics Panel")]
        [SerializeField] private ItemSpecificsPanel itemSpecificsPanel;

        // 삭제 모드 시 삭제할 슬롯 저장용 리스트
        private List<EraseItem> _eraseItemList;
        // 자동 정렬 시 임시로 데이터를 담을 큐 배열
        private Queue<ItemData>[] _itemDataQueues;
        private InventorySlot[] _slots;
        private RectTransform[] _rectTransforms;

        private PlayerItemData _itemData;
        private InventorySlot _selectedSlot;
        private ItemData _tempData;

        private Vector3 _focusLocalPos;

        private bool _isCategoryListing, _isEraseMode;
        private int _selectedSlotID;

        private void Awake()
        {
            // 초기화
            _slots = slotParent.GetComponentsInChildren<InventorySlot>();
            _rectTransforms = new RectTransform[_slots.Length];
            _eraseItemList = new List<EraseItem>();
            _itemDataQueues = new Queue<ItemData>[Enum.GetValues(typeof(ItemType)).Length];
            for (int i = 0; i < _itemDataQueues.Length; i++)            
                _itemDataQueues[i] = new Queue<ItemData>();
            
            _focusLocalPos = tabFocus.localPosition;
                        
            // slot ID 할당
            for (int i = 0; i < _slots.Length; i++)
            {
                _slots[i].slotID = i;
                _slots[i].OnClickEvent += OnSelectSlot; // 슬롯을 단순 클릭 시 발동 이벤트 등록
                _slots[i].OnAssignEvent += NewAssignItem; // 슬롯에 새로운 아이템 할당 시 발동 이벤트 등록
                _slots[i].OnSwapEvent += SwapSlot; // 슬롯 상태 변경 시 발동 이벤트 등록
                _slots[i].OnDragEndEvent += CheckSlotCollision; // 슬롯 드랍 시 발동 이벤트 등록

                _rectTransforms[i] = _slots[i].transform as RectTransform;
            }

            // Tab 버튼 이벤트 할당
            for (int i = 0; i < tabs.Length; i++)
            {
                int tempIndex = i; // Closuer problem 으로 인해 임시 인덱스 값을 생성
                // 탭 인덱스에 따른 아이템 리스팅
                tabs[i].onClick.AddListener(delegate { TabButton(tempIndex); });
            }

            // 아이템 삭제 모드 버튼 이벤트 할당
            button_EraseMode.onClick.AddListener(SwitchEraseMode);

            // 선택한 아이템을 모두 삭제하는 버튼 이벤트 할당
            button_Erase.onClick.AddListener(EraseSelectedItem);

            // 삭제할 아이템 선택을 모두 취소하는 버튼 이벤트 할당
            button_CancelSelection.onClick.AddListener(CancelSelection);

            // 자동 정렬 버튼 이벤트 할당
            autoListingButton.onClick.AddListener(AutoListing);

            // 보유 아이템 데이터를 토대로 인벤토리에 슬롯 정보 불러오기(초기화)_220503
            Invoke("Initialize", 0.3f);
        }

        private void Initialize()
        {
            LoadSlotData();
            uiManager.window_Invenroty.alpha = 0;
            uiManager.window_Invenroty.blocksRaycasts = false;
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
                    _slots[_itemData.items[i].slotID].AssignItem(_itemData.items[i].item, _itemData.items[i].amount); 
                // 아이템이 슬롯에 할당되지 않았던 경우 빈 슬롯에 아이템 할당
                else
                {
                    for (int j = 0; j < _slots.Length; j++)
                    {
                        if (!_slots[j].IsAssigned) // 할당되지 않은 빈 슬롯인지 체크
                        {
                            _slots[j].AssignItem(_itemData.items[i].item, _itemData.items[i].amount); // 빈 슬롯에 아이템 할당
                            _itemData.items[i].slotID = _slots[j].slotID; // 아이템 데이터에 슬롯 ID 지정
                            break;
                        }    
                    }
                }
            }
            UpdateInfoUI();
        }

        /// <summary>
        /// 새로운 슬롯 할당_220504
        /// </summary>
        /// <param name="slot">슬롯 정보</param>
        /// <param name="slotID">슬롯 ID</param>
        /// <param name="amount">아이템 수량</param>
        private void NewAssignItem(Object slot, int slotID, uint amount)
        {
            _selectedSlot = slot as InventorySlot;
            GameManager.Instance.DataManager.AddNewItemData(_selectedSlot);
            UpdateInfoUI();
        }

        // 두 슬롯 정보 스왑_220504
        private void SwapSlot(int aSlotID, int bSlotID)
        {
            GameManager.Instance.DataManager.SwapSlot(aSlotID, bSlotID);
            UpdateInfoUI();
        }
        #endregion

        #region INFO UI
        private void UpdateInfoUI()
        {
            // 인베토리 최대 무게
            weightSlider.maxValue = GameManager.Instance.Player.playerData.Str * maxWeightPerStr;

            // 아이템 총 무게 변수
            float totalWeight = 0;
            for (int i = 0; i < _itemData.items.Count; i++)            
                totalWeight += _itemData.items[i].item.weight;

            // 아이템 총 무게 슬라이드에 표시
            weightSlider.value = totalWeight;

            // 아이템 갯수 텍스트 표시
            text_CurrentItemAmount.text = _itemData.items.Count.ToString();
        }
        #endregion

        #region LISTING & TAB BUTTON
        // 카테고리 별로 아이템 정렬(탭 인덱스)_220506
        private void TabButton(int tabIndex)
        {
            // 아이템 리스팅 함수 호출
            LoadSlotByCategory(tabIndex); 

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
            if (_isEraseMode && _itemData.items.Count != 0)
            {
                // 삭제 수량 입력 패드가 켜져있다면 끔
                if (eraseAmountPanel.gameObject.activeSelf) 
                    eraseAmountPanel.gameObject.SetActive(false);

                // 할당된 슬롯 중에 삭제하기로 선택된 아이템을 탐색하여 하이라이트로 표시_220509
                for (int i = 0; i < _slots.Length; i++)
                {
                    if (_slots[i].IsAssigned)
                    {
                        for (int j = 0; j < _eraseItemList.Count; j++)
                        {
                            
                            if (_slots[i].GetAssignedItem().Equals(_eraseItemList[j].item) &&
                                _slots[i].GetItemAmount() == _eraseItemList[j].itemAmount)
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
                    if (_itemData.items[i].item.itemType == ItemType.Equipment &&
                        _itemData.items[i].item.equipmentType == EquipmentType.Weapon)
                        _slots[slotCount++].AssignItem(_itemData.items[i].item, _itemData.items[i].amount);
                }
                else if (tabCategory == 2) // 방어구 종류 아이템 리스팅
                {
                    if (_itemData.items[i].item.itemType == ItemType.Equipment &&
                        _itemData.items[i].item.equipmentType != EquipmentType.Weapon)
                        _slots[slotCount++].AssignItem(_itemData.items[i].item, _itemData.items[i].amount);
                }
                else if (tabCategory == 3) // 사용 가능한 아이템 리스팅
                {
                    if (_itemData.items[i].item.itemType != ItemType.Equipment && _itemData.items[i].item.isConsumable)
                        _slots[slotCount++].AssignItem(_itemData.items[i].item, _itemData.items[i].amount);
                }
                else // 그 외 모든 아이템 리스팅
                {
                    if (_itemData.items[i].item.itemType != ItemType.Equipment && !_itemData.items[i].item.isConsumable)
                        _slots[slotCount++].AssignItem(_itemData.items[i].item, _itemData.items[i].amount);
                }
            }
        }

        // 카테고리 별로 자동 정렬 함수_220506
        private void AutoListing()
        {
            // 데이터 없으면 return
            if (_itemData.items.Count == 0) return;

            // 인벤토리 슬롯 비우기
            ClearAllSlots();

            // 슬롯 ID 변수
            int slotNum = 0;
            // 큐에 

            // 아이템 타입에 따라 각각의 큐에 보관
            for (int i = 0; i < _itemData.items.Count; i++)            
                _itemDataQueues[(int)_itemData.items[i].item.itemType].Enqueue(_itemData.items[i]);

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
                    _slots[slotNum++].AssignItem(_tempData.item, _tempData.amount);
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

        #region ERASE MODE
        // 삭제 모드, 일반 모드로 전환하는 함수_220506
        private void SwitchEraseMode()
        {
            _isEraseMode = !_isEraseMode;
            eraseMenuPanel.SetActive(_isEraseMode);

            // 모든 하이라이트를 끔
            TurnOffAllHightlight();

            // 선택된 슬롯 정보 초기화
            if (_eraseItemList.Count > 0) _eraseItemList.Clear();

            // 아이템 정보 끔
            itemSpecificsPanel.gameObject.SetActive(false);
        }

        // 삭제하려고 선택했던 것을 취소하는 함수_220506
        private void CancelSelection()
        {
            // 삭제할 아이템 슬롯 정보를 초기화
            _eraseItemList.Clear();
            
            // 선택되었던 슬롯 하이라이트를 모두 끔
            for (int i = 0; i < _slots.Length; i++)            
                _slots[i].highlight.SetActive(false);            
        }

        // 선택한 아이템을 모두 삭제하는 함수_220506
        private void EraseSelectedItem()
        {
            foreach (var eraseItem in _eraseItemList)
            {
                uint changedAmount = eraseItem.itemAmount - eraseItem.eraseAmount;

                // 슬롯 데이터 변경(수량이 0이면 삭제, 0보다 많으면 수량 감소)
                if (changedAmount > 0)
                    GameManager.Instance.DataManager.UpdateItemData(eraseItem.item, eraseItem.itemAmount, changedAmount);
                else
                    GameManager.Instance.DataManager.DeleteItemData(eraseItem.item, eraseItem.itemAmount);

                for (int i = 0; i < _slots.Length; i++)
                {
                    if (_slots[i].IsAssigned && _slots[i].GetAssignedItem().Equals(eraseItem.item) && _slots[i].GetItemAmount() == eraseItem.itemAmount)
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

            _eraseItemList.Clear(); // 리스트 초기화
            TurnOffAllHightlight(); // 선택된 모든 슬롯의 하이라이트 꺼짐
            SwitchEraseMode(); // 삭제 모드에서 일반 모드로 변경
            UpdateInfoUI(); // 전체적인 인벤토리 UI 업데이트
        }

        // 입력 패드 화면을 통해 삭제할 아이템 수량을 전달받는 함수_220507
        public void ConfirmEraseAmount(uint eraseAmount)
        {
            // 삭제할 수량이 0이면 삭제 취소
            if (eraseAmount == 0) 
                _slots[_selectedSlotID].highlight.SetActive(false);
            else // 리스트에 슬롯 ID, 삭제 수량 추가
                _eraseItemList.Add(new EraseItem()
                {
                    item = _slots[_selectedSlotID].GetAssignedItem(),
                    eraseAmount = eraseAmount,
                    itemAmount = _slots[_selectedSlotID].GetItemAmount()
                });
        }
        #endregion

        #region EVENT FUNCTION
        // 슬롯 드랍 시 다른 슬롯과 충돌 확인_220503
        private void CheckSlotCollision(int slotID, PointerEventData eventData)
        {
            // 아이템 삭제 모드 시 드래그 앤 드랍 불가
            if (_isEraseMode) return;

            for (int i = 0; i < _rectTransforms.Length; i++)
            {
                if (_rectTransforms[i].position.x - _rectTransforms[i].rect.width * 0.5f <= eventData.position.x && eventData.position.x <= _rectTransforms[i].position.x + _rectTransforms[i].rect.width * 0.5f &&
                   _rectTransforms[i].position.y - _rectTransforms[i].rect.height * 0.5f <= eventData.position.y && eventData.position.y <= _rectTransforms[i].position.y + _rectTransforms[i].rect.height * 0.5f)
                {
                    _slots[slotID].SwapSlot(_slots[i], !_isCategoryListing);
                }
            }
        }

        // 슬롯을 단순 클릭했을 경우 아이템 정보 표시(삭제모드 시 삭제할 아이템 선택)_220506
        private void OnSelectSlot(int slotNum)
        {
            // 할당된 슬롯을 클릭하였을 경우 세부 정보 패널 표시_220508
            if (slotNum >= 0 && _slots[slotNum].IsAssigned)
                itemSpecificsPanel.SetPanel(_slots[slotNum].GetAssignedItem(), uiManager.window_Invenroty.transform.position.x);
            else
                itemSpecificsPanel.gameObject.SetActive(false);

            // 삭제 모드가 아닌 경우 해당 슬롯 외에 다른 슬롯 하이라이트 꺼짐
            if (!_isEraseMode)
            {
                TurnOffAllHightlight(slotNum);
            }
            // 삭제 모드인 경우
            else
            {
                // 빈 슬롯 클릭 시 리턴
                if (slotNum == -1) return;

                // 선택된 슬롯 ID를 변수에 저장
                _selectedSlotID = slotNum;

                // 슬롯 아이템의 수량
                var itemAmount = _slots[slotNum].GetItemAmount();

                // 이미 선택한 아이템인지 여부 확인
                if (_eraseItemList.Count > 0)
                {
                    for (int i = 0; i < _eraseItemList.Count; i++)
                    {
                        // 이미 선택되었던 경우에는 선택을 취소
                        if (_eraseItemList[i].item.Equals(_slots[slotNum].GetAssignedItem()) &&
                            _eraseItemList[i].itemAmount == itemAmount)
                        {
                            _eraseItemList.RemoveAt(i);
                            _slots[slotNum].highlight.SetActive(false);
                            return;
                        }
                    }
                }

                // 선택이 안되었던 경우에는 선택
                if (itemAmount > 1) // 다중 수량일 경우
                {
                    // 삭제할 수량 입력 패드 켜짐
                    eraseAmountPanel.SetPaenl(itemAmount);
                }
                else // 단일 수량일 경우
                {
                    // 리스트에 추가
                    _eraseItemList.Add(new EraseItem()
                    {
                        item = _slots[_selectedSlotID].GetAssignedItem(),
                        eraseAmount = itemAmount,
                        itemAmount = itemAmount
                    });
                }
            }      
        }

        // 모든 슬롯의 하이라이트를 끔_220506
        private void TurnOffAllHightlight(int exceptSlot = -1)
        {
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
                _slots[i].OnClickEvent -= OnSelectSlot;
                _slots[i].OnAssignEvent -= NewAssignItem;
                _slots[i].OnSwapEvent -= SwapSlot;
                _slots[i].OnDragEndEvent -= CheckSlotCollision;
            }
        }
    }
}