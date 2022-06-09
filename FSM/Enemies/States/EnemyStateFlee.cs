using UnityEngine;
using UnityEngine.AI;

namespace SK.FSM
{
    public class EnemyStateFlee : StateBase
    {
        private readonly Enemy _enemy;
        private readonly EnemyStateMachine _stateMachine;

        private NavMeshHit _hit;

        public EnemyStateFlee(Enemy enemyControl, EnemyStateMachine stateMachine)
        {
            _enemy = enemyControl;
            _stateMachine = stateMachine;
        }

        public override void StateInit()
        {
            _enemy.navAgent.speed = _enemy.enemyData.Speed * 0.8f;
            
            // NavAgent 재가동
            if (_enemy.navAgent.isStopped)
            {
                _enemy.navAgent.velocity = Vector3.zero;
                _enemy.navAgent.isStopped = false;
                _enemy.navAgent.updatePosition = true;
                _enemy.navAgent.updateRotation = true;
            }
            
            _enemy.walkAnimSpeed = 1f;
            
            // Set Anim State
            _enemy.anim.SetBool(Strings.AnimPara_onCombat, false);

            // 도주할 포인트 탐색
            _enemy.navAgent.SetDestination(GetFleePosition());
        }

        public override void FixedTick()
        {
            if (_enemy.navAgent.remainingDistance < 3)
            {
                if (_enemy.targetDistance < _enemy.fleeDistance) // Continue Flee
                    _enemy.navAgent.SetDestination(GetFleePosition());
                else // End Flee
                {
                    _enemy.UnassignTarget();
                    _enemy.anim.SetBool(Strings.AnimPara_onCombat, true);
                    _stateMachine.ResetPatrol();
                }
            }
        }

        public override void StateExit()
        {
            _enemy.mRigidbody.velocity = Vector3.zero;
            _enemy.navAgent.isStopped = true;
        }

        private Vector3 GetFleePosition()
        {
            if (NavMesh.SamplePosition(GetFleePoint(90), out _hit, 3, NavMesh.AllAreas))
            {
                return _hit.position;
            }
            return Vector3.zero;
        }

        private Vector3 GetFleePoint(float angle)
        {
            float randomVal = UnityEngine.Random.Range(-1f, 1f); // 반원에서 방향을 랜덤 값으로 정함
            angle = randomVal * angle; // 0 ~ angle 사이의 각을 구함

            return _enemy.mTransform.position + (_enemy.combat.Target.transform.rotation * Quaternion.Euler(0, angle, 0)) * (Vector3.forward * _enemy.fleeDistance);
        }
    }
}