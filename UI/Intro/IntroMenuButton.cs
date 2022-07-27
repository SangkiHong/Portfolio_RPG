using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace SK
{
    public class IntroMenuButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [SerializeField] private Image buttonImage;
        public UnityEvent OnClickButton;

        private Color _defaultButtonColor;
        private bool _isMouseEnter;

        private void Awake()
        {
            _defaultButtonColor = buttonImage.color;
        }

        private void Update()
        {
            if (_isMouseEnter && _defaultButtonColor.a < 1)
            {
                _defaultButtonColor.a += 1.5f * Time.deltaTime;
                buttonImage.color = _defaultButtonColor;
            }
            else if (!_isMouseEnter && _defaultButtonColor.a > 0)
            {
                _defaultButtonColor.a -= 1.5f * Time.deltaTime;
                buttonImage.color = _defaultButtonColor;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _isMouseEnter = true;
            AudioManager.Instance.PlayAudio(Strings.Audio_UI_OnButton);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _isMouseEnter = false;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            OnClickButton?.Invoke();
        }
    }
}
