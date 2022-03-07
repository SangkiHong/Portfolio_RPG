using UnityEngine;

namespace SK.FSM
{
    public class StateAttack : EnemyState
    {
        private readonly Enemy _enemy;

        private Collider[] _colliders;
        private float targetDist, attackTimer;
        
        public StateAttack(Enemy enemyControl)
        {
            _enemy = enemyControl;
            
            attackTimer = _enemy.AttackCooldown * 0.3f;
        }

        public override void StateInit()
        {
            _enemy.Anim.applyRootMotion = true;
        }

        public override void StateExit()
        {
            _enemy.Anim.applyRootMotion = false;
        }

        public override void Tick()
        {
            var position = _enemy.mTransfrom.position + _enemy.AttackColOffset;
            var rotation = _enemy.mTransfrom.rotation;
            bool isOddNum = _enemy.AttackColSize % 2 != 0; // 박스 갯수가 홀수인지 판별
            
            for (int i = 0; i < _enemy.AttackColSize; i++)
            {
                Physics.OverlapBoxNonAlloc(position, _enemy.AttackColScale * 0.5f, _colliders, rotation);
            }
        }

        public override void FixedTick()
        {
            if (_enemy.Anim.GetBool(_enemy.AnimPara_isInteracting))
                return;
            
            base.FixedTick();
            if (!_enemy.NavAgent.isStopped)
            {
                _enemy.NavAgent.isStopped = true;
                //_enemy.NavAgent.updatePosition = false;
                _enemy.NavAgent.updateRotation = false;
            }

            // TARGET 과 거리 측정 후 STATE 재설정(공격 중이 아닐 때)
            targetDist = Vector3.Distance(_enemy.mTransfrom.position, _enemy.searchRadar.TargetObject.transform.position);
            if (targetDist > _enemy.NavAgent.stoppingDistance + 0.1f)
            {
                _enemy.stateMachine.ChangeState(_enemy.stateChase);
                return;
            }

            // Attack Cooldown
            if (0 < attackTimer)
            {
                if (!_enemy.isDamaged) attackTimer -= _enemy.fixedDelta;
            }
            // Do Attack
            else
            {
                if (!_enemy.isDamaged)
                {
                    attackTimer = _enemy.AttackCooldown;

                    if (targetDist <= _enemy.NavAgent.stoppingDistance + 0.1f)
                    {
                        _enemy.Anim.SetBool(_enemy.AnimPara_isInteracting, true);
                        _enemy.Anim.CrossFade(_enemy.AnimPara_Attack, 0.2f);
                    }
                }
            }
        }
        
        public override void LateTick()
        {
            base.LateTick();
            FollowTarget();
        }

        // Targeting on FIGHT MODE
        private void FollowTarget()
        {
            Vector3 dir = _enemy.searchRadar.TargetObject.transform.position - _enemy.mTransfrom.position;
            _enemy.mTransfrom.rotation = Quaternion.Lerp(_enemy.mTransfrom.rotation, Quaternion.LookRotation(dir), _enemy.delta * _enemy.LookTargetSpeed);
        }
    }
}