using System;
using System.Collections;
using UnityEngine;

namespace SK
{
    public class OnStateEnterLayerWeight : StateMachineBehaviour
    {
        [Header("Change Layer Weight On State Enter")]
        [SerializeField] private int targetLayer;
        [SerializeField] private float targetWeight;
        [SerializeField] private float changeSpeed;

        private bool isChangingWeight;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (animator.GetLayerWeight(targetLayer) != targetWeight)
            { 
                if (changeSpeed == 0)
                    animator.SetLayerWeight(targetLayer, targetWeight);
                else
                    isChangingWeight = true;
            }
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            // Interacting 중에는 Layer 변경 중단
            if (animator.GetBool(Strings.animPara_isInteracting)) isChangingWeight = false;

            // Change Animator Layer Weight
            if (isChangingWeight)
            {
                float currentWeight = animator.GetLayerWeight(targetLayer);

                if (targetWeight > 0.5f)
                {
                    if (currentWeight < 0.99f)
                        currentWeight += Time.deltaTime * changeSpeed;
                    else
                    {
                        currentWeight = 1;
                        isChangingWeight = false;
                    }
                }
                else
                {
                    if (currentWeight > 0.01f)
                        currentWeight -= Time.deltaTime * changeSpeed;
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
