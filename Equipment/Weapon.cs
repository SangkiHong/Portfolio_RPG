using UnityEngine;
using SK.Behavior;

namespace SK
{
    [CreateAssetMenu(fileName = "Weapon_", menuName = "Equipments/Weapon", order = 0)]
    public class Weapon : Equipments
    {
        [Header("Stat")]
        [SerializeField] private int attackMinPower;
        [SerializeField] private int attackMaxPower;

        [Header("Combo")]
        [SerializeField] private int selectedComboIndex;
        [SerializeField] private int currentAttackIndex;
        [SerializeField] private ComboAttack[] attackCombo;

        private int _prevIndex;

        //Property
        public int AttackMinPower => attackMinPower;
        public int AttackMaxPower => attackMaxPower;


        public override void ExecuteAction(Animator anim, bool isDefault)
        {
            if (isDefault) currentAttackIndex = 0;
            _prevIndex = currentAttackIndex;

            anim.SetBool(Strings.animPara_isInteracting, true);
            anim.CrossFade(attackCombo[selectedComboIndex].comboAttacks[currentAttackIndex++].animName, 0.15f);

            if (currentAttackIndex > attackCombo[selectedComboIndex].comboAttacks.Length - 1) 
                currentAttackIndex = 0;
        }

        public int GetAttackAngle() => attackCombo[selectedComboIndex].comboAttacks[_prevIndex].attackAngle;
    }
}