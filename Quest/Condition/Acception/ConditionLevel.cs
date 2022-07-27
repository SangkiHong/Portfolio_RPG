using UnityEngine;

namespace SK.Quests
{
    /* 작성자: 홍상기
     * 내용: 지정된 레벨 이상이 되었을 경우 퀘스트 수락 가능
     * 작성일: 22년 6월 13일
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
