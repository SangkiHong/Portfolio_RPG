using UnityEngine;

namespace SK.Quests
{
    /* 작성자: 홍상기
     * 내용: 퀘스트 보상을 아이템으로 주는 클래스
     * 작성일: 22년 5월 21일
     */

    [CreateAssetMenu(menuName = "Quest/Reward/Reward Item", fileName = "RewardItem_")]
    public class RewardItem : Reward
    {
        [SerializeField] private Data.ItemList itemList;
        [SerializeField] private int itemIndex;

        public override void GiveReward()
        {
            GameManager.Instance.DataManager.AddNewItem(itemList.itemList[itemIndex], Quantity);
        }
    }
}
