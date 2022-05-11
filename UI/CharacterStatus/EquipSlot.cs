using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace SK.UI
{
    internal class EquipSlot : SlotBase
    {
        [SerializeField] private EquipmentType slotEquipmentType;
        private Item assignedItem;

        // 슬롯에 아이템 할당(아이템 정보, 수량, 데이터 변경 여부)_220510
        public bool AssignEquipment(Item item)
        {
            if (!item.equipmentType.Equals(slotEquipmentType))
                return false;

            this.Unassign(); // 슬롯 초기화
            base.Assign(item.itemIcon); // 베이스 슬롯 할당(이미지 변경 등)
            assignedItem = item; // 아이템 정보 할당

            return true;
        }

        // 슬롯 해제하며 아이템 정보 초기화_220510
        public override void Unassign()
        {
            base.Unassign();
            assignedItem = null;
        }

        // 할당된 아이템 전달_220510
        public Item GetAssignedItem() => assignedItem;

        #region Event Function
        // 단순 클릭 시 호출_220510
        public override void OnClick()
        {
            base.OnClick();
            if (IsAssigned)
                OnClickEvent?.Invoke(slotID);
        }
        
        // 마우스를 눌렀을 경우 이벤트 호출_220510
        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);
        }

        // 마우스를 뗐을 경우 이벤트 호출_220510
        public override void OnPointerUp(PointerEventData eventData)
        {
            base.OnPointerUp(eventData);
        }

        // 드래그 시작 시 이벤트 호출_220510
        public override void OnBeginDrag(PointerEventData eventData)
        {
            if (!IsAssigned || !canDrag) return;
            base.OnBeginDrag(eventData);

            // 모든 슬롯의 하이라이트 꺼짐
            OnClickEvent?.Invoke(-1);
        }

        // 드래그 중 이벤트 반복 호출_220510
        public override void OnDrag(PointerEventData eventData)
        {
            if (!IsAssigned) return;
            IsOnClick = false;
            if (!canDrag) return;
            base.OnDrag(eventData);

        }

        // 드래그 종료 시 이벤트 호출_220510
        public override void OnEndDrag(PointerEventData eventData)
        {
            if (!IsAssigned || !canDrag) return;
            base.OnEndDrag(eventData);

        }
        #endregion

    }
}
