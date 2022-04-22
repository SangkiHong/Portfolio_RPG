namespace SK.FSM
{
    public class MonitorAnimationBool
    {
        private readonly PlayerStateManager states;
        private readonly StateBase targetState;
        private readonly int targetBool;

        public MonitorAnimationBool(PlayerStateManager characterStateManager, int targetBool, StateBase targetState)
        {
            states = characterStateManager;
            this.targetBool = targetBool;
            this.targetState = targetState;
        }

        public bool Execute()
        {
            if (states.anim.GetBool(targetBool))
            {
                return false;
            }
            else
            {
                states.stateMachine.ChangeState(targetState);
                
                return true;
            }
        }
    }
}
