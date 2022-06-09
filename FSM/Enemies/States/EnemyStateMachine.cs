namespace SK.FSM
{
    public class EnemyStateMachine : StateMachineBase
    {
        public EnemyStatePatrol statePatrol;
        public EnemyStateChase stateChase;
        public EnemyStateCombat stateCombat;
        public EnemyStateAttack stateAttack;
        public EnemyStateKnockBack stateKnockBack;
        public EnemyStateFlee stateFlee;

        internal readonly Enemy enemy;

        public EnemyStateMachine(Enemy enemy)
        {
            this.enemy = enemy;
            statePatrol = new EnemyStatePatrol(enemy, this);
            stateChase = new EnemyStateChase(enemy, this);
            stateCombat = new EnemyStateCombat(enemy, this);
            stateAttack = new EnemyStateAttack(enemy, this);
            stateKnockBack = new EnemyStateKnockBack(enemy, this);
            stateFlee = new EnemyStateFlee(enemy, this);
        }

        public override void ChangeState(StateBase state)
        {
            // 넉백 중일 경우 다른 상태로 변경 불가
            if (CurrentState == stateKnockBack)
                return;

            base.ChangeState(state);
            
            // 디버그용으로 현재 상태 표시
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