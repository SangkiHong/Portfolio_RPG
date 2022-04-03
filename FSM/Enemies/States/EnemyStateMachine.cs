namespace SK.FSM
{
    public class EnemyStateMachine : StateMachineBase
    {
        public StateBase statePatrol;
        public StateBase stateChase;
        public StateBase stateCombat;
        public StateBase stateAttack;
        public StateBase stateFlee;

        internal readonly Enemy enemy;

        public EnemyStateMachine(Enemy enemy)
        {
            this.enemy = enemy;
            statePatrol = new StatePatrol(enemy, this);
            stateChase = new StateChase(enemy, this);
            stateCombat = new StateCombat(enemy, this);
            stateAttack = new StateAttack(enemy, this);
            stateFlee = new StateFlee(enemy, this);
        }

        public override void ChangeState(StateBase state)
        {
            base.ChangeState(state);
            enemy.currentStateName = state.GetType().Name;
        }

        public void StopMachine()
        {
            base.CurrentState = null;
            
            // Stop NavAgent
            if (!enemy.navAgent.isStopped)
            {
                enemy.navAgent.isStopped = true;
                enemy.navAgent.updatePosition = false;
                enemy.navAgent.updateRotation = false;
            }
        }

        internal void ResetPatrol()
        {
            enemy.isPatrol = true;
            enemy.searchRadar.ResetOriginPos();
            ChangeState(statePatrol);
        }
    }
}