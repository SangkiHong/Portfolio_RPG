using SK.FSM;
using System.Collections;
using UnityEngine;

namespace SK
{
    public class EnemyDemon : HumanoidEnemy
    {
        [SerializeField] private float defenseDistance = 2.5f;
        [SerializeField] private float defenseDuration = 3f;
        [SerializeField] private float defenseIntervalMin = 5f;
        [SerializeField] private float defenseIntervalMax = 10f;

        private readonly int _animPara_GuardRelease = Animator.StringToHash("GuardRelease");
        private readonly string _animName_BattleCry = "BattleCry";
        private readonly string _animName_Guard = "Guard";
        private readonly string _animName_Block = "Block";

        private bool _isFindTarget;
        private bool _isOnGuard;
        private float _defenseElapsed, _defenseDurationElapsed, _defenseSetTime, _defenseDist;

        public override void Awake()
        {
            base.Awake();
            // 거리 비교 연산을 위해 제곱
            _defenseDist = defenseDistance * defenseDistance;
            // 디펜스 간격 시간 랜덤 지정
            _defenseSetTime = Random.Range(defenseIntervalMin, defenseIntervalMax);
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

            // 디펜스 상태
            if (combat.Target && !isInteracting && !isDead && targetDistance <= _defenseDist)
            {
                // 공격 상태인 경우 리턴
                if (stateMachine.CurrentState == stateMachine.stateAttack) return;

                if (_defenseElapsed >= _defenseSetTime)
                {
                    // 디펜스 상태로 전환
                    if (!_isOnGuard)
                    {
                        _isOnGuard = true;

                        _defenseDurationElapsed = 0;

                        // 상태 머신 일시정지
                        stateMachine.StopMachine(true);

                        // 디펜스 간격 시간 랜덤 지정
                        _defenseSetTime = Random.Range(defenseIntervalMin, defenseIntervalMax);

                        // 디펜스 애니메이션 재생
                        anim.CrossFade(_animName_Guard, 0.15f);
                    }
                    // 디펜스 유지 시간
                    else
                    {
                        // 디펜스 유지 시간 도달한 경우 디펜스 해제
                        if (_defenseDurationElapsed >= defenseDuration)
                            ReleaseGuard();
                        else
                            _defenseDurationElapsed += fixedDeltaTime;

                        // 타겟을 바라보도록 회전
                        RotateToTarget();
                    }
                }
                else
                    _defenseElapsed += fixedDeltaTime;
            }
            else
                ReleaseGuard();
        }

        private void ReleaseGuard()
        {
            if (_isOnGuard)
            {
                _isOnGuard = false;

                // 디펜스 쿨타임 초기화
                _defenseElapsed = 0;

                // 상태 머신 재작동
                stateMachine.RecoverMachine(true);

                // 디펜스 해제 애니메이터 파라미터 변경
                anim.SetTrigger(_animPara_GuardRelease);
            }
        }

        public override void OnDamage(Unit attacker, uint damage, bool isStrong)
        {
            if (!_isOnGuard)
                base.OnDamage(attacker, damage, isStrong);
            else
                anim.CrossFade(_animName_Block, 0.1f);
        }

        // 피격 시 소리 재생
        public override void PlaySoundOnDamage()
        {
            int index = Random.Range(0, Strings.Audio_FX_Voice_Troll_Pain.Length);
            AudioManager.Instance.PlayAudio(Strings.Audio_FX_Voice_Troll_Pain[index], mTransform);
        }

        // 죽음 시 소리 재생
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