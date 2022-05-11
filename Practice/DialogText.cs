using UnityEngine;
using UnityEngine.UI;
using System.Text;

namespace SK.Practice
{
    public class DialogText : MonoBehaviour
    {
        public string dialog = "안녕하세요 아이템 상점입니다. 필요한 아이템이 있으면 말씀해주세요.";

        [SerializeField] private Text dialogText;
        [SerializeField] private float interval;

        private StringBuilder stringBuilder;
        private float _elapsed;
        private int dialogIndex;

        private void Start()
        {
            stringBuilder = new StringBuilder();
        }

        private void Update()
        {
            if (_elapsed < interval)
            {
                _elapsed += Time.deltaTime;
            }
            else
            {
                if (dialogIndex < dialog.Length)
                {
                    _elapsed = 0;
                    stringBuilder.Append(dialog[dialogIndex++]);
                    dialogText.text = stringBuilder.ToString();
                    dialogText.text = stringBuilder.Append(dialog[stringBuilder.Length]).ToString();
                }
            }
        }
    }
}