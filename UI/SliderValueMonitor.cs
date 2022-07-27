using UnityEngine;
using UnityEngine.UI;

namespace SK.UI
{
    public class SliderValueMonitor : MonoBehaviour
    {
        [SerializeField] private Text targetText;

        public void SetTextValue(float value)
            => targetText.text = Mathf.FloorToInt(value).ToString();
    }
}