using UnityEngine;

namespace SK.Data
{
    [CreateAssetMenu(fileName = "PlayerData", menuName = "Game Data/Player Data")]
    public class PlayerData : UnitBaseData
    {
        private uint exp;
        private uint gold;
        private uint gem;
        public uint Sp = 10;
        public uint Avoidance = 1;
        public float RecoverSp = 0.1f; // 초당 최대 스테미나에서 몇 %씩 회복
        private uint skillPoint = 0;
        private uint statPoint = 0;

        public Vector3 RecentPosition;
        public int RecentLocation;
        
        // 프로퍼티
        public new uint Exp
        {
            get { return exp; }
            set
            {
                if (value < 0)
                    exp = 0;
                else
                    exp = value;
            }
        }
        public uint Gold
        {
            get { return gold; }
            set
            {
                if (value < 0)
                    gold = 0;
                else
                    gold = value;
            }
        }
        public uint Gem
        {
            get { return gem; }
            set
            {
                if (value < 0)
                    gem = 0;
                else
                    gem = value;
            }
        }
        public uint SkillPoint
        {
            get { return skillPoint; }
            set
            {
                if (value < 0)
                    skillPoint = 0;
                else
                    skillPoint = value;
            }
        }
        public uint StatPoint 
        {
            get { return statPoint; }
            set
            {
                if (value < 0)
                    statPoint = 0;
                else
                    statPoint = value;
            }
        }

        public void Initialize()
        {
            DisplayName = "CalebHong";
            Level = 1;
            exp = 0;
            gold = 2000;
            gem = 0;
            Sp = 0;
            Hp = 0;
            Mp = 0;
            Sp = 0;
            Str = 1;
            Dex = 1;
            Int = 1;
            skillPoint = 0;
            statPoint = 0;
            AttackSpeed = 1f;
            CriticalChance = 0.05f;
            CriticalMultiplier = 1.5f;
            Def = 0;
            Speed = 1f;
            Avoidance = 1;
            RecoverHp = 1.5f;
            RecoverMp = 0.5f;
            RecoverSp = 2.5f;
            RecentLocation = 0;
            RecentPosition = new Vector3(-164.48f, 14, -173.11f);
        }
    }
}
