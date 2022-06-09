namespace SK.FSM
{
    public abstract class StateMachineBase
    {
        public StateBase CurrentState;

        public virtual void ChangeState(StateBase state)
        {
            if (CurrentState != null)
                CurrentState.StateExit();

            CurrentState = state;
            CurrentState.StateInit();
        }

        public virtual bool isAssigned() { return CurrentState != null; }
    }
}