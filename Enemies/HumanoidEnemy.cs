using UnityEngine;

namespace SK
{
    public class HumanoidEnemy : Enemy
    {
        private float _moveBlend, _walkSpeed;
        
        public override void Init()
        {
            base.Init();
            
            stateMachine.ChangeState(statePatrol); // 기본 상태로 변경
        }

        public override void Update()
        {
            base.Update();
            
            if (!isInteracting && !dodge.isDodge)
            {
                AnimateMove(); // 애니메이션 MoveBlend
            
                Rotate(); // 움직임에 따른 회전
            }

            if (Anim.GetBool(Strings.AnimPara_isFight) && !targetObject)
            {
                Anim.SetBool(Strings.AnimPara_isFight, false);
                stateMachine.ChangeState(statePatrol);
            }
        }
        
        private void Rotate()
        {
            if (NavAgent.desiredVelocity.sqrMagnitude >= 0.01f)
            {
                // 에이전트의 이동방향
                Vector3 direction = NavAgent.desiredVelocity;

                // 회전각도(쿼터니언) 산출
                Quaternion targetAngle = Quaternion.LookRotation(direction);

                // 선형보간 함수를 이용해 부드러운 회전
                mTransform.rotation = Quaternion.Slerp(mTransform.rotation, targetAngle, delta * 8.0f);
            }
        }

        private void AnimateMove()
        {
            _walkSpeed = NavAgent.velocity.magnitude/NavAgent.speed;
            
            if (NavAgent.velocity.sqrMagnitude >= 0.01f && NavAgent.remainingDistance <= 0.1f)
            {
                // 목표지점에 도달 시 애니메이션 중지
                if (Anim.GetFloat(Strings.AnimPara_MoveBlend) != 0)                
                    Anim.SetFloat(Strings.AnimPara_MoveBlend, 0);                
            }
            else            
                _moveBlend = walkAnimSpeed * _walkSpeed + delta;            
                
            Anim.SetFloat(Strings.AnimPara_MoveBlend, _moveBlend);
        }
    }
}