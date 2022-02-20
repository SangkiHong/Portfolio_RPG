using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sangki
{
    public class AnimatorHook : MonoBehaviour
    {
        private HumanoidStateManager states;

        public virtual void Init(HumanoidStateManager stateManager)
        {
            states = (HumanoidStateManager)stateManager;
        }

        public void OnAnimatorMove() => OnAnimatorMoveOverride();
        
        protected virtual void OnAnimatorMoveOverride()
        {
            if (!states.useRootMotion) return;
            
            if (states.isGrounded && states.delta > 0)
            {
                Vector3 v = (states.anim.deltaPosition) / states.delta;
                v.y = states.rigidbody.velocity.y;
                states.rigidbody.velocity = v;
            }
        }
    }
}
