using UnityEngine;

namespace SK.Quests
{
    /* �ۼ���: ȫ���
     * ����: ������ ���� �̻��� �Ǿ��� ��� ����Ʈ ���� ����
     * �ۼ���: 22�� 6�� 13��
     */

    [CreateAssetMenu(menuName = "Quest/Condition/LevelCondition", fileName = "Condition_Level")]
    public class ConditionLevel : QuestCondition
    {
        [SerializeField] private int acceptablePlayerLevel;
        public override bool IsPass(Quest quest)
        {
            if (Data.DataManager.Instance.PlayerData.Level >= acceptablePlayerLevel)
                return true;
            else
                return false;
        }
    }
}
