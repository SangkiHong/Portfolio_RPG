using UnityEngine;

namespace SK.FSM
{
    public class KnockBackState : StateBase
    {
        private readonly PlayerStateManager _state;
        public KnockBackState(PlayerStateManager psm) 
        { 
            _state = psm;

            // Event 등록
            psm.OnKnockBack += SetKnockBack;
        }

        private Vector3 _direction;
        private float _elapsed, _duration, _power;

        public override void StateInit()
        {
            _direction = (_state.mTransform.position - _state.health.hitTransform.position).normalized;
        }

        public override void FixedTick()
        {
            if (_elapsed < _duration)
            {
                _elapsed += _state.fixedDelta;
                _state.characterController.Move(_direction * _power);
            }
            else            
                _state.stateMachine.ChangeState(_state.stateMachine.locomotionState);            
        }

        private void SetKnockBack(float duration = 0.2f, float power = 0.1f)
        {
            _elapsed = 0;
            _duration = duration;
            _power = power;
            if (_state.stateMachine.CurrentState != _state.stateMachine.knockBackState)
                _state.stateMachine.ChangeState(this);
        }

        // 소멸자 Event 해제
        ~KnockBackState() => _state.OnKnockBack -= SetKnockBack;
    }
}
