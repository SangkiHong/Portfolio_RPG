using UnityEngine;

namespace SK.FSM
{
    public class AttackState : StateBase
    {
        private readonly Player _player;
        public AttackState(Player player) => _player = player;

        private Vector3 _targetDir;
        private Quaternion _lookRot;

        public override void StateInit()
        {
            _player.onCombatMode = true;
            _player.combat.canComboAttack = false;
            _player.anim.SetBool(Strings.animPara_isInteracting, true);

            if (_player.combat.currentAttack.onRootMotion)
                _player.EnableRootMotion();

            _targetDir = _player.cameraManager.transform.forward;
            _targetDir.y = 0;
            _lookRot = Quaternion.LookRotation(_targetDir);
        }

        public override void FixedTick()
        {
            // Attack 실행되기 전까지 input 받기
            _player.inputActions.Execute();

            _player.mTransform.rotation = Quaternion.Lerp(_player.mTransform.rotation, _lookRot, _player.fixedDeltaTime * _player.rotationSpeed);
        }

        public override void Tick()
        {
            base.Tick();
            _player.monitorInteracting.Execute(_player.stateMachine.locomotionState);
        }
        public override void StateExit()
        {
            _player.combat.canComboAttack = true;
            _player.DisableRootMotion();
        }
    }
}
