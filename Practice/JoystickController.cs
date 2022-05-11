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

    // ��ġ(���콺 ����) �ٿ� �� �̺�Ʈ
    public void OnPointerDonw(BaseEventData _eventData)
    {
        PointerEventData eventData = (PointerEventData)_eventData;
        handler.transform.position = eventData.position;
    }

    // ��ġ(���콺 ����) �� �� �̺�Ʈ
    public void OnPointerUp(BaseEventData _eventData)
    {
        PointerEventData eventData = (PointerEventData)_eventData;
        handler.transform.position = eventData.position;
    }

    // �巡�� ���� �� �̺�Ʈ
    public void BeginDrag(BaseEventData _eventData)
    {
        PointerEventData eventData = (PointerEventData)_eventData;
        handler.transform.position = eventData.position;
    }

    // �巡�� �� �̺�Ʈ
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

    // �巡�� ��ħ �� �̺�Ʈ
    public void EndDrag(BaseEventData _eventData)
    {
        PointerEventData eventData = (PointerEventData)_eventData;
        handler.transform.position = _defaultPos;
    }
}
