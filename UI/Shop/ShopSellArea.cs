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

        // 마우스가 드랍영역 안에 들어온 경우 발생 이벤트 함수
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_inventoryManager.IsDragging)
                areaImage.color = highlightColor; 
        }

        // 마우스가 드랍영역 밖으로 나간 경우 발생 이벤트 함수
        public void OnPointerExit(PointerEventData eventData)
            => areaImage.color = _defaultColor;

        // 마우스 드랍 경우 발생 이벤트 함수
        public void OnDrop(PointerEventData eventData)
            => areaImage.color = _defaultColor;
    }
}
