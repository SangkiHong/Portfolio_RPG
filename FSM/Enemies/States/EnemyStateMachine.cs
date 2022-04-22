namespace SK.FSM
{
    public class EnemyStateMachine : StateMachineBase
    {
        public StatePatrol statePatrol;
        public StateChase stateChase;
        public StateCombat stateCombat;
        public StateAttack stateAttack;
        public StateFlee stateFlee;

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

        public void StopMachine(bool withNavAgent = false)
        {
            base.CurrentState = null;
            
            // Stop NavAgent
            if (withNavAgent && !enemy.navAgent.isStopped)
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