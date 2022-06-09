namespace SK.FSM
{
    public class LocomotionState : StateBase
    {
        private readonly Player _player;
        public LocomotionState(Player player) => _player = player;

        public override void FixedTick()
        {
            base.FixedTick();
            _player.inputActions.Execute();
            _player.moveCharacter.Execute();
        }
    }
}
