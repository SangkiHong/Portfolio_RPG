using UnityEngine;

namespace SK.FSM
{
    public class AnimatorHook : MonoBehaviour
    {
        private Player _states;

        private Vector3 _animDelta;

        public void Init(Player stateManager)
        {
            _states = stateManager;
        }

        public void OnAnimatorMove() => OnAnimatorMoveOverride();
        
        protected virtual void OnAnimatorMoveOverride()
        {
            if (!_states.useRootMotion) return;
            
            if (_states.isGrounded && _states.fixedDeltaTime > 0)
            {
                _animDelta = _states.anim.deltaPosition / _states.fixedDeltaTime;               
                _states.characterController.SimpleMove(_animDelta);
            }
        }
    }
}
