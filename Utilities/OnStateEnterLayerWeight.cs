using UnityEngine;

namespace Sangki
{
    public class OnStateEnterLayerWeight : StateMachineBehaviour
    {
        [SerializeField] private int targetLayer;
        [SerializeField] private float targetWeight;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
            => animator.SetLayerWeight(targetLayer, targetWeight);
    }
}
