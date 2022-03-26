using UnityEngine;

namespace SK.Data
{
    [CreateAssetMenu(fileName = "PlayerData", menuName = "Game Data/Player Data")]
    public class PlayerData : ScriptableObject
    {
        public int Level = 1;
        public int Str = 1;
        public int Dex = 1;
        public int Int = 1;
        public float CriticalChance = 0.05f; // 치명타 확률(기본 5%)
        public float CriticalMultiplier = 1.5f; // 치명타 배율(기본 150%)
    }
}
