using System;
using System.Collections;
using UnityEngine;

namespace SK
{
    public class OnStateEnterLayerWeight : StateMachineBehaviour
    {
        [SerializeField] private int targetLayer;
        [SerializeField] private float targetWeight;

        private bool isChangingWeight;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (animator.GetLayerWeight(targetLayer) != targetWeight)
                isChangingWeight = true;
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            // Change Animator Layer Weight
            if (isChangingWeight)
            {
                float currentWeight = animator.GetLayerWeight(targetLayer);

                if (targetWeight > 0.5f)
                {
                    if (currentWeight < 0.99f)
                        currentWeight += Time.deltaTime * 3f;
                    else
                    {
                        currentWeight = 1;
                        isChangingWeight = false;
                    }
                }
                else
                {
                    if (currentWeight > 0.01f)
                        currentWeight -= Time.deltaTime * 3f;
                    else
                    {
                        currentWeight = 0;
                        isChangingWeight = false;
                    }
                }
                animator.SetLayerWeight(targetLayer, currentWeight);
            }
        }
    }
}
