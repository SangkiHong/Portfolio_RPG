using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class JoystickController : MonoBehaviour
{
    [SerializeField] private RectTransform handler;

    private Vector2 _defaultPos, _dir;
    private float _radius;

    private void Start()
    {
        _defaultPos = handler.transform.position;
        _radius = GetComponent<RectTransform>().rect.width * 0.5f;
    }

    // 터치(마우스 포함) 다운 시 이벤트
    public void OnPointerDonw(BaseEventData _eventData)
    {
        PointerEventData eventData = (PointerEventData)_eventData;
        handler.transform.position = eventData.position;
    }

    // 터치(마우스 포함) 업 시 이벤트
    public void OnPointerUp(BaseEventData _eventData)
    {
        PointerEventData eventData = (PointerEventData)_eventData;
        handler.transform.position = eventData.position;
    }

    // 드래그 시작 시 이벤트
    public void BeginDrag(BaseEventData _eventData)
    {
        PointerEventData eventData = (PointerEventData)_eventData;
        handler.transform.position = eventData.position;
    }

    // 드래그 중 이벤트
    public void Drag(BaseEventData _eventData)
    {
        PointerEventData eventData = (PointerEventData)_eventData;

        _dir = eventData.position - _defaultPos;

        var _dirMag = _dir.magnitude;
        if (_dirMag <= _radius)
            handler.transform.position = eventData.position;
        else
            handler.transform.position = _defaultPos + _dir.normalized * _radius;
    }

    // 드래그 마침 시 이벤트
    public void EndDrag(BaseEventData _eventData)
    {
        PointerEventData eventData = (PointerEventData)_eventData;
        handler.transform.position = _defaultPos;
    }
}
