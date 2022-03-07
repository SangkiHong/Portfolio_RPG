namespace SK.FSM
{
    public class StateChase : EnemyState
    {
        private readonly Enemy _enemy;
        
        public StateChase(Enemy enemyControl)
        {
            _enemy = enemyControl;
        }

        public override void StateInit()
        {
            _enemy.NavAgent.speed = _enemy.enemyData.Speed;
        }

        public override void FixedTick()
        {
            if (!_enemy.searchRadar.FindTarget())
            {
                _enemy.stateMachine.ChangeState(_enemy.statePatrol);
                return;
            }
            
            
            if (_enemy.NavAgent.isStopped)
            {
                _enemy.NavAgent.Warp(_enemy.mTransfrom.position);
                _enemy.NavAgent.isStopped = false;
                _enemy.NavAgent.updatePosition = true;
                _enemy.NavAgent.updateRotation = true;
            }
            
            if (!_enemy.NavAgent.pathPending)
            {
                if (!_enemy.isDamaged && _enemy.searchRadar.TargetObject)
                {
                    _enemy.NavAgent.SetDestination(_enemy.searchRadar.TargetObject.transform.position);
                }
            }
            if (_enemy.NavAgent.velocity.magnitude != 0 && _enemy.NavAgent.remainingDistance <= _enemy.NavAgent.stoppingDistance + 0.1f)
            {
                _enemy.Anim.SetBool(_enemy.AnimPara_isFight, true);
                _enemy.stateMachine.ChangeState(_enemy.stateAttack);
            }
        }
    }
}