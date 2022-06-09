using UnityEngine;

namespace SK.FSM
{
    public class KnockBackState : StateBase
    {
        private readonly Player _state;
        private Transform _attackerTr;

        public KnockBackState(Player player) 
        { 
            _state = player;

            // Event 등록
            player.OnKnockBackState += SetKnockBack;
        }

        private Vector3 _direction;

        private float _elapsed, _duration, _power;

        // 상태 진입 시 적으로부터 플레이어를 향한 방향을 변수에 저장
        public override void StateInit()
            => _direction = (_state.mTransform.position - _attackerTr.position).normalized;

        public override void FixedTick()
        {
            if (_elapsed < _duration)
            {
                _elapsed += _state.fixedDeltaTime;
                _state.characterController.Move(_direction * _power);
            }
            else            
                _state.stateMachine.ChangeState(_state.stateMachine.locomotionState);            
        }

        // 넉백 시 호출 시 인자 값을 변수에 할당 후 상태 변경하는 함수
        private void SetKnockBack(Transform attackerTr, float duration = 0.2f, float power = 0.1f)
        {
            _elapsed = 0;
            _attackerTr = attackerTr;
            _duration = duration;
            _power = power;
            if (_state.stateMachine.CurrentState != _state.stateMachine.knockBackState)
                _state.stateMachine.ChangeState(this);
        }

        // 소멸자 Event 해제
        ~KnockBackState() => _state.OnKnockBackState -= SetKnockBack;
    }
}
