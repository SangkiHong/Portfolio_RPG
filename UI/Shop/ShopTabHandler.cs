using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace SK
{
    public class ShopTabHandler : MonoBehaviour
    {
        public UnityAction<int> OnChanged;

        [SerializeField] private Transform focusTr;
        [SerializeField] private Button[] buttons;

        private Vector3 _focusLocalPos;
        private int _lastIndex;

        private void Awake()
        {
            for (int i = 0; i < buttons.Length; i++)
            {
                int tempIndex = i; 
                buttons[i].onClick.AddListener(delegate { OnChangeTab(tempIndex); }); 
            }

            _focusLocalPos = focusTr.localPosition;
        }

        public void Reset()
            =>OnChangeTab(0);

        private void OnChangeTab(int index)
        {
            buttons[_lastIndex].interactable = true;
            buttons[index].interactable = false;
            _lastIndex = index;

            _focusLocalPos.x = buttons[index].transform.localPosition.x;
            focusTr.localPosition = _focusLocalPos;

            OnChanged?.Invoke(index);
        }
    }
}