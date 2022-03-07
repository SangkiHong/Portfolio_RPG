using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SK
{
    public class MonitorInteractingAnimation : StateAction
    {
        private CharacterStateManager states;
        private string targetBool;
        private string targetState;

        public MonitorInteractingAnimation(CharacterStateManager characterStateManager, string targetBool, string targetState)
        {
            states = characterStateManager;
            this.targetBool = targetBool;
            this.targetState = targetState;
        }

        public override bool Execute()
        {
            if (states.anim.GetBool(targetBool))
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
