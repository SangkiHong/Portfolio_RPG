using UnityEngine;
using SK.Behavior;

namespace SK
{
    [CreateAssetMenu(fileName = "Weapon_", menuName = "Equipments/Weapon", order = 0)]
    public class Weapon : Equipments
    {
        [Header("Stat")]
        public int attackMinPower;
        public int attackMaxPower;

        [Header("Combo")]
        public int selectedComboIndex;
        public int currentAttackIndex;
        public ComboAttack[] attackCombo;

        private int _prevIndex;


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