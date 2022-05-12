namespace SK.FSM
{
    public class DodgeState : StateBase
    {
        private readonly PlayerStateManager _state;

        public DodgeState(PlayerStateManager psm) => _state = psm;

        // 0: forward, 180: backward, 270: left, 90: right,
        // 225: backward left, 135: backward right
        internal int directionAngle; 
        private float _elapsed, _value;
        private bool isBackward;

        public override void StateInit()
        {
            _state.isDodge = true;
            _state.health.CanDamage = false; // 무적 상태
            _state.combat.attackExcuted = true; // 공격 취소

            _elapsed = 0;
            isBackward = directionAngle == 135 || directionAngle == 180 || directionAngle == 225;

            _state.cameraManager.LookAtCameraDirection(directionAngle);

            // Play Animation
            if (isBackward)
                _state.PlayerTargetAnimation(Strings.AnimName_RollBack, true);
            else
            {
                if (directionAngle == 270) 
                    _state.PlayerTargetAnimation(Strings.AnimName_DodgeLeft, true);
                else if (directionAngle == 90)
                    _state.PlayerTargetAnimation(Strings.AnimName_DodgeRight, true);
                else
                    _state.PlayerTargetAnimation(Strings.AnimName_Roll, true);
            }
        }
        
        public override void FixedTick()
        {
            base.FixedTick();
            _state.playerInputs.Execute();

            if (_state.isDodge)
            {
                _elapsed += _state.fixedDelta;

                if (isBackward) // 뒤로 후퇴
                    _value = _state.animationCurve_back.Evaluate(_elapsed);
                else // 옆으로 피하기
                    _value = _state.animationCurve_Forward.Evaluate(_elapsed);

                if (_elapsed < 1)
                {
                    // 옆으로 피할 경우
                    if (directionAngle == 270 || directionAngle == 90)
                    {
                        // 방향 체크
                        var dir = directionAngle == 90 ? 0.65f : -0.65f;

                        _state.characterController.SimpleMove(_state.dodgeSpeed * _value * dir * _state.mTransform.right);
                    }
                    else
                    {
                        // 뒤로 갈 경우 체크
                        var dir = !isBackward ? 1 : -0.7f;

                        _state.characterController.SimpleMove(_state.dodgeSpeed * _value * dir * _state.mTransform.forward);
                    }
                }
                else
                {
                    // Animator의 isInteracting Bool 값이 false가 되면 locomotion 상태로 전환
                    _state.monitorInteracting.Execute(_state.stateMachine.locomotionState);
                }

                // 카메라 타겟팅 유지
                if (_state.isTargeting)                
                    _state.cameraTarget.LookAt(_state.targetEnemy);                
            }
        }

        public override void StateExit()
        {
            _state.isDodge = false;
            _state.health.CanDamage = true;
        }
    }
}