using UnityEngine;
using UnityEngine.UI;

namespace SK.Quests
{
    public class QuestInfo : MonoBehaviour
    {
        [SerializeField] private GameObject infoPanel;
        [SerializeField] private Text questTitle;
        [SerializeField] private Text questDescription;
        [SerializeField] private Text questNpcName;

        [SerializeField] private GameObject rewardSlotParent;

        private UI.SlotBase[] _rewardSlots;
        private Quest _assignedQuest;

        private void Awake()
        {
            _rewardSlots = rewardSlotParent.GetComponentsInChildren<UI.SlotBase>();
            
        }

        public void DisplayQuestInfo(Quest quest)
        {
            // 이미 할당 되었던 퀘스트를 다시 열지 않는 경우
            if (_assignedQuest != quest)
            {
                _assignedQuest = quest;
                questTitle.text = _assignedQuest.DisplayName;
                questDescription.text = _assignedQuest.Description;
            }

            infoPanel.SetActive(true);
        }
    }
}