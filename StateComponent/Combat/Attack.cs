using UnityEngine;

namespace SK.Behavior
{
    [CreateAssetMenu(fileName = "Attack_", menuName = "Behavior/Attack")]
    public class Attack : ScriptableObject
    {
        public string animName;
        public uint attackPower = 1;
        public int attackAngle = 60;
        public bool isStrongAttack;
        public bool onRootMotion;
    }
}