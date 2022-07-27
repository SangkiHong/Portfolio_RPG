using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SK.UI;
using System;

namespace SK.Loot
{
    /* 작성자: 홍상기
     * 내용: 전리품(드랍) 아이템 목록에 대한 정보 관리하며 UI에 표시하는 클래스
     * 작성일: 22년 7월 20일
     */

    public class LootItemListPanel : MonoBehaviour
    {
        [SerializeField] private Transform contentParent;
        [SerializeField] private GameObject slotPrefab;

        [Header("UI")]
        [SerializeField] private Button btn_SelectLooting;

        // 대기 슬롯 큐
        private Queue<LootItemSlot> _waitingSlotQueue = new Queue<LootItemSlot>();
        // 사용 중인 슬롯 딕셔너리(키: 인스턴스ID, 값: 슬롯)
        private Dictionary<int, LootItemSlot> _usingSlotDic = new Dictionary<int, LootItemSlot>();

        private InventoryManager _inventoryManager;
        private LootItems _assignedLootItems;
        private LootItemSlot _selectedSlot;
        private GameObject _tempSlot;

        private int[] _slotIdBuffer = new int[10];
        private int _selectSlotID;

        public bool IsOpen { get; private set; }

        // 전리품 리스트 패널 열기
        public void Show(LootItems lootItems)
        {
            // 컴포넌트 정보 가져오기
            if (_inventoryManager == null)
                _inventoryManager = UIManager.Instance.inventoryManager;

            if (!IsOpen)
            {
                IsOpen = true;
                // 전리품 아이템 컴포넌트를 변수에 저장
                _assignedLootItems = lootItems;

                // 슬롯 여유분 확인하여 모자라면 추가 생성
                if (_waitingSlotQueue.Count < lootItems.LootItemDic.Count)
                {
                    int createAmount = lootItems.LootItemDic.Count - _waitingSlotQueue.Count;

                    for (int i = 0; i < createAmount; i++)
                        CreateSlot();
                }

                // 슬롯에 데이터 할당 후 배치
                int instanceID = 0;
                foreach (KeyValuePair<Item, int> lootItem in lootItems.LootItemDic)
                {
                    if (_waitingSlotQueue.Count > 0)
                    {
                        // 대기 중인 슬롯을 꺼내서 변수에 할당
                        _selectedSlot = _waitingSlotQueue.Dequeue();

                        // 슬롯에 데이터 할당
                        _selectedSlot.Assign(lootItem.Key, lootItem.Value);

                        // 슬롯 이벤트 함수 할당
                        _selectedSlot.onSelect += OnSelectSlot;
                        _selectedSlot.onLooting += OnLootItem;

                        // 사용 중인 딕셔너리에 추가
                        instanceID = _selectedSlot.GetInstanceID();
                        if (!_usingSlotDic.ContainsKey(instanceID))
                            _usingSlotDic.Add(instanceID, _selectedSlot);
                    }
                    else
                        Debug.Log("전리품 슬롯의 여유가 없습니다.");
                }

                // 초기화
                btn_SelectLooting.interactable = false;
                gameObject.SetActive(true);
                transform.SetAsLastSibling();
            }
        }

        // 패널 끔
        public void Hide()
        {
            if (IsOpen)
            {
                IsOpen = false;

                // 사용 중인 슬롯 수거
                if (_usingSlotDic.Count > 0)
                {
                    // 버퍼 비우기
                    Array.Clear(_slotIdBuffer, 0, _slotIdBuffer.Length);

                    // 버퍼 크기 확인 후 작은 경우 크기 증가
                    if (_slotIdBuffer.Length < _usingSlotDic.Count)
                        _slotIdBuffer = new int[_usingSlotDic.Count];

                    // 버퍼 인덱스
                    int index = 0;

                    // 슬롯을 순환하며 키 값을 버퍼에 저장
                    foreach (KeyValuePair<int, LootItemSlot> lootSlot in _usingSlotDic)
                        _slotIdBuffer[index++] = lootSlot.Key;

                    for (int i = 0; i < _slotIdBuffer.Length; i++)
                    {
                        // 버퍼 데이터 비워진 부분에서 루프 탈출
                        if (_slotIdBuffer[i] == 0) break;

                        _selectedSlot = _usingSlotDic[_slotIdBuffer[i]];
                        // 모든 사용 중인 슬롯을 할당해제
                        _selectedSlot.UnAssign();
                        // 대기 슬롯 큐에 추가
                        _waitingSlotQueue.Enqueue(_selectedSlot);
                    }

                    // 사용 중인 슬롯 딕셔너리 비우기
                    _usingSlotDic.Clear();
                }

                // 초기화
                _selectSlotID = 0;
                _assignedLootItems = null;
                gameObject.SetActive(false);
            }
        }

