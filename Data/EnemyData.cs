using UnityEngine;

namespace SK.Data
{
    [CreateAssetMenu(fileName = "EnemyData", menuName = "Game Data/Enemy Data")]
    public class EnemyData : UnitBaseData
    {
        public int EnemyId; // 적 고유 ID
        public float respawnInterval; // 리스폰 시간 간격
    }
}
