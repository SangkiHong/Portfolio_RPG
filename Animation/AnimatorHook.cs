using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SK
{
    public class AnimatorHook : MonoBehaviour
    {
        private CharacterStateManager states;

        public virtual void Init(CharacterStateManager stateManager)
        {
            states = (CharacterStateManager)stateManager;
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

        public void OpenDamageCollider() => states.HandleDamageCollider(true);
        
        public void CloseDamageCollider() => states.HandleDamageCollider(false);
    }
}
