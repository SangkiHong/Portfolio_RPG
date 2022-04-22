using UnityEngine;
using SK.Behavior;

namespace SK
{
    public enum AttackType 
    {
        CounterAttack, DodgeAttack, FinishAttack
    }

    [CreateAssetMenu(fileName = "Weapon_", menuName = "Equipments/Weapon", order = 0)]
    public class Weapon : Equipments
    {
        [System.Serializable]
        public struct SpecialAttack
        {
            public AttackType attackType;
            public AttackBase specialAttack;
        }

        [Header("Stat")]
        [SerializeField] private int attackMinPower;
        [SerializeField] private int attackMaxPower;

        [Header("Combo")]
        [SerializeField] private int selectedComboIndex;
        [SerializeField] private int currentAttackIndex;
        [SerializeField] private ComboAttack[] attackCombo;
        
        [Header("Special Attack")]
        [SerializeField] private SpecialAttack[] specialAttacks;

        public int CurrentAttackIndex => currentAttackIndex;

        private int _prevIndex;

        //Property
        public int AttackMinPower => attackMinPower;
        public int AttackMaxPower => attackMaxPower;


        public override void ExecuteAction(Animator anim, bool comboAttack)
        {
            if (!comboAttack) currentAttackIndex = 0;
            _prevIndex = currentAttackIndex;

            anim.CrossFade(attackCombo[selectedComboIndex].comboAttacks[currentAttackIndex].animName, 0.15f);

            currentAttackIndex++;

            if (currentAttackIndex > attackCombo[selectedComboIndex].comboAttacks.Length - 1) 
                currentAttackIndex = 0;
        }

        public override void ExecuteAction(Animator anim, AttackType attackType)
        {
            foreach (var attack in specialAttacks)
            {
                if (attack.attackType.Equals(attackType))
                {
                    anim.CrossFade(attack.specialAttack.animName, 0.15f);
                    break;
                }
            }
        }

        public int GetAttackAngle() => attackCombo[selectedComboIndex].comboAttacks[_prevIndex].attackAngle;
    }
}