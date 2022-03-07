using System;
using System.Collections;
using UnityEngine;

namespace SK
{
    public class OnStateEnterLayerWeight : StateMachineBehaviour
    {
        [SerializeField] private int targetLayer;
        [SerializeField] private float targetWeight;


        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (animator.GetLayerWeight(targetLayer) != targetWeight) PlayerStateManager.Instance.ChangeLayerWeight(targetLayer, targetWeight);
        }
    }
}
