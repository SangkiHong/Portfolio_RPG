using System;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace SK.Utilities
{
    // AI의 순찰과 타겟 탐색 기능_220516
    public class SearchRadar : MonoBehaviour
    {
        // Draw line 디버그_220516
        [SerializeField] private bool debugRader;

        [Header("Patrol")]
        // 순찰 시작 지점으로부터 순찰 범위 지정_220516
        [SerializeField] internal float patrolDistance = 10f;
        // 순찰 목표 지점에 다다랐을 경우 아이들 상태 시간_220516
        public float idleDuration = 5f;

        [Header("Search Radar")]
        // 타겟 레이어 마스크
        [SerializeField] private LayerMask targetLayerMask;
        // 전방 탐색 각도
        [SerializeField] private float fieldOfViewAngle = 150;
        // 탐색 가능 거리
        [SerializeField] internal float searchDistance = 15;
        // 탐색 시작 위치 오프셋
        [SerializeField] private Vector3 radarOffset;

        internal SearchUtility searchUtility;

        private Transform _thisTransform;
        private NavMeshHit _navHit;

        private Vector3 _originPos;

        private void Awake()
        {
            _thisTransform = transform;
            searchUtility = new SearchUtility(_thisTransform);
        }

        // 탐색 시작 위치를 변수에 저장_220516
        private void Start() => _originPos = _thisTransform.position;

        // SearchUtility를 통해 타겟 오브젝트 탐색_220516
        public GameObject FindTarget()
        { return searchUtility.SearchTarget(radarOffset, fieldOfViewAngle, searchDistance, targetLayerMask); }

        // 시작 위치를 중심으로 순찰 거리 안의 랜덤 위치를 반환하는 함수_220516
        public Vector3 GetPatrolPoint(float distance)
        {
            // Random.insideUnitSphere으로 랜덤한 위치 값을 가져옴
            Vector3 randomDirection = Random.insideUnitSphere * distance;

            // 랜덤 위치 값에 순찰 시작 지점을 더하여 순찰 지점을 중심으로 랜덤 위치를 구함
            randomDirection += _originPos;

            // NavMesh.SamplePosition 함수를 통해 랜덤 위치에 NavMeshAgent가 이동 가능한 위치인지 판별하여 _navHit에 할당
            NavMesh.SamplePosition(randomDirection, out _navHit, distance, NavMesh.AllAreas);

            // _navHit에 할당된 position 값을 반환
            return _navHit.position;
        }

        // 순찰 지점을 현재 트랜스폼의 위치로 변경_220516
        public void ResetOriginPos() => _originPos = _thisTransform.position;

        #region Debug Sight
        // 순찰 범위 및 타겟 탐색 범위 디버그_220516
        private void OnDrawGizmosSelected()
        {
#if UNITY_EDITOR
            if (debugRader)
            {
                var oldColor = UnityEditor.Handles.color;
                var color = Color.yellow;
                color.a = 0.1f;
                UnityEditor.Handles.color = color;

                var halfFOV = fieldOfViewAngle * 0.5f;
                var beginDirection = Quaternion.AngleAxis(-halfFOV, transform.up) * transform.forward;
                UnityEditor.Handles.DrawSolidArc(transform.TransformPoint(radarOffset), transform.up, beginDirection, fieldOfViewAngle, searchDistance);

                UnityEditor.Handles.color = oldColor;
            }
#endif
        }
        #endregion
    }
}