using UnityEngine;

namespace SK.FSM
{
    public class StatePatrol : EnemyState
    {
        private readonly Enemy _enemy;
        private float _seekIdleTimer;
        
        public StatePatrol(Enemy enemyControl)
        {
            _enemy = enemyControl;
        }

        public override void FixedTick()
        {
            if (_enemy.searchRadar.FindTarget())
            {
                _enemy.stateMachine.ChangeState(_enemy.stateChase);
                return;
            }
            
            if (_enemy.NavAgent.isStopped)
            {
                _enemy.NavAgent.isStopped = false;
                _enemy.NavAgent.updatePosition = true;
                _enemy.NavAgent.updateRotation = true;
            }
            
            if (!_enemy.NavAgent.hasPath)
            {
                if (_seekIdleTimer > _enemy.searchRadar.SeekIdleDuration)
                {
                    Vector3 randomPos = _enemy.searchRadar.SeekAndWonder(_enemy.searchRadar.SeekDistance);
                    _enemy.NavAgent.SetDestination(randomPos);
                    _seekIdleTimer = 0;
                    _enemy.NavAgent.speed = 2;
                    _enemy.NavAgent.stoppingDistance = 0;
                }
                else
                {
                    _enemy.Anim.SetFloat(_enemy.AnimPara_MoveBlend, 0);
                    _seekIdleTimer += _enemy.fixedDelta;
                }
            }
        }
    }
}