using UnityEngine;

namespace SK
{
    public class OnComboAttack : StateMachineBehaviour
    {
        [SerializeField] 
        private ComboAttack[] comboAttack;
        
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (comboAttack != null)
                PlayerStateManager.Instance.currentCombo = comboAttack;
        }
    }
}