using UnityEngine;

namespace SK.Data
{
    /* 작성자: 홍상기
     * 내용: 스킬에 관련된 데이터의 스크립터블오브젝트
     * 작성일: 22년 6월 19일
     */

    [CreateAssetMenu(fileName = "Skill_", menuName = "Behavior/Skill", order = 1)]
    public class SkillData : ScriptableObject
    {
        public int skillID;
        public Sprite skillIcon;
        public string skillName;
        public string skillDescription;
        public Behavior.Attack skillAttack;
        public bool isActiveSkill;
        public uint useMpAmount;
        public float skillCoolTime;
        public uint requireSkillPoint;
        public SkillData prevSkill;
        public SkillData nextSkill;
    }
}
