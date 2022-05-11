using UnityEngine;

namespace SK.FSM
{
    public class StateKnockBack : StateBase
    {
        private readonly Enemy _enemy;
        private readonly EnemyStateMachine _stateMachine;

        internal bool onStateKnockBack;
        private Vector3 _direction;
        private float _elapsed, _duration, _power;

        public StateKnockBack(Enemy enemyControl, EnemyStateMachine stateMachine)
        {
            _enemy = enemyControl;
            _stateMachine = stateMachine;
            _enemy.OnKnockBack += SetKnockBack;
        }

        public override void StateInit()
        {
            _enemy.navAgent.updateRotation = false;
            _direction = (_enemy.mTransform.position - _enemy.health.hitTransform.position).normalized;
        }

        public override void FixedTick()
        {
            if (_elapsed < _duration)
            {
                _elapsed += _enemy.fixedDelta;
                _enemy.navAgent.Move(_direction * _power);
            }
            else
            {
                onStateKnockBack = false;
                _enemy.anim.SetBool(Strings.animPara_isInteracting, false);
                _stateMachine.ChangeState(_stateMachine.stateCombat);
            }
        }

        public void SetKnockBack(float duration = 0.3f, float power = 0.15f)
        {
            _elapsed = 0;
            _duration = duration;
            _power = power;
            onStateKnockBack = true;
            _stateMachine.ChangeState(_stateMachine.stateKnockBack);
        }

        ~StateKnockBack()=> _enemy.OnKnockBack -= SetKnockBack;        
    }
}
