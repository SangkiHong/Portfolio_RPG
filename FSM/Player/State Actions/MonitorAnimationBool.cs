namespace SK.FSM
{
    public class MonitorAnimationBool
    {
        private readonly PlayerStateManager states;
        private readonly int _targetBoolHash;

        public MonitorAnimationBool(PlayerStateManager characterStateManager, int targetBoolHash)
        {
            states = characterStateManager;
            _targetBoolHash = targetBoolHash;
        }

        public bool Execute(StateBase targetState)
        {
            if (states.anim.GetBool(_targetBoolHash))
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
