using UnityEngine;

namespace SK.Data
{
    [CreateAssetMenu(fileName = "EnemyData", menuName = "Game Data/Enemy Data")]
    public class EnemyData : ScriptableObject
    {
        public string EnemyName;
        public uint Level = 1;
        public uint Hp = 10;
        public uint Mp = 10;
        public uint Str = 1;
        public uint Def = 5;
        public float Speed = 5;
        public float CriticalChance = 0.05f; // ġ��Ÿ Ȯ��(�⺻ 5%)
        public float CriticalMultiplier = 1.5f; // ġ��Ÿ ����(�⺻ 150%)
    }
}
