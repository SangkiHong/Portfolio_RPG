using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace SK.UI
{
    public class InventorySlot : SlotBase
    {
        [SerializeField] internal Text amountText;
        [SerializeField] internal GameObject highlight;
        [SerializeField] internal GameObject notify;

        InventorySlot source, tempSlot;
        private Item assignedItem;
        private uint itemAmount;

        // 슬롯에 아이템 할당(아이템 정보, 수량, 데이터 변경 여부)_220504
        public void AssignItem(Item item, uint amount = 1, bool addData = false)
        {
            this.Unassign(); // 슬롯 초기화
            base.Assign(item.itemIcon); // 베이스 슬롯 할당(이미지 변경 등)
            assignedItem = item; // 아이템 정보 할당
            itemAmount = amount; // 아이템 수량 할당

            // 텍스트 표시 업데이트
            UpdateAmount();

            // 슬롯 정보 변경 이벤트 호출
            if (addData) OnAssignEvent?.Invoke(this, this.slotID, amount); 
        }

        // 슬롯 해제하며 아이템 정보 초기화_220503
        public override void Unassign()
        {
            base.Unassign();

            // 수량 텍스트 초기화
            if (amountText.gameObject.activeSelf) 
                amountText.gameObject.SetActive(false);
            itemAmount = 0;
            assignedItem = null;

            // 하이라이트 꺼짐
            highlight.SetActive(false);
        }

        // 할당된 아이템 전달_220504
        public Item GetAssignedItem() => assignedItem;

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
        // 단순 클릭 시 호출_220503
        public override void OnClick()
        {
            base.OnClick();
            if (IsAssigned)
            {
                highlight.SetActive(true);
                OnClickEvent?.Invoke(slotID);
            }
            // 할당되지 않은 슬롯을 클릭할 시 모든 하이라이트 꺼짐
            else
                OnClickEvent?.Invoke(-1);
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

            // 모든 슬롯의 하이라이트 꺼짐
            OnClickEvent?.Invoke(-1);
        }

        // 드래그 중 이벤트 반복 호출_220503
        public override void OnDrag(PointerEventData eventData)
        {
            if (!IsAssigned) return;
            IsOnClick = false;
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
                source.AssignItem(assignedItem, itemAmount);

                // 현재 슬롯 해제
                this.Unassign();
            }
            else
            {
                // 같은 종류의 아이템인지 확인
                if (this.assignedItem == source.assignedItem)
                {
                    var addAmount = source.itemAmount + this.itemAmount;

                    // 같은 종류면 수량 증가
                    source.itemAmount = addAmount;

                    // 데이터의 아이템 수량 업데이트
                    GameManager.Instance.DataManager.UpdateItemData(source.slotID, addAmount);

                    // 현재 슬롯 할당 해제
                    this.Unassign();
                    return;
                }

                // 임시 슬롯 생성
                if (!tempSlot) tempSlot = new InventorySlot();

                // 임시 슬롯에 타겟 슬롯 정보 저장
                tempSlot.assignedItem = source.assignedItem;
                tempSlot.itemAmount = source.itemAmount;
                tempSlot.slotID = source.slotID;

                // 타겟 슬롯에 현재 슬롯 정보 할당(데이터 업데이트 X)
                source.AssignItem(assignedItem, itemAmount);

                // 임시 슬롯에 저장했던 타겟 슬롯 정보를 현재 슬롯에 할당(데이터 업데이트 X)
                this.AssignItem(tempSlot.assignedItem, tempSlot.itemAmount);
            }

            // 현재 슬롯과 타겟 슬롯의 정보 교체로 인한 데이터 업데이트
            if (fixData) OnSwapEvent?.Invoke(this.slotID, source.slotID);
        }
    }
}
