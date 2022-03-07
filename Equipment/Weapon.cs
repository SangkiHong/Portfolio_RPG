using UnityEngine;

namespace SK
{
    [CreateAssetMenu(fileName = "Weapon_", menuName = "Equipments/Weapon", order = 0)]
    public class Weapon : Equipments
    {
        [SerializeField] 
        private string defaultAim;
        
        [System.NonSerialized]
        public WeaponHook weaponHook;
        
        private int animIndex;
        
        public override void ExcuteAction()
        {
            if (PlayerStateManager.Instance.canComboAttack)
            {
                PlayerStateManager.Instance.PlayerTargetAnimation(PlayerStateManager.Instance.currentCombo[animIndex].animName, true);
            }
            else
            {
                PlayerStateManager.Instance.PlayerTargetAnimation(defaultAim, true);
            }

            PlayerStateManager.Instance.ChangeState(PlayerStateManager.AttackStateId);

            animIndex++;

            if (animIndex > PlayerStateManager.Instance.currentCombo.Length - 1) animIndex = 0;
        }
    }
}