using UnityEngine;

namespace SK.Behavior
{
    [CreateAssetMenu(fileName = "Attack_", menuName = "Behavior/Attack")]
    public class AttackBase : ScriptableObject
    {
        public string animName;
        public int attackAngle = 60;
    }
}