using System;
using System.Collections;
using System.Collections.Generic;
using SK.FSM;
using UnityEditorInternal;
using UnityEngine;

namespace SK
{
    public class HumanoidEnemy : Enemy
    {
        public override void Init()
        {
            base.Init();
            
            stateMachine.ChangeState(statePatrol);
        }

        private void Update()
        {
            stateMachine.CurrentState.Tick();

            // MoveBlend
            var moveBlend = Mathf.Clamp01(NavAgent.velocity.magnitude * 0.5f);
            /*if (NavAgent.velocity.magnitude > NavAgent.stoppingDistance + 1.5f)
            {
                moveBlend = moveBlend < 0.95f ? moveBlend + 0.04f : 1;
            }
            else
            {
                moveBlend = moveBlend > 0.05f ? moveBlend - 0.06f : 0;
            }*/
            if (moveBlend < 0.1f) moveBlend = 0;
            Anim.SetFloat(AnimPara_MoveBlend, moveBlend);
            
            if (NavAgent.velocity.sqrMagnitude >= 0.01f && NavAgent.remainingDistance <= 0.1f)
            {
                Anim.SetFloat(AnimPara_MoveBlend, 0); // 걷는 애니메이션 중지
            }
            
            if (stateMachine.CurrentState == stateAttack)
                return;
            
            if (NavAgent.desiredVelocity.sqrMagnitude >= 0.01f)
            {
                // 에이전트의 이동방향
                Vector3 direction = NavAgent.desiredVelocity;
                // 회전각도(쿼터니언) 산출
                Quaternion targetAngle = Quaternion.LookRotation(direction);
                // 선형보간 함수를 이용해 부드러운 회전
                mTransfrom.rotation = Quaternion.Slerp(mTransfrom.rotation, targetAngle, delta * 8.0f);
            }
        }

        private void FixedUpdate()
        {
            stateMachine.CurrentState.FixedTick();
        }

        private void LateUpdate()
        {
            stateMachine.CurrentState.LateTick();
        }
    }
}