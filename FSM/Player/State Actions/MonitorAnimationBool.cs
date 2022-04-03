using System.Collections;

namespace SK.FSM
{
    public class MonitorAnimationBool : StateAction
    {
        private readonly PlayerStateManager states;
        private StateBase targetState;
        private int targetBool;

        public MonitorAnimationBool(PlayerStateManager characterStateManager, int targetBool, StateBase targetState)
        {
            states = characterStateManager;
            this.targetBool = targetBool;
            this.targetState = targetState;
        }

        public override bool Execute()
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
