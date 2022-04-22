using UnityEngine;

namespace SK.FSM
{
    public class AnimatorHook : MonoBehaviour
    {
        private PlayerStateManager _states;

        private Vector3 _animDelta;
        private LayerMask _layerMask;

        public void Init(PlayerStateManager stateManager)
        {
            _states = stateManager;

            _layerMask = 1 << _states.gameObject.layer;
            _layerMask = ~_layerMask;
        }

        public void OnAnimatorMove() => OnAnimatorMoveOverride();
        
        protected virtual void OnAnimatorMoveOverride()
        {
            if (!_states.useRootMotion) return;
            
            if (_states.isGrounded && _states.fixedDelta > 0)
            {
                _animDelta = _states.anim.deltaPosition / _states.fixedDelta;

                Debug.DrawRay(_states.mTransform.position + Vector3.up * 0.5f, _animDelta.normalized);
                Ray ray = new Ray(_states.mTransform.position + Vector3.up * 0.5f, _animDelta.normalized);
                if (Physics.Raycast(ray, 0.5f, _layerMask, QueryTriggerInteraction.Ignore))
                    return;
               
                _states.characterController.SimpleMove(_animDelta);
            }
        }
    }
}
