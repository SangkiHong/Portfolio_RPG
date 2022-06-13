using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace SK.UI
{
    public class MovableUI : MonoBehaviour, IBeginDragHandler, IDragHandler
    {
        [SerializeField] private RectTransform targetUI;

        private Vector3 movePos;
        private Vector3 offsetPos;

        public void OnBeginDrag(PointerEventData eventData)
        {
            offsetPos = targetUI.position - (Vector3)eventData.position;
        }

        public void OnDrag(PointerEventData eventData)
        {
            movePos.x = eventData.position.x;
            movePos.y = eventData.position.y;
            targetUI.position = movePos + offsetPos;
        }
    }
}
