using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sangki
{
    public class MonitorInteractingAnimation : StateAction
    {
        private HumanoidStateManager states;
        private string targetBool;
        private string targetState;

        public MonitorInteractingAnimation(HumanoidStateManager humanoidStateManager, string targetBool, string targetState)
        {
            states = humanoidStateManager;
            this.targetBool = targetBool;
            this.targetState = targetState;
        }

        public override bool Execute()
        {
            bool isInteracting = states.anim.GetBool(targetBool);

            if (isInteracting)
            {
                return false;
            }
            else
            {
                states.ChangeState(targetState);
                
                return true;
            }
        }
    }
}
