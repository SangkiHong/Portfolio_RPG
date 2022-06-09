using UnityEngine;

namespace SK.Data
{
    public class UnitBaseData : ScriptableObject
    {
        public string Name = "";
        public uint Level = 1;
        public uint Hp = 10;
        public uint Mp = 10;
        public uint Str = 1;
        public uint Dex = 1;
        public uint Int = 1;
        public uint Def = 5;
        public float Speed = 5;
        public float AttackSpeed = 1f;
        public float CriticalChance = 0.05f; // 치명타 확률(기본 5%)
        public float CriticalMultiplier = 1.5f; // 치명타 배율(기본 150%)
        public float RecoverHp = 1.5f;
        public float RecoverMp = 0.5f;
    }
}