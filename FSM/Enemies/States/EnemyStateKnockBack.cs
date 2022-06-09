using UnityEngine;

namespace SK.FSM
{
    public class EnemyStateKnockBack : StateBase
    {
        private readonly Enemy _enemy;
        private readonly EnemyStateMachine _stateMachine;

        private Transform _attackerTr;
        private Vector3 _direction;
        private float _elapsed, _duration, _power;

        public EnemyStateKnockBack(Enemy enemy, EnemyStateMachine stateMachine)
        {
            _enemy = enemy;
            _stateMachine = stateMachine;
            _enemy.OnKnockBackState += SetKnockBack;
        }

        public override void StateInit()
        {
            _enemy.navAgent.updateRotation = false;
            _direction = (_enemy.mTransform.position - _attackerTr.position).normalized;
        }

        public override void FixedTick()
        {
            if (_elapsed < _duration)
            {
                _elapsed += _enemy.fixedDeltaTime;
                _enemy.navAgent.Move(_direction * _power);
            }
            else
            {
                _enemy.anim.SetBool(Strings.animPara_isInteracting, false);
                _stateMachine.ChangeState(_stateMachine.stateCombat);
            }
        }

        public void SetKnockBack(Transform attackerTr, float duration = 0.3f, float power = 0.15f)
        {
            _elapsed = 0;
            _attackerTr = attackerTr;
            _duration = duration;
            _power = power;
            _stateMachine.ChangeState(_stateMachine.stateKnockBack);
        }

        ~EnemyStateKnockBack()=> _enemy.OnKnockBackState -= SetKnockBack;        
    }
}
