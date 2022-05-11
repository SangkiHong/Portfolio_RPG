using UnityEngine;

namespace SK.Data
{
    [CreateAssetMenu(fileName = "PlayerData", menuName = "Game Data/Player Data")]
    public class PlayerData : ScriptableObject
    {
        public string Name = "";
        public uint Level = 1;
        public uint Exp = 0;
        public uint MaxHp = 10;
        public uint MaxMp = 10;
        public uint MaxSp = 10;
        public uint Str = 1;
        public uint Dex = 1;
        public uint Int = 1;
        public float AttackSpeed = 1f;
        public float CriticalChance = 0.05f; // 치명타 확률(기본 5%)
        public float CriticalMultiplier = 1.5f; // 치명타 배율(기본 150%)
        public uint Armor = 0;
        public float Speed = 1f;
        public uint Avoidance = 1;
        public float RecoverHp = 1.5f;
        public float RecoverMp = 0.5f;
        public float RecoverSp = 0.3f;

        public Vector3 RecentLocation;

        public void Initialize()
        {
            Name = "Newbie";
            Level = 1;
            Exp = 0;
            MaxHp = 10;
            MaxMp = 10;
            MaxSp = 10;
            Str = 1;
            Dex = 1;
            Int = 1;
            AttackSpeed = 1f;
            CriticalChance = 0.05f;
            CriticalMultiplier = 1.5f;
            Armor = 0;
            Speed = 1f;
            Avoidance = 1;
            RecoverHp = 1.5f;
            RecoverMp = 0.5f;
            RecoverSp = 0.3f;
            RecentLocation = new Vector3(110, 30, 5);
        }
    }
}
