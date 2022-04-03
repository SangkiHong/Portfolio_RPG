using UnityEngine;

namespace SK
{
    public class HumanoidEnemy : Enemy
    {
        private float _moveBlend, _walkSpeed;

        public override void Update()
        {
            base.Update();
            
            if (!isInteracting && !dodge.isDodge)
            {
                AnimateMove(); // 애니메이션 MoveBlend
            
                Rotate(); // 움직임에 따른 회전
            }

            if (anim.GetBool(Strings.AnimPara_isFight) && !combat.TargetObject)
            {
                UnassignTarget();
            }
        }
        
        private void Rotate()
        {
            if (navAgent.desiredVelocity.sqrMagnitude >= 0.01f)
            {
                // 에이전트의 이동방향
                Vector3 direction = navAgent.desiredVelocity;

                // 회전각도(쿼터니언) 산출
                Quaternion targetAngle = Quaternion.LookRotation(direction);

                // 선형보간 함수를 이용해 부드러운 회전
                mTransform.rotation = Quaternion.Slerp(mTransform.rotation, targetAngle, delta * 8.0f);
            }
        }

        private void AnimateMove()
        {
            _walkSpeed = navAgent.velocity.magnitude/navAgent.speed;
            
            if (navAgent.velocity.sqrMagnitude >= 0.01f && navAgent.remainingDistance <= 0.1f)
            {
                // 목표지점에 도달 시 애니메이션 중지
                if (anim.GetFloat(Strings.AnimPara_MoveBlend) != 0)                
                    anim.SetFloat(Strings.AnimPara_MoveBlend, 0);                
            }
            else            
                _moveBlend = walkAnimSpeed * _walkSpeed + delta;            
                
            anim.SetFloat(Strings.AnimPara_MoveBlend, _moveBlend);
        }
    }
}