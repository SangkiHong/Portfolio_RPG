using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using SK.Quests;

namespace SK.UI
{
    public class QuestMiniInfo : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private Image image_QuestIcon;
        [SerializeField] private Text text_QuestTitle;
        [SerializeField] private Text text_QuestDescription;

        private readonly string _string_Bar = "- ";

        public void Assign(Quest quest)
        {
            text_QuestTitle.text = quest.DisplayName;
            text_QuestDescription.text = _string_Bar + quest.Description;

            gameObject.SetActive(true);
            gameObject.SetActive(false);
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
        }

        public void Unassign()
        {
            text_QuestTitle.text = string.Empty;
            text_QuestDescription.text = string.Empty;
            gameObject.SetActive(false);
        }
    }
}