namespace SK.FSM
{
    public class PlayerStateMachine : StateMachineBase
    {
        private readonly PlayerStateManager _psm;

        internal LocomotionState locomotionState;
        internal AttackState attackState;
        internal DodgeState dodgeState;

        public PlayerStateMachine(PlayerStateManager psm)
        {
            _psm = psm;

            locomotionState = new LocomotionState(psm);
            attackState = new AttackState(psm);
            dodgeState = new DodgeState(psm);

            ChangeState(locomotionState);
        }
    }
}