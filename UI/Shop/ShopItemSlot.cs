using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Events;

namespace SK.UI
{
    /* 작성자: 홍상기
     * 내용: 상점 아이템의 정보를 표시할 슬롯 UI
     * 작성일: 22년 6월 14일
     */

    public class ShopItemSlot : SlotBase
    {
        // 슬롯을 드래그 중인 경우 발생할 이벤트
        public UnityAction<PointerEventData> OnDragEvent;
        // 슬롯을 클릭한 경우 발생할 이벤트
        public UnityAction<int> OnSelectSlotEvent;

        [SerializeField] private Image image_slotBaseImage;
        [SerializeField] private Text textField_ItemName;
        [SerializeField] private Text textField_ItemPrice;

        // 프로퍼티
        public Item AssignedItem { get; private set; }

        private GameObject _thisGameObject;
        private Color _defaultColor, _highlightColor;

        private bool _isClick;

        public void Initialize(Color highlightColor)
        {
            _thisGameObject = gameObject;
            _defaultColor = image_slotBaseImage.color;
            _highlightColor = highlightColor;
        }

        // 슬롯에 아이템 정보를 할당
        public void Assign(Item item)
        {
            // 현재 슬롯의 게임오브젝트를 끔
            _thisGameObject.SetActive(true);
            // 변수에 아이템 정보를 저장
            AssignedItem = item;
            base.Assign(AssignedItem.ItemIcon);

            // 아이템 이름을 Text에 표시
            textField_ItemName.text = AssignedItem.ItemName;
            // 아이템 가격을 Text에 표시
            textField_ItemPrice.text = string.Format("{0:#,0}", AssignedItem.ItemPrice);
        }

        // 슬롯의 할당된 정보를 해제
        public override void Unassign()
        {
            base.Unassign();
            textField_ItemName.text = string.Empty;
            textField_ItemPrice.text = string.Empty;
            // 현재 슬롯의 게임오브젝트를 켬
            _thisGameObject.SetActive(false);
        }

        public void Highlight(bool isOn)
        {
            if (isOn)
                image_slotBaseImage.color = _highlightColor;
            else
                image_slotBaseImage.color = _defaultColor;
        }

        #region Event
        public override void OnPointerDown(PointerEventData eventData)
        {
            _isClick = true;
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            // 단순 클릭한 경우
            if (_isClick)
            {
                OnSelectSlotEvent?.Invoke(slotID);
            }
        }

        public override void OnBeginDrag(PointerEventData eventData)
        {
            _isClick = false;
            OnBeginDragEvent?.Invoke(eventData);
        }

        public override void OnDrag(PointerEventData eventData)
        {
            OnDragEvent?.Invoke(eventData);
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            OnDragEndEvent?.Invoke(0, eventData);
        }
        #endregion
    }
}
