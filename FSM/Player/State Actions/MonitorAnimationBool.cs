namespace SK.FSM
{
    public class MonitorAnimationBool
    {
        private readonly Player _player;
        private readonly int _targetBoolHash;

        public MonitorAnimationBool(Player player, int targetBoolHash)
        {
            _player = player;
            _targetBoolHash = targetBoolHash;
        }

        public bool Execute(StateBase targetState)
        {
            if (_player.anim.GetBool(_targetBoolHash))
            {
                return false;
            }
            else
            {
                _player.stateMachine.ChangeState(targetState);
                
                return true;
            }
        }
    }
}
