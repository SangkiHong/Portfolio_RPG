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
        }

        public override void Tick()
        {
            if (!_enemy.Anim.GetBool(id: AnimParas.animPara_isInteracting))
                _enemy.stateMachine.ChangeState(_enemy.stateChase);
        }

        public override void StateExit()
        {
            _enemy.Anim.applyRootMotion = false;
            _enemy.mRigidbody.constraints = RigidbodyConstraints.FreezeAll;
            _enemy.dodge.DodgeAttack();
        }
    }
}