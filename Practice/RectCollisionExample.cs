using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace SK.Practice
{
    public class RectCollisionExample : MonoBehaviour, IPointerDownHandler, IDragHandler
    {
        [SerializeField] private RectTransform rectTransform;

        public bool IsInRect(Vector2 pos)
        {
            // 중심으로 Pivot으로 했을 경우
            return rectTransform.position.x - rectTransform.rect.width * 0.5f <= pos.x && pos.x <= rectTransform.position.x + rectTransform.rect.width * 0.5f &&
                   rectTransform.position.y - rectTransform.rect.height * 0.5f <= pos.y && pos.y <= rectTransform.position.y + rectTransform.rect.height * 0.5f;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (IsInRect(eventData.position))
            {
                Debug.Log("Pointer Down");
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            
        }
    }
}