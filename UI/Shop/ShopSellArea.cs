using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace SK.UI
{
    public class ShopSellArea : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IDropHandler
    {
        [SerializeField] private Image areaImage;
        [SerializeField] private Color highlightColor;

        private InventoryManager _inventoryManager;
        private Color _defaultColor;
        private bool _init;

        private void Start()
        {
            _inventoryManager = GameManager.Instance.UIManager.inventoryManager;
            _defaultColor = areaImage.color;
            _init = true;
        }

        private void OnEnable()
        {
            if (_init) areaImage.color = _defaultColor; 
        }

        // ���콺�� ������� �ȿ� ���� ��� �߻� �̺�Ʈ �Լ�
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_inventoryManager.IsDragging)
                areaImage.color = highlightColor; 
        }

        // ���콺�� ������� ������ ���� ��� �߻� �̺�Ʈ �Լ�
        public void OnPointerExit(PointerEventData eventData)
            => areaImage.color = _defaultColor;

        // ���콺 ��� ��� �߻� �̺�Ʈ �Լ�
        public void OnDrop(PointerEventData eventData)
            => areaImage.color = _defaultColor;
    }
}
