using UnityEngine;
using UnityEngine.UI;
using SK.Quests;

namespace SK.UI
{
    public class RewardPanel : MonoBehaviour
    {
        // ����Ʈ �̸�
        [SerializeField] private Text text_QuestName;
        // ���� ����
        [SerializeField] private RewardSlot[] rewardSlots;

        public void AssignReward(Quest quest)
        {
            gameObject.SetActive(true);

            // ����Ʈ �̸� �Ҵ�
            text_QuestName.text = quest.DisplayName;

            // ���� �ʱ�ȭ
            for (int i = 0; i < rewardSlots.Length; i++)
                rewardSlots[i].Unassign();

            // ���� ���� ��������
            Reward questReward = quest.Reward;
            int slotIndex = 0;

            // ȹ�� ����ġ
            if (questReward.exp > 0)
                rewardSlots[slotIndex++].Assign(UIManager.Instance.sprite_Exp, questReward.exp);
            // ȹ�� ��差
            if (questReward.gold > 0)
                rewardSlots[slotIndex++].Assign(UIManager.Instance.sprite_Gold, questReward.gold);
            // ȹ�� ������
            if (questReward.gem > 0)
                rewardSlots[slotIndex++].Assign(UIManager.Instance.sprite_Gem, questReward.gem);
            
            // ȹ�� ������
            Item tempItem;
            RewardItem tempRewardItem;

            // ���� �������� �ִ� ���
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
