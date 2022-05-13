using UnityEngine;
using UnityEngine.EventSystems;

namespace SK.UI
{
    internal class EquipSlot : SlotBase
    {
        public EquipmentType slotEquipmentType;

        [SerializeField] internal bool isPrimaryWeapon;

        public Item AssignedItem { get; private set; }

        // 슬롯에 아이템 할당(아이템 정보, 수량, 데이터 변경 여부)_220510
        public bool AssignEquipment(Item item)
        {
            if (!item.equipmentType.Equals(slotEquipmentType))
                return false;

            this.Unassign(); // 슬롯 초기화
            base.Assign(item.itemIcon); // 베이스 슬롯 할당(이미지 변경 등)
            AssignedItem = item; // 아이템 정보 할당

            return true;
        }

        // 슬롯 해제하며 아이템 정보 초기화_220510
        public override void Unassign()
        {
            base.Unassign();
            AssignedItem = null;
        }

        #region Event Function
        // 좌클릭 시 이벤트 호출(아이템 세부 정보 패널 열기)_220510
        public override void OnLeftClick()
        {
            if (IsAssigned)
                OnLeftClickEvent?.Invoke(slotID);
        }

        // 우클릭 시 이벤트 호출(아이템 착용 해제)_220511
        public override void OnRightClick()
        {
            if (IsAssigned)
                OnRightClickEvent?.Invoke(slotID);
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

        // 드래그 시작 시 이벤트 호출_220512
        public override void OnBeginDrag(PointerEventData eventData)
        {
            IsOnLeftClick = false;
            IsOnRightClick = false;
            if (!IsAssigned || !canDrag) return;
        }

        // 드래그 중 이벤트 반복 호출_220512
        public override void OnDrag(PointerEventData eventData)
        {
            if (!IsAssigned) return;
        }

        // 드래그 종료 시 이벤트 호출_220512
        public override void OnEndDrag(PointerEventData eventData)
        {
            if (!canDrag) return;
        }
        #endregion
    }
}
