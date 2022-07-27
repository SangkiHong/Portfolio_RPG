using UnityEngine;

namespace SK.FSM
{
    public class EnemyStateAttack : StateBase
    {
        private readonly Enemy _enemy;
        private readonly EnemyStateMachine _stateMachine;

        public EnemyStateAttack(Enemy enemyControl, EnemyStateMachine stateMachine)
        {
            _enemy = enemyControl;
            _stateMachine = stateMachine;
        }

        public override void StateInit()
        {
            _enemy.mRigidbody.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;

            _enemy.anim.SetBool(Strings.animPara_isInteracting, true);

            if (!_enemy.navAgent.isStopped)            
                _enemy.navAgent.isStopped = true;            
        }

        public override void Tick()
        {
            if (!_enemy.isInteracting)
                _enemy.stateMachine.ChangeState(_stateMachine.stateCombat);
        }

        public override void StateExit()
        {
            _enemy.onUninterruptible = false;
            _enemy.mRigidbody.constraints = RigidbodyConstraints.FreezeAll;
        }
    }
}