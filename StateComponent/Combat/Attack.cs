using UnityEngine;

namespace SK.Behavior
{
    [CreateAssetMenu(fileName = "Attack_", menuName = "Behavior/Attack")]
    public class Attack : ScriptableObject
    {
        public string animName; // 공격에 대한 애니메이션 이름
        public uint attackPower = 1; // 공격 액션의 고정 데미지 값
        public int attackAngle = 60; // 공격 범위 각
        public bool isStrongAttack; // 강 공격 여부
        public bool isUninterruptible; // 공격 중단 불가 여부
        public bool showDamageRange; // 공격 범위 표시 여부
        public bool onRootMotion; // 루트 모션 여부
    }
}