        // 슬롯 오브젝트 생성
        private void CreateSlot()
        {
            // 슬롯 오브젝트 추가 생성
            _tempSlot = Instantiate(slotPrefab, contentParent);
            // 큐에 추가
            _waitingSlotQueue.Enqueue(_tempSlot.GetComponent<LootItemSlot>());
            // 초기화::오브젝트 끄기
            _tempSlot.SetActive(false);
        }

        // 선택한 아이템 획득 버튼
        public void LootButton()
            => LootSelectItem();

        // 선택한 전리품 아이템 획득
        private bool LootSelectItem(bool isLoop = false)
        {
            // 사용중인 슬롯 딕셔너리에서 슬롯 정보 가져옴
            _selectedSlot = _usingSlotDic[_selectSlotID];

            // 아이템 획득 가능 상태 확인
            if (_inventoryManager.CanTakeItem(_selectedSlot.AssignedItem))
            {
                // 전리품 아이템 획득
                _assignedLootItems.TakeItem(_selectedSlot.AssignedItem);
            }
            // 획득 불가 상태인 경우
            else
            {
                UIManager.Instance.confirmPanel.ShowInfo(InfoType.NotEnoughSlot);
                return false;
            }

            // 슬롯 할당 해제
            _selectedSlot.UnAssign();

            // 대기 슬롯 큐에 추가
            _waitingSlotQueue.Enqueue(_selectedSlot);

            // 딕셔너리에서 제거(모든 아이템 순환 중인 경우 예외)
            if (!isLoop) _usingSlotDic.Remove(_selectSlotID);

            // 초기화
            _selectSlotID = 0;
            btn_SelectLooting.interactable = false;

            // 사용 중인 슬롯이 없는 경우 패널 닫기
            if (_usingSlotDic.Count == 0) Hide();

            return true;
        }

        // 모든 전리품 아이템 획득
        public void LootAll()
        {
            // 버퍼 비우기
            Array.Clear(_slotIdBuffer, 0, _slotIdBuffer.Length);

            // 버퍼 크기 확인 후 작은 경우 크기 증가
            if (_slotIdBuffer.Length < _usingSlotDic.Count)
                _slotIdBuffer = new int[_usingSlotDic.Count];

            // 버퍼 인덱스
            int index = 0;

            // 슬롯을 순환하며 키 값을 버퍼에 저장
            foreach (KeyValuePair<int, LootItemSlot> lootSlot in _usingSlotDic)
                _slotIdBuffer[index++] = lootSlot.Key;

            // 버퍼를 순환하며 아이템 획득
            for (int i = 0; i < _slotIdBuffer.Length; i++)
            {
                // 버퍼 데이터 비워진 부분에서 루프 탈출
                if (_slotIdBuffer[i] == 0) break;

                _selectedSlot = _usingSlotDic[_slotIdBuffer[i]];

                if (_selectedSlot == null || _selectedSlot.AssignedItem == null)
                    return;

                // 아이템 획득 가능 상태 확인
                if (_inventoryManager.CanTakeItem(_selectedSlot.AssignedItem))
                {
                    // 전리품 아이템 획득
                    _assignedLootItems.TakeItem(_selectedSlot.AssignedItem);
                }
                // 획득 불가 상태인 경우
                else
                {
                    UIManager.Instance.confirmPanel.ShowInfo(InfoType.NotEnoughSlot);
                    return;
                }

                // 대기 슬롯 큐에 추가
                _waitingSlotQueue.Enqueue(_selectedSlot);

                // 슬롯 할당 해제
                _selectedSlot.UnAssign();

                // 사용 중인 슬롯 딕셔너리에서 제거
                _usingSlotDic.Remove(_slotIdBuffer[i]);
            }

            // 패널 닫기
            Hide();
        }

        #region Event Method
        private void OnSelectSlot(int instanceID)
        {
            // 이전 선택이 있는 경우 선택 해제
            if (_selectSlotID != 0)
                _usingSlotDic[_selectSlotID].SelectControl(false);

            _selectSlotID = instanceID;
            btn_SelectLooting.interactable = true;
        }

        private void OnLootItem(int instanceID)
        {
            _selectSlotID = instanceID;
            LootSelectItem();
        }
        #endregion
    }
}