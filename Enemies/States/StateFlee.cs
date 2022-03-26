using UnityEngine;

namespace SK.FSM
{
    public class StateFlee : EnemyState
    {
        private readonly Enemy _enemy;
        
        public StateFlee(Enemy enemyControl)
        {
            _enemy = enemyControl;
        }
        
        public override void StateInit()
        {
            _enemy.NavAgent.speed = _enemy.enemyData.Speed * 0.8f;
            
            // NavAgent 재가동
            if (_enemy.NavAgent.isStopped)
            {
                _enemy.NavAgent.velocity = Vector3.zero;
                _enemy.NavAgent.isStopped = false;
                _enemy.NavAgent.updatePosition = true;
                _enemy.NavAgent.updateRotation = true;
            }
            
            _enemy.walkAnimSpeed = 1f;
            
            // Set Anim State
            _enemy.Anim.SetBool(Strings.AnimPara_isFight, false);
        }

        public override void FixedTick()
        {
            if (_enemy.isInteracting) return;

            // 타겟의 반대 방향으로 도주
            var position = _enemy.mTransform.position;
            var fleePos = (position - _enemy.health.hitTransform.position).normalized;
            _enemy.NavAgent.SetDestination(position + fleePos * 5);
        }

        public override void StateExit()
        {
            _enemy.mRigidbody.velocity = Vector3.zero;
            _enemy.NavAgent.isStopped = true;
        }
    }
}