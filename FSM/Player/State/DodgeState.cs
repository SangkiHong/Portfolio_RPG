using UnityEngine;

namespace SK.FSM
{
    public class DodgeState : StateBase
    {
        private readonly PlayerStateManager _state;

        public DodgeState(PlayerStateManager psm) => _state = psm;

        internal int directionNum; // 0: backward, 1: left, 2: right
        private float _elapsed, _value;

        public override void StateInit()
        {
            _state.isDodge = true;
            _state.health.canDamage = false; // 무적 상태
            _state.combat.attackExcuted = true; // 공격 취소

            _elapsed = 0;
            _state.cameraManager.lockOnCamera.m_RecenterToTargetHeading.m_enabled = false;
            _state.cameraManager.LookAtCameraDirection(directionNum);
            
            // Play Animation
            if (directionNum != 0)
                _state.PlayerTargetAnimation(Strings.AnimName_Roll, true);
            else
                _state.PlayerTargetAnimation(Strings.AnimName_RollBack, true);
        }
        
        public override void FixedTick()
        {
            base.FixedTick();
            _state.inputManager.Execute();

            if (_state.isDodge)
            {
                _elapsed += _state.fixedDelta;

                if (directionNum != 0) // 앞으로 구르기
                    _value = _state.animationCurve_Forward.Evaluate(_elapsed);
                else // 뒤로 후퇴
                    _value = _state.animationCurve_back.Evaluate(_elapsed);

                if (_elapsed < 1)
                {
                    // 뒤로 갈 경우 체크
                    var dir = directionNum != 0 ? 1 : -0.7f;

                    _state.characterController.SimpleMove(_state.mTransform.forward * dir * _value * _state.dodgeSpeed);
                }
                else
                {
                    // Animator의 isInteracting Bool 값이 false가 되면 locomotion 상태로 전환
                    if (_state.monitorInteracting.Execute())                    
                        _state.stateMachine.ChangeState(_state.stateMachine.locomotionState);                    
                }
            }
        }

        public override void StateExit()
        {
            _state.isDodge = false;
            _state.health.canDamage = true;
            _state.cameraManager.lockOnCamera.m_RecenterToTargetHeading.m_enabled = true;
        }
    }
}