using UnityEngine;

namespace SK
{
    public class HumanoidEnemy : Enemy
    {
        private float _moveBlend, _sideways, _walkSpeed;

        public override void Tick()
        {
            base.Tick();
            
            if (!isInteracting)
            {
                if (dodge != null && dodge.isDodge) return;

                AnimateMove(); // 애니메이션 MoveBlend
            
                Rotate(); // 움직임에 따른 회전
            }

            if (anim.GetBool(Strings.AnimPara_onCombat) && !combat.Target)
            {
                UnassignTarget();
            }
        }
        
        private void Rotate()
        {
            if (navAgent.updateRotation && navAgent.desiredVelocity.sqrMagnitude >= 0.01f)
            {
                // 에이전트의 이동방향
                Vector3 direction = navAgent.desiredVelocity;

                // 회전각도(쿼터니언) 산출
                Quaternion targetAngle = Quaternion.LookRotation(direction);

                // 선형보간 함수를 이용해 부드러운 회전
                mTransform.rotation = Quaternion.Slerp(mTransform.rotation, targetAngle, deltaTime * 8.0f);
            }
        }

        private void AnimateMove()
        {
            _walkSpeed = navAgent.velocity.magnitude/navAgent.speed;

            // 전투 상황이 아닐 경우
            if (stateMachine.CurrentState != stateMachine.stateCombat)
            {
                if (navAgent.velocity.sqrMagnitude <= 0.03f || navAgent.remainingDistance <= 0.15f)
                {
                    var moveBlend = anim.GetFloat(Strings.AnimPara_MoveBlend);
                    // 목표지점에 도달 시 애니메이션 중지
                    _moveBlend = moveBlend > 0 ? moveBlend - 0.07f : 0;
                }
                else
                {
                    _moveBlend = walkAnimSpeed * _walkSpeed + deltaTime;
                }
            }
            // 전투 상황인 경우
            else
            {
                if (navAgent.velocity.sqrMagnitude <= 0.03f || navAgent.remainingDistance <= 0.15f)
                {
                    var moveBlend = anim.GetFloat(Strings.AnimPara_MoveBlend);
                    _moveBlend = moveBlend > 0.02f ? moveBlend - 0.07f : 0;

                    var sideways = anim.GetFloat(Strings.AnimPara_Sideways);
                    _sideways = sideways > 0.02f ? sideways - 0.07f : 0;
                }
                else
                {
                    _moveBlend = stateMachine.stateCombat.MoveDirection.normalized.z;
                    var sideway = -stateMachine.stateCombat.MoveDirection.y / 90;
                    if (sideway > 1) sideway -= (int)sideway; // 정수부 제거
                    _sideways = sideway;
                }

                anim.SetFloat(Strings.AnimPara_Sideways, _sideways);
            }

            anim.SetFloat(Strings.AnimPara_MoveBlend, _moveBlend);
        }

        public override void PlaySoundOnDamage()
        {
            int index = Random.Range(0, Strings.Audio_FX_Voice_Grunt_Male.Length);
            AudioManager.Instance.PlayAudio(Strings.Audio_FX_Voice_Grunt_Male[index], mTransform);
        }

        public override void PlaySoundOnDeath()
        {
            int index = Random.Range(0, Strings.Audio_FX_Voice_Death.Length);
            AudioManager.Instance.PlayAudio(Strings.Audio_FX_Voice_Death[index], mTransform);
        }

        // 발걸음 소리 재생
        public override void FootStepSound()
        {
            int randomindex = Random.Range(0, Strings.Audio_FX_Footstep_Normal.Length);
            AudioManager.Instance.PlayAudio(Strings.Audio_FX_Footstep_Normal[randomindex], mTransform);
        }
    }
}