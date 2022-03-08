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

            // Interacting 체크
            if (!Anim.GetBool(AnimParas.animPara_isInteracting) && !dodge.isDodge)
            {
                // 애니메이션 MoveBlend 
                AnimateMove();
            
                // 움직임에 따른 회전
                Rotate();
            }

            if (Anim.GetBool(AnimParas.AnimPara_isFight) && !searchRadar.targetObject)
            {
                Anim.SetBool(AnimParas.AnimPara_isFight, false);
                stateMachine.ChangeState(statePatrol);
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
            if (NavAgent.velocity.sqrMagnitude >= 0.01f && NavAgent.remainingDistance <= 0.1f)
            {
                // 목표지점에 도달 시 애니메이션 중지
                Anim.SetFloat(AnimParas.AnimPara_MoveBlend, 0); 
            }
            else
            {
                var moveBlend = Mathf.Clamp01(NavAgent.velocity.magnitude * 0.5f);
                if (moveBlend < 0.1f) moveBlend = 0;
                Anim.SetFloat(AnimParas.AnimPara_MoveBlend, moveBlend);
            }
        }
    }
}