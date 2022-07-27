using UnityEngine;

namespace SK.FSM
{
    public class AnimatorHook : MonoBehaviour
    {
        private Player _states;

        private Animator _anim;
        private Vector3 _animDelta;

        public void Init(Player stateManager)
        {
            _states = stateManager;
            _anim = _states.anim;
        }

        public void OnAnimatorMove() => OnAnimatorMoveOverride();
        
        protected virtual void OnAnimatorMoveOverride()
        {
            if (!_states.useRootMotion) return;
            
            if (_states.isGrounded)
            {
                _animDelta = _anim.deltaPosition / Time.deltaTime;
                _states.characterController.SimpleMove(_animDelta);
            }
        }
    }
}
