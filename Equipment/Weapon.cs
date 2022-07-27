using UnityEngine;

namespace SK
{
    [CreateAssetMenu(fileName = "Weapon_", menuName = "Equipments/Weapon", order = 0)]
    public class Weapon : Equipments
    {
        public bool isTwoHand;

        [Header("Stat")]
        [SerializeField] private int attackMinPower; // 무기 최소 공격력
        [SerializeField] private int attackMaxPower; // 무기 최대 공격력

        // 파라미터
        public int AttackMinPower => attackMinPower;        
        public int AttackMaxPower => attackMaxPower;
    }
}