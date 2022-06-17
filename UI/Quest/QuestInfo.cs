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
        [SerializeField] private RewardSlot[] _rewardSlots;

        [SerializeField] private Sprite sprite_Exp;
        [SerializeField] private Sprite sprite_Gold;
        [SerializeField] private Sprite sprite_Gem;

        // 아이템리스트매니저
        private Data.ItemListManager _itemListManager;
        // 할당된 퀘스트
        private Quest _assignedQuest;

        private readonly string _string_EmptyNPC = "없음(즉시 완료)";

        public void DisplayQuestInfo(Quest quest)
        {
            if (_itemListManager == null)
                _itemListManager = GameManager.Instance.ItemListManager;

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
                for (int i = 0; i < _rewardSlots.Length; i++)
                    _rewardSlots[i].Unassign();

                // 획득 경험치
                if (questReward.exp > 0) 
                    _rewardSlots[slotIndex++].Assign(sprite_Exp, questReward.exp);
                // 획득 골드량
                if (questReward.gold > 0)
                    _rewardSlots[slotIndex++].Assign(sprite_Gold, questReward.gold);
                // 획득 보석량
                if (questReward.gem > 0) 
                    _rewardSlots[slotIndex++].Assign(sprite_Gem, questReward.gem);

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
                            tempItem = _itemListManager.GetItembyID((int)tempRewardItem.itemList, tempRewardItem.itemId);
                            _rewardSlots[slotIndex++].Assign(tempItem.ItemIcon, tempRewardItem.itemAmount);
                        }
                    }
                }

                // 완료 가능 NPC가 있는 경우 이름 표시
                if (!string.IsNullOrEmpty(_assignedQuest.CompleteNPC))
                    questNpcName.text = _assignedQuest.CompleteNPC;
                // 완료 가능 NPC가 따로 없이 즉시 완료 표시
                else
                    questNpcName.text = _string_EmptyNPC;
            }

            infoPanel.SetActive(true);
        }
    }
}