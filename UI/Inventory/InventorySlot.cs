using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace SK.UI
{
    /* 작성자: 홍상기
     * 내용: 인벤토리 아이템의 정보를 표시할 슬롯 UI
     * 작성일: 22년 5월 2일
     */

    public class InventorySlot : SlotBase
    {
        [SerializeField] internal Text amountText;
        [SerializeField] internal GameObject highlight;
        [SerializeField] internal GameObject notify;

        // 프로퍼티
        public Item AssignedItem { get; private set; }
        public bool IsEquiped { get; private set; }
        public int OriginSlotID { get; private set; }

        InventorySlot source, tempSlot;
        private uint itemAmount;

        // 슬롯에 아이템 할당(아이템 정보, 수량, 초기화 여부)_220504
        public void AssignItem(Item item, uint amount, bool isEquiped, bool init = false, int originSlotID = -1)
        {
            this.Unassign(); // 슬롯 초기화
            base.Assign(item.ItemIcon); // 베이스 슬롯 할당(이미지 변경 등)
            AssignedItem = item; // 아이템 정보 할당
            itemAmount = amount; // 아이템 수량 할당
            
            // 아이템이 장비인 경우
            if (item.ItemType.Equals(ItemType.Equipment))
                EquipItem(isEquiped); // 아이템 착용 여부_220512
            else
                UpdateAmount(); // 수량 텍스트 표시 업데이트

            // 카테고리 리스팅 시 아이템의 실제 SlotID를 변수에 저장
            OriginSlotID = originSlotID;

            // 슬롯 정보 변경 이벤트 호출
            if (!init) OnAssignEvent?.Invoke(slotID);
        }

        // 슬롯 해제하며 아이템 정보 초기화_220503
        public override void Unassign()
        {
            base.Unassign();

            // 수량 텍스트 초기화
            if (amountText.gameObject.activeSelf) 
                amountText.gameObject.SetActive(false);
            itemAmount = 0;
            OriginSlotID = -1;
            AssignedItem = null;
            IsEquiped = false;

            // 하이라이트 꺼짐
            highlight.SetActive(false);
        }

        // 아이템 수량 전달_220503
        public uint GetItemAmount() => itemAmount;

        // 아이템 수량 변경_220507
        public void SetItemAmount(uint amount)
        {
            // 수량 변경
            itemAmount = amount;
            // 텍스트 표시 업데이트
            UpdateAmount();
        }

        // 슬롯의 장비 아이템을 장착한 경우_220512
        public void EquipItem(bool equip)
        {
            IsEquiped = equip;

            // 착용 텍스트(E) 표시
            if (IsEquiped)
            {
                amountText.gameObject.SetActive(true);
                amountText.text = Strings.ETC_EquipSign;
            }
            // 착용 텍스트(E) 표시 해제
            else            
                amountText.gameObject.SetActive(false);

            UI.UIManager.Instance.quickSlotManager.CheckEquipState(AssignedItem, IsEquiped);
        }

        // 수량 텍스트 표시를 업데이트_220507
        private void UpdateAmount()
        {
            // 아이템 수량이 2개 이상일 경우 텍스트 표시
            if (itemAmount > 1)
            {
                amountText.gameObject.SetActive(true);
                amountText.text = itemAmount.ToString();
            }
            else // 아이템 수량이 1개인 경우 텍스트 표시 해제
            {
                if (amountText.gameObject.activeSelf)
                    amountText.gameObject.SetActive(false);
            }
        }

        #region Event Function
        // 단순 좌클릭 시 호출_220503
        public override void OnLeftClick()
        {
            base.OnLeftClick();
            if (IsAssigned)
            {
                highlight.SetActive(true);
                OnLeftClickEvent?.Invoke(slotID);
            }
            // 할당되지 않은 슬롯을 클릭할 시 모든 하이라이트 꺼짐
            else
                OnLeftClickEvent?.Invoke(-1);
        }

        // 단순 우클릭 시 호출_220511
        public override void OnRightClick()
        {
            base.OnRightClick();
            // 좌클릭 시 장비 아이템인 경우 이벤트 호출
            if (IsAssigned)
                OnRightClickEvent?.Invoke(slotID);
        }

        // 마우스를 눌렀을 경우 이벤트 호출_220503
        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);
        }

        // 마우스를 뗐을 경우 이벤트 호출_220503
        public override void OnPointerUp(PointerEventData eventData)
        {
            base.OnPointerUp(eventData);
        }

        // 드래그 시작 시 이벤트 호출_220503
        public override void OnBeginDrag(PointerEventData eventData)
        {
            if (!IsAssigned || !canDrag) return;
            base.OnBeginDrag(eventData);
            OnBeginDragEvent?.Invoke(eventData);
            // 모든 슬롯의 하이라이트 꺼짐
            OnLeftClickEvent?.Invoke(-1);
        }

        // 드래그 중 이벤트 반복 호출_220503
        public override void OnDrag(PointerEventData eventData)
        {
            if (!IsAssigned) return;
            IsOnLeftClick = false;
            if (!canDrag) return;
            base.OnDrag(eventData);
        }

        // 드래그 종료 시 이벤트 호출_220503
        public override void OnEndDrag(PointerEventData eventData)
        {
            if (!IsAssigned || !canDrag) return;
            base.OnEndDrag(eventData);

        }
        #endregion

        // 인벤토리 스왑 함수_220504
        public override void SwapSlot(SlotBase targetSlot, bool fixData)
        {
            if (!targetSlot.canDrag || targetSlot == this)
                return;

            source = targetSlot as InventorySlot;

            // 타겟 슬롯이 비어있는 경우
            if (!targetSlot.IsAssigned)
            {
                // 빈 슬롯에 현재 정보 할당
                source.AssignItem(AssignedItem, itemAmount, IsEquiped);

                // 현재 슬롯 해제
                this.Unassign();
            }
            else
            {
                // 같은 종류의 아이템인지 확인 후 장비 아이템이 아닌 경우 수량만 증가
                if (this.AssignedItem == source.AssignedItem && this.AssignedItem.ItemType != ItemType.Equipment)
                {
                    var addAmount = source.itemAmount + this.itemAmount;

                    // 같은 종류면 수량 증가
                    source.itemAmount = addAmount;

                    // 타겟 슬롯 아이템 수량 텍스트 업데이트
                    source.UpdateAmount();

                    // 데이터의 아이템 수량 업데이트
                    GameManager.Instance.DataManager.UpdateItemData(source.AssignedItem, source.itemAmount, addAmount);

                    // 현재 슬롯 할당 해제
                    this.Unassign();
                    return;
                }

                // 임시 슬롯 생성
                if (!tempSlot) tempSlot = new InventorySlot();

                // 임시 슬롯에 타겟 슬롯 정보 저장
                tempSlot.AssignedItem = source.AssignedItem;
                tempSlot.itemAmount = source.itemAmount;
                tempSlot.IsEquiped = source.IsEquiped;
                tempSlot.SetSlotID(source.slotID);

                // 타겟 슬롯에 현재 슬롯 정보 할당(데이터 업데이트 X)
                source.AssignItem(AssignedItem, itemAmount, IsEquiped);

                // 임시 슬롯에 저장했던 타겟 슬롯 정보를 현재 슬롯에 할당(데이터 업데이트 X)
                this.AssignItem(tempSlot.AssignedItem, tempSlot.itemAmount, tempSlot.IsEquiped);
            }

            // 현재 슬롯과 타겟 슬롯의 정보 교체로 인한 데이터 업데이트
            if (fixData) OnSwapEvent?.Invoke(this.slotID, source.slotID);
        }
    }
}
