using UnityEngine;

namespace SK.FSM
{
    public class AnimatorHook : MonoBehaviour
    {
        private PlayerStateManager _states;
        private Vector3 animDelta;

        public void Init(PlayerStateManager stateManager)
        {
            _states = stateManager;
        }

        public void OnAnimatorMove() => OnAnimatorMoveOverride();
        
        protected virtual void OnAnimatorMoveOverride()
        {
            if (!_states.useRootMotion) return;
            
            if (_states.isGrounded && _states.fixedDelta > 0)
            {
                transform.position = _states.anim.rootPosition;

                /*animDelta = _states.anim.deltaPosition / _states.fixedDelta;
                var pos = _states.mTransform.position;
                Vector3.MoveTowards(pos, pos + animDelta, _states.fixedDelta);*/
                //_states.thisRigidbody.velocity = animDelta; //deprecated::Don't use Rigidbody
            }
        }
    }
}
