using UnityEngine;

namespace SK.FSM
{
    public class StatePatrol : EnemyState
    {
        private readonly Enemy _enemy;

        private Vector3 _randomPos;
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
            
            // NavAgent 재가동
            if (_enemy.NavAgent.isStopped)
            {
                _enemy.NavAgent.isStopped = false;
                _enemy.NavAgent.updatePosition = true;
                _enemy.NavAgent.updateRotation = true;
            }

            // 이동중이면 리턴
            if (_enemy.NavAgent.velocity.magnitude > 0.1f) return;
            // 순찰 타이머
            if (_seekIdleTimer < _enemy.searchRadar.SeekIdleDuration)
            {
                _enemy.Anim.SetBool(AnimParas.animPara_isInteracting, true);
                _seekIdleTimer += _enemy.fixedDelta;
            }
            else
            {
                _enemy.Anim.SetBool(AnimParas.animPara_isInteracting, false);
                _randomPos = _enemy.searchRadar.SeekAndWonder(_enemy.searchRadar.SeekDistance);
                _enemy.NavAgent.SetDestination(_randomPos);
                _seekIdleTimer = 0;
                _enemy.NavAgent.speed = 2;
                _enemy.NavAgent.stoppingDistance = 0;
            }
        }

        public override void StateExit()
        {
            _enemy.Anim.SetBool(AnimParas.animPara_isInteracting, false);
        }
    }
}