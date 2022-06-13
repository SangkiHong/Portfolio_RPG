using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SK.Quests
{
    /* 작성자: 홍상기
     * 내용: 앞서 지정된 퀘스트를 완료해야 퀘스트 수락 가능
     * 작성일: 22년 6월 13일
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
