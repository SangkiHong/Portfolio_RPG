using UnityEngine;

namespace SK.Data
{
    [CreateAssetMenu(fileName = "EnemyData", menuName = "Game Data/Enemy Data")]
    public class EnemyData : ScriptableObject
    {
        public string EnemyName;
        public int Level = 1;
        public int Hp = 10;
        public int Mp = 10;
        public int Str = 1;
        public int Def = 5;
        public float Speed = 5;
        public float CriticalChance = 0.05f; // ġ��Ÿ Ȯ��(�⺻ 5%)
        public float CriticalMultiplier = 1.5f; // ġ��Ÿ ����(�⺻ 150%)
    }
}
