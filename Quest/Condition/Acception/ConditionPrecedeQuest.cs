using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SK.Quests
{
    /* �ۼ���: ȫ���
     * ����: �ռ� ������ ����Ʈ�� �Ϸ��ؾ� ����Ʈ ���� ����
     * �ۼ���: 22�� 6�� 13��
     */

    [CreateAssetMenu(menuName = "Quest/Condition/PrecedeQuestCondition", fileName = "Condition_PrecedeQuest")]
    public class ConditionPrecedeQuest : QuestCondition
    {
        [SerializeField] private Quest[] precedeCompleteQuests;

        public override bool IsPass(Quest quest)
        {
            for (int i = 0; i < precedeCompleteQuests.Length; i++)
            {
                if (!precedeCompleteQuests[i].IsComplete)
                    return false;
            }

            return true;
        }
    }
}
