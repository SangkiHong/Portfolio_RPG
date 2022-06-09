using System.Collections;
using UnityEngine;

namespace SK
{
    public class EnemyTroll : HumanoidEnemy
    {
        [SerializeField] private float nearAttackDistance = 2.5f;
        [SerializeField] private float nearAttackInterval = 5f;

        private readonly string _animName_Jumping = "Troll_Jumping"; 
        private readonly string _animName_BattleCry = "BattleCry";
        private bool _isFindTarget;
        private float _attackElapsed;

        public override void FixedTick()
        {
            base.FixedTick();

            // 타겟을 처음 발견하여 전투 외침(Battle Cry) 시전
            if (combat.Target && !_isFindTarget)
            {
                _isFindTarget = true;
                anim.CrossFade(_animName_BattleCry, 0.1f);
            }
            else if (!combat.Target && _isFindTarget)
                _isFindTarget = false;

            if (combat.Target && !isInteracting && !isDead && targetDistance <= nearAttackDistance)
            {
                if (_attackElapsed >= nearAttackInterval)
                {
                    _attackElapsed = 0;

                    // 방해 받지 않는 상태로 전환
                    uninterruptibleState = true;

                    // 공격 젠 초기화
                    stateMachine.stateCombat.ResetElapsed();
                    // 공격 상태로 전환
                    stateMachine.ChangeState(stateMachine.stateAttack); 
                    // 애니메이션 전환
                    anim.CrossFade(_animName_Jumping, 0.15f);
                }
                else
                    _attackElapsed += fixedDeltaTime;                
            }
        }
    }
}