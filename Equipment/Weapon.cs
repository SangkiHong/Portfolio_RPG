using UnityEngine;
using SK.Behavior;

namespace SK
{
    public enum AttackType 
    {
        CounterAttack, DodgeAttack, ChargeAttack, FinishAttack
    }

    [CreateAssetMenu(fileName = "Weapon_", menuName = "Equipments/Weapon", order = 0)]
    public class Weapon : Equipments
    {
        [System.Serializable]
        public struct SpecialAttack
        {
            public AttackType attackType;
            public AttackBase[] specialAttack;
        }

        [Header("Stat")]
        [SerializeField] private uint attackMinPower;
        [SerializeField] private uint attackMaxPower;

        [Header("Combo")]
        [SerializeField] private int selectedComboIndex;
        [SerializeField] private int currentAttackIndex;
        [SerializeField] private ComboAttack[] attackCombo;
        
        [Header("Special Attack")]
        [SerializeField] private SpecialAttack[] specialAttacks;
        
        [System.NonSerialized] public AttackBase currentAttack;
        public int CurrentAttackIndex => currentAttackIndex;

        // 무기 최소 공격력 + 공격 액션 추가 공격력을 합산한 값
        public uint AttackMinPower => attackMinPower;
        // 무기 최대 공격력 + 공격 액션 추가 공격력을 합산한 값
        public uint AttackMaxPower => attackMaxPower;


        public override void ExecuteAction(Animator anim, bool comboAttack)
        {
            // Attack Length - 1 보다 Index 값이 더 많아졌을 경우
            if (currentAttackIndex >= attackCombo[selectedComboIndex].comboAttacks.Length - 1)
            { 
                currentAttackIndex = -1;

                selectedComboIndex++;

                if (selectedComboIndex >= attackCombo.Length)                
                    selectedComboIndex = 0;
            }

            if (!comboAttack) currentAttackIndex = -1;

            currentAttackIndex++;

            currentAttack = attackCombo[selectedComboIndex].comboAttacks[currentAttackIndex];

            anim.CrossFade(currentAttack.animName, 0.15f);
        }

        public override void ExecuteSpecialAction(Animator anim, AttackType attackType, int index = 0)
        {
            foreach (var attack in specialAttacks)
            {
                if (attack.attackType.Equals(attackType))
                {
                    currentAttack = attack.specialAttack[index];

                    anim.CrossFade(currentAttack.animName, 0.15f);
                    break;
                }
            }
        }
    }
}