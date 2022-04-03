using UnityEngine;

namespace SK.FSM
{
    public class LocomotionState : StateBase
    {
        PlayerStateManager _state;
        public LocomotionState(PlayerStateManager psm) => _state = psm;

        public override void FixedTick()
        {
            base.FixedTick();
            _state.inputManager.Execute();
            _state.moveCharacter.Execute();
        }
    }
}
