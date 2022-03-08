using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace SK
{
    public class Dodge : MonoBehaviour
    {
        [Header("Dodge")]
        public float dodgeChance = 0.3f;
        [SerializeField]
        private float dodgeAngle = 30;
        [SerializeField]
        private float dodgeDistance = 5f;
        [SerializeField]
        private float dodgeTime = 1f;
        [SerializeField]
        private float counterattackChance = 0.3f;
        
        [Header("Reference")]
        [SerializeField]
        private Enemy enemy;
        
        private Transform thisTransform;
        private NavMeshHit navHit;
        private Vector3 startPos, tempPos;
        

        private float timer;
        [NonSerialized]
        public bool isDodge;

        private void Awake()
        {
            if (!enemy) enemy = GetComponent<Enemy>();
            thisTransform = GetComponent<Transform>();
        }

        private void Update()
        {
            if (isDodge)
                Move();
        }

        public void DodgeAttack()
        {
            thisTransform.rotation = Quaternion.LookRotation(enemy.searchRadar.targetObject.transform.position - thisTransform.position);
            
            // NavMesh의 길이 있는지 파악 후 위치로 닷지
            if (NavMesh.SamplePosition(GetDodgePoint(dodgeAngle), out navHit, dodgeDistance, NavMesh.AllAreas))
            {
                enemy.NavAgent.isStopped = true;
                timer = 0;
                startPos = thisTransform.position;
                enemy.Anim.SetBool(AnimParas.animPara_isInteracting, true);
                enemy.Anim.CrossFade(AnimParas.AnimName_RollBack, 0.2f);
                isDodge = true;
            }
        }

        private void Move()
        {
            timer += Time.deltaTime;
            tempPos.x = EasingFunction.EaseOutCubic(startPos.x, navHit.position.x, timer / dodgeTime);
            tempPos.y = EasingFunction.EaseOutCubic(startPos.y, navHit.position.y, timer / dodgeTime);
            tempPos.z = EasingFunction.EaseOutCubic(startPos.z, navHit.position.z, timer / dodgeTime);
            thisTransform.position = tempPos;
            
            // 이동 완료
            if (timer >= dodgeTime)
            {
                isDodge = false;
                enemy.NavAgent.velocity = Vector3.zero;
                enemy.NavAgent.Warp(thisTransform.position);
                enemy.NavAgent.isStopped = false;

                // 닷지 후 반격
                /*if (enemyClass == EnemyClass.Normal && UnityEngine.Random.value < counterattackChance)
                {
                    navAgent.isStopped = false;
                    navAgent.Warp(thisTransform.position);
                    navAgent.SetDestination(targetObject.transform.position);

                    anim.SetBool(m_AnimPara_isAttack, true);
                    anim.SetTrigger(m_AnimPara_Counterattack);
                }*/
            }
        }

        private Vector3 GetDodgePoint(float angle)
        {
            float randomVal = UnityEngine.Random.Range(-1f, 1f); // 반원에서 방향을 랜덤 값으로 정함
            angle = randomVal * angle; // 0 ~ angle 사이의 각을 구함

            return thisTransform.position + (thisTransform.rotation * Quaternion.Euler(0, angle, 0)) * (Vector3.forward * -dodgeDistance);
        }
    }
}
