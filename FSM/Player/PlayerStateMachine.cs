namespace SK.FSM
{
    public class PlayerStateMachine : StateMachineBase
    {
        private readonly PlayerStateManager _psm;
        public PlayerStateMachine(PlayerStateManager psm)
        {
            _psm = psm;
        }
    }
}