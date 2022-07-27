using UnityEngine;

namespace SK.Data
{
    [CreateAssetMenu(fileName = "EnemyData", menuName = "Game Data/Enemy Data")]
    public class EnemyData : UnitBaseData
    {
        public int EnemyId; // �� ���� ID
        public float respawnInterval; // ������ �ð� ����
    }
}
