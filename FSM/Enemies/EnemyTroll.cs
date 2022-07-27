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
        private float _attackElapsed, _attackDist;

        public override void Awake()
        {
            base.Awake();
            // 거리 비교 연산을 위해 제곱
            _attackDist = nearAttackDistance * nearAttackDistance;
        }

        public override void FixedTick()
        {
            base.FixedTick();

            // 타겟을 처음 발견하여 전투 외침(Battle Cry) 시전
            if (combat.Target && !_isFindTarget)
            {
                _isFindTarget = true;
                anim.CrossFade(_animName_BattleCry, 0.1f);
            }

            if (combat.Target && !isInteracting && !isDead && targetDistance <= _attackDist)
            {
                if (_attackElapsed >= nearAttackInterval)
                {
                    _attackElapsed = 0;

                    // 방해 받지 않는 상태로 전환
                    onUninterruptible = true;

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

        public override void PlaySoundOnDamage()
        {
            int index = Random.Range(0, Strings.Audio_FX_Voice_Troll_Pain.Length);
            AudioManager.Instance.PlayAudio(Strings.Audio_FX_Voice_Troll_Pain[index], mTransform);
        }

        public override void PlaySoundOnDeath()
        {
            AudioManager.Instance.PlayAudio(Strings.Audio_FX_Voice_Troll_Death, mTransform);
        }

        // 발걸음 소리 재생
        public override void FootStepSound()
        {
            int index = Random.Range(0, Strings.Audio_FX_Footstep_Heavy.Length);
            AudioManager.Instance.PlayAudio(Strings.Audio_FX_Footstep_Heavy[index], mTransform);
        }
    }
}