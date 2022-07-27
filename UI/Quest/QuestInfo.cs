using UnityEngine;
using UnityEngine.UI;

namespace SK.Quests
{
    /* 작성자: 홍상기
     * 내용: 퀘스트 정보를 표시할 UI 패널
     * 작성일: 22년 5월 25일
     */

    public class QuestInfo : MonoBehaviour
    {
        // 퀘스트 정보 UI 패널 오브젝트
        [SerializeField] private GameObject infoPanel;
        // 퀘스트 타이틀 TEXT
        [SerializeField] private Text questTitle;
        // 퀘스트 상세 설명 TEXT
        [SerializeField] private Text questDescription;
        // 퀘스트 완료 NPC TEXT
        [SerializeField] private Text questNpcName;

        // 보상 슬롯
        [SerializeField] private RewardSlot[] rewardSlots;

        // 할당된 퀘스트
        private Quest _assignedQuest;

        private readonly string _string_EmptyNPC = "없음(즉시 완료)";

        public void DisplayQuestInfo(Quest quest)
        {
            // 이전 할당 퀘스트를 다시 여는 것이 아닌 경우
            if (_assignedQuest != quest)
            {
                _assignedQuest = quest;
                // 퀘스트 정보 표시
                questTitle.text = _assignedQuest.DisplayName;
                questDescription.text = _assignedQuest.Description;

                // 퀘스트 완료 시 받을 보상을 슬롯으로 표시
                Reward questReward = _assignedQuest.Reward;
                int slotIndex = 0;

                // 슬롯 초기화
                for (int i = 0; i < rewardSlots.Length; i++)
                    rewardSlots[i].Unassign();

                // 획득 경험치
                if (questReward.exp > 0) 
                    rewardSlots[slotIndex++].Assign(UI.UIManager.Instance.sprite_Exp, questReward.exp);
                // 획득 골드량
                if (questReward.gold > 0)
                    rewardSlots[slotIndex++].Assign(UI.UIManager.Instance.sprite_Gold, questReward.gold);
                // 획득 보석량
                if (questReward.gem > 0) 
                    rewardSlots[slotIndex++].Assign(UI.UIManager.Instance.sprite_Gem, questReward.gem);

                // 획득 아이템
                Item tempItem;
                RewardItem tempRewardItem;
                
                // 보상 아이템이 있는 경우
                if (questReward.rewardItems != null)
                {
                    var length = questReward.rewardItems.Length;
                    if (length > 0)
                    {
                        for (int i = 0; i < length; i++)
                        {
                            tempRewardItem = questReward.rewardItems[i];
                            tempItem = GameManager.Instance.ItemListManager
                                .GetItembyID((int)tempRewardItem.itemList, tempRewardItem.itemId);
                            rewardSlots[slotIndex++].Assign(tempItem, tempRewardItem.itemAmount);
                        }
                    }
                }

                // 완료 가능 NPC가 있는 경우 이름 표시
                if (!string.IsNullOrEmpty(_assignedQuest.CompleteNPC))
                    questNpcName.text = SceneManager.Instance.GetNPC(_assignedQuest.CompleteNPC).DisplayName;
                // 완료 가능 NPC가 따로 없이 즉시 완료 표시
                else
                    questNpcName.text = _string_EmptyNPC;
            }

            infoPanel.SetActive(true);
        }
    }
}