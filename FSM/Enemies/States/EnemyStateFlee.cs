using UnityEngine;
using UnityEngine.AI;

namespace SK.FSM
{
    public class EnemyStateFlee : StateBase
    {
        private readonly Enemy _enemy;
        private readonly EnemyStateMachine _stateMachine;

        private NavMeshHit _hit;
        private float _fleeDist;

        public EnemyStateFlee(Enemy enemyControl, EnemyStateMachine stateMachine)
        {
            _enemy = enemyControl;
            _stateMachine = stateMachine;
            _fleeDist = _enemy.fleeDistance;
            // 거리 비교 연산을 위해 제곱
            _fleeDist *= _fleeDist;
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

            // Hp 회복
            _enemy.health.Recovering();

            // 도주할 포인트 탐색
            _enemy.navAgent.SetDestination(GetFleePosition());
        }

        public override void FixedTick()
        {
            if (!_enemy.isDead)
            {
                if (_enemy.targetDistance > _fleeDist)
                {
                    _enemy.UnassignTarget();
                    _enemy.anim.SetBool(Strings.AnimPara_onCombat, true);
                    _stateMachine.ResetPatrol();
                }
            }
            else
                StateExit();
        }

        public override void StateExit()
        {
            _enemy.mRigidbody.velocity = Vector3.zero;
            _enemy.navAgent.isStopped = true;
        }

        private Vector3 GetFleePosition()
        {
            if (NavMesh.SamplePosition(GetFleePoint(90), out _hit, _enemy.fleeDistance * 0.5f, NavMesh.AllAreas))
            {
                return _hit.position;
            }
            return Vector3.zero;
        }

        private Vector3 GetFleePoint(float angle)
        {
            float randomVal = Random.Range(-1f, 1f); // 반원에서 방향을 랜덤 값으로 정함
            angle = randomVal * (angle * 0.5f); // angle 사이의 각을 구함

            return _enemy.mTransform.position 
                + (_enemy.combat.Target.transform.rotation * Quaternion.Euler(0, angle, 0)) * (Vector3.forward * _enemy.fleeDistance);
        }
    }
}