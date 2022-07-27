namespace SK.FSM
{
    public class DodgeState : StateBase
    {
        private readonly Player _player;

        public DodgeState(Player player) => _player = player;

        // 0: forward, 180: backward, 270: left, 90: right,
        // 225: backward left, 135: backward right
        internal int directionAngle; 
        private float _elapsed, _value;
        private bool isBackward;

        public override void StateInit()
        {
            _player.isDodge = true;
            _player.health.SetDamagableState(false); // 무적 상태
            //_player.combat.attackExcuted = true; // 공격 취소

            _elapsed = 0;
            isBackward = directionAngle == 135 || directionAngle == 180 || directionAngle == 225;

            _player.cameraManager.LookAtCameraDirection(directionAngle);

            // Play Animation
            if (isBackward)
                _player.PlayerTargetAnimation(Strings.AnimName_RollBack, true);
            else
            {
                if (directionAngle == 270) 
                    _player.PlayerTargetAnimation(Strings.AnimName_DodgeLeft, true);
                else if (directionAngle == 90)
                    _player.PlayerTargetAnimation(Strings.AnimName_DodgeRight, true);
                else
                    _player.PlayerTargetAnimation(Strings.AnimName_Roll, true);
            }
        }
        
        public override void FixedTick()
        {
            base.FixedTick();
            _player.inputActions.Execute();

            if (_player.isDodge)
            {
                _elapsed += _player.fixedDeltaTime;

                if (isBackward) // 뒤로 후퇴
                    _value = _player.animationCurve_back.Evaluate(_elapsed);
                else // 옆으로 피하기
                    _value = _player.animationCurve_Forward.Evaluate(_elapsed);

                if (_elapsed < 1)
                {
                    // 옆으로 피할 경우
                    if (directionAngle == 270 || directionAngle == 90)
                    {
                        // 방향 체크
                        var dir = directionAngle == 90 ? 0.65f : -0.65f;

                        _player.characterController.SimpleMove(_player.dodgeSpeed * _value * dir * _player.mTransform.right);
                    }
                    else
                    {
                        // 뒤로 갈 경우 체크
                        var dir = !isBackward ? 1 : -0.7f;

                        _player.characterController.SimpleMove(_player.dodgeSpeed * _value * dir * _player.mTransform.forward);
                    }
                }
                else
                {
                    // Animator의 isInteracting Bool 값이 false가 되면 locomotion 상태로 전환
                    _player.monitorInteracting.Execute(_player.stateMachine.locomotionState);
                }

                // 카메라 타겟팅 유지
                _player.targeting.LookAtTarget();          
            }
        }

        public override void StateExit()
        {
            _player.isDodge = false;
            _player.health.SetDamagableState(true);
        }
    }
}