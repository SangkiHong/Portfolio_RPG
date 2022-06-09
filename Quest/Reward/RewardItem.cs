using UnityEngine;

namespace SK.Quests
{
    /* �ۼ���: ȫ���
     * ����: ����Ʈ ������ ���������� �ִ� Ŭ����
     * �ۼ���: 22�� 5�� 21��
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
