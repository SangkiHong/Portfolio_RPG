using UnityEngine;

namespace SK.FSM
{
    public class StateAttack : EnemyState
    {
        private readonly Enemy _enemy;
        
        public StateAttack(Enemy enemyControl)
        {
            _enemy = enemyControl;
        }

        public override void StateInit()
        {
            _enemy.mRigidbody.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;
            _enemy.Anim.applyRootMotion = true;
            
            if (!_enemy.NavAgent.isStopped)
            {
                _enemy.NavAgent.isStopped = true;
                _enemy.NavAgent.updatePosition = false;
                _enemy.NavAgent.updateRotation = false;
            }
        }

        public override void Tick()
        {
            if (!_enemy.Anim.GetBool(id: Strings.animPara_isInteracting))
                _enemy.stateMachine.ChangeState(_enemy.stateCombat);
        }

        public override void StateExit()
        {
            _enemy.Anim.applyRootMotion = false;
            _enemy.mRigidbody.constraints = RigidbodyConstraints.FreezeAll;
            
            if (_enemy.NavAgent.isStopped)
            {
                _enemy.NavAgent.Warp(_enemy.mTransform.position);
                _enemy.NavAgent.isStopped = false;
                _enemy.NavAgent.updatePosition = true;
                _enemy.NavAgent.updateRotation = true;
            }
        }
    }
}