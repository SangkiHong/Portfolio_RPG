using UnityEngine;

namespace SK.Behavior
{
    [CreateAssetMenu(fileName = "Attack_", menuName = "Behavior/Attack")]
    public class AttackBase : ScriptableObject
    {
        [SerializeField]
        internal string animName;
        [SerializeField]
        internal int attackAngle = 60;
    }
}