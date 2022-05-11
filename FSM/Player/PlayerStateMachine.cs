namespace SK.FSM
{
    public class PlayerStateMachine : StateMachineBase
    {
        internal LocomotionState locomotionState;
        internal AttackState attackState;
        internal DodgeState dodgeState;
        internal KnockBackState knockBackState;

        public PlayerStateMachine(PlayerStateManager psm)
        {
            locomotionState = new LocomotionState(psm);
            attackState = new AttackState(psm);
            dodgeState = new DodgeState(psm);
            knockBackState = new KnockBackState(psm);

            ChangeState(locomotionState);
        }
    }
}