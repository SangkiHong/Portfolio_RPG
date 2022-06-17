using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Events;

namespace SK.UI
{
    /* �ۼ���: ȫ���
     * ����: ���� �������� ������ ǥ���� ���� UI
     * �ۼ���: 22�� 6�� 14��
     */

    public class ShopItemSlot : SlotBase
    {
        // ������ �巡�� ���� ��� �߻��� �̺�Ʈ
        public UnityAction<PointerEventData> OnDragEvent;
        // ������ Ŭ���� ��� �߻��� �̺�Ʈ
        public UnityAction<int> OnSelectSlotEvent;

        [SerializeField] private Image image_slotBaseImage;
        [SerializeField] private Text textField_ItemName;
        [SerializeField] private Text textField_ItemPrice;

        // ������Ƽ
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

        // ���Կ� ������ ������ �Ҵ�
        public void Assign(Item item)
        {
            // ���� ������ ���ӿ�����Ʈ�� ��
            _thisGameObject.SetActive(true);
            // ������ ������ ������ ����
            AssignedItem = item;
            base.Assign(AssignedItem.ItemIcon);

            // ������ �̸��� Text�� ǥ��
            textField_ItemName.text = AssignedItem.ItemName;
            // ������ ������ Text�� ǥ��
            textField_ItemPrice.text = string.Format("{0:#,0}", AssignedItem.ItemPrice);
        }

        // ������ �Ҵ�� ������ ����
        public override void Unassign()
        {
            base.Unassign();
            textField_ItemName.text = string.Empty;
            textField_ItemPrice.text = string.Empty;
            // ���� ������ ���ӿ�����Ʈ�� ��
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
            // �ܼ� Ŭ���� ���
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
