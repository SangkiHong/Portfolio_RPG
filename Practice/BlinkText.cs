using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace SK.Practice
{
    public class BlinkText : MonoBehaviour
    {
        public float blinkPeriod = 0.5f;
        private Text touchText;
        private float _elapsed;

        void Update()
        {
            _elapsed += Time.deltaTime;

            if (_elapsed >= blinkPeriod)
            {
                _elapsed = 0;
                touchText.gameObject.SetActive(!touchText.gameObject.activeSelf);
            }
        }
    }
}