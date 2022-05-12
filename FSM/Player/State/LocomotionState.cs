using UnityEngine;

namespace SK.FSM
{
    public class LocomotionState : StateBase
    {
        private readonly PlayerStateManager _state;
        public LocomotionState(PlayerStateManager psm) => _state = psm;

        public override void FixedTick()
        {
            base.FixedTick();
            _state.playerInputs.Execute();
            _state.moveCharacter.Execute();
        }
    }
}
