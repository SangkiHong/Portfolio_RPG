using UnityEngine;
using UnityEngine.UI;
using SK.Quests;

namespace SK.UI
{
    public class RewardPanel : MonoBehaviour
    {
        // 퀘스트 이름
        [SerializeField] private Text text_QuestName;
        // 보상 슬롯
        [SerializeField] private RewardSlot[] rewardSlots;

        public void AssignReward(Quest quest)
        {
            gameObject.SetActive(true);

            // 퀘스트 이름 할당
            text_QuestName.text = quest.DisplayName;

            // 슬롯 초기화
            for (int i = 0; i < rewardSlots.Length; i++)
                rewardSlots[i].Unassign();

            // 보상 정보 가져오기
            Reward questReward = quest.Reward;
            int slotIndex = 0;

            // 획득 경험치
            if (questReward.exp > 0)
                rewardSlots[slotIndex++].Assign(UIManager.Instance.sprite_Exp, questReward.exp);
            // 획득 골드량
            if (questReward.gold > 0)
                rewardSlots[slotIndex++].Assign(UIManager.Instance.sprite_Gold, questReward.gold);
            // 획득 보석량
            if (questReward.gem > 0)
                rewardSlots[slotIndex++].Assign(UIManager.Instance.sprite_Gem, questReward.gem);
            
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
        }
    }
}
