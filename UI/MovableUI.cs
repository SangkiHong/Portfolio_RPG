using UnityEngine;
using UnityEngine.EventSystems;

namespace SK.UI
{
    public class MovableUI : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler
    {
        [SerializeField] private RectTransform targetUI;

        private UIManager _uiManager;

        private Vector3 _movePos;
        private Vector3 _offsetPos;

        private CanvasGroup _targetWindow;

        private void Awake()
            => _targetWindow = targetUI.GetComponent<CanvasGroup>();

        private void Start()
            => _uiManager = GameManager.Instance.UIManager;
        
        public void OnBeginDrag(PointerEventData eventData)
            => _offsetPos = targetUI.position - (Vector3)eventData.position;

        public void OnDrag(PointerEventData eventData)
        {
            _movePos.x = eventData.position.x;
            _movePos.y = eventData.position.y;
            targetUI.position = _movePos + _offsetPos;
        }

        // 해당 UI 창이 가장 앞으로 오도록 Sibling 변경
        public void OnPointerDown(PointerEventData eventData)
            => _uiManager.VisibleWindowAtFront(_targetWindow);        
    }
}