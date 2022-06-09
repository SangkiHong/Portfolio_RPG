using System.Collections.Generic;
using UnityEngine;

namespace SK.Utilities
{
    public class SearchUtility
    {
        // by.상기_검색한 콜라이더를 임시로 저장할 버퍼 배열
        private readonly Collider[] _overlapColliders;

        // by.상기_현재 오브젝트의 트랜스폼
        private readonly Transform _thisTransform;

        // by.상기_탐색된 게임오브젝트를 최종 반환할 변수
        private GameObject _catchedObject;

        // by.상기_라인캐스트의 충돌 반환할 RaycastHit 변수
        private RaycastHit _hit;

        // by.상기_생성자(현재 트랜스폼)_220516
        public SearchUtility(Transform _transform) 
        {
            _thisTransform = _transform;
            _overlapColliders = new Collider[20]; // 탐색된 콜라이더를 담을 콜라이더 버퍼 배열
        }

        /// <summary>
        /// by.상기_타겟을 탐색하는 함수_220516
        /// </summary>
        /// <param name="positionOffset">탐색을 시작하는 위치의 오프셋</param>
        /// <param name="fov">탐색 시야 각도</param>
        /// <param name="searchDist">탐색 가능 거리</param>
        /// <param name="objectMask">타겟 레이어 마스크</param>
        /// <returns></returns>
        public GameObject SearchTarget(Vector3 positionOffset, float fov, float searchDist, LayerMask objectMask)
        {
            // 변수 초기화
            _catchedObject = null;

            // Physics.OverlapSphereNonAlloc 함수를 통해 현재 트랜스폼을 기준으로 탐색 범위 안에 있으며 레이어마스크에 해당하는 콜라이더를 버퍼에 할당함
            var hitCount = Physics.OverlapSphereNonAlloc(_thisTransform.TransformPoint(positionOffset), searchDist, _overlapColliders, objectMask, QueryTriggerInteraction.Ignore);
            
            // 탐색된 콜라이더 갯수가 0 이상일 경우
            if (hitCount > 0)
            {
                // 최소 각도에 위치한 타겟을 판별하기 위한 float형 변수
                float minAngle = Mathf.Infinity;

                for (int i = 0; i < hitCount; ++i)
                {
                    // 탐색된 오브젝트와 현재 오브젝트의 각도를 저장할 float형 변수
                    float angle;

                    // 탐색된 오브젝트가 전방 탐색 각도(fov) 내에 있으며, 탐색 범위(searchDist) 내에 있으면 _foundObject에 할당 함
                    if (SearchWithinSight(positionOffset, fov, searchDist, _overlapColliders[i].gameObject, out angle, objectMask))
                    {
                        // 탐색된 오브젝트의 각도가 최소 각도보다 작을 경우
                        if (angle < minAngle)
                        {
                            // 최소 각도 업데이트
                            minAngle = angle;

                            // 탐색된 오브젝트를 변수에 저장
                            _catchedObject = _overlapColliders[i].gameObject;
                        }
                    }
                }
            }
            return _catchedObject;
        }

        /// <summary>
        /// by.상기_타겟을 모두 탐색하여 참조된 리스트에 추가하는 함수_220516
        /// </summary>
        /// <param name="positionOffset">탐색을 시작하는 위치의 오프셋</param>
        /// <param name="fov">탐색 시야 각도</param>
        /// <param name="searchDist">탐색 가능 거리</param>
        /// <param name="objectList">탐색한 오브젝트를 담을 참조 리스트</param>
        /// <param name="objectMask">타겟 레이어 마스크</param>
        public void SearchTargets(Vector3 positionOffset, float fov, float searchDist, ref SortedSet<int> objectList, LayerMask objectMask)
        {
            // Physics.OverlapSphereNonAlloc 함수를 통해 현재 트랜스폼을 기준으로 탐색 범위 안에 있으며 레이어마스크에 해당하는 콜라이더를 버퍼에 할당함
            var hitCount = Physics.OverlapSphereNonAlloc(_thisTransform.TransformPoint(positionOffset), searchDist, _overlapColliders, objectMask, QueryTriggerInteraction.Ignore);

            // 탐색된 콜라이더 갯수가 0 이상일 경우
            if (hitCount > 0)
            {
                for (int i = 0; i < hitCount; ++i)
                {
                    // 탐색된 오브젝트와 현재 오브젝트의 각도를 저장할 float형 변수
                    float angle = 0;

                    // 탐색된 오브젝트가 전방 탐색 각도(fov) 내에 있으며, 시야 탐색 범위(searchDist) 내에 있으면 _foundObject에 할당 함
                    if (SearchWithinSight(positionOffset, fov, searchDist, _overlapColliders[i].gameObject, out angle, objectMask))
                    {
                        // 리스트 내에 동일한 오브젝트가 없으면 리스트에 오브젝트를 추가
                            objectList.Add(_overlapColliders[i].gameObject.GetInstanceID());
                    }
                }
            }
        }

        /// <summary>
        /// by.상기_타겟 오브젝트가 시야 범위 내에 있으며, 시야 각도 내에 있는지에 대한 여부를 반환하는 함수_220516
        /// </summary>
        /// <param name="positionOffset">탐색 시작 오프셋</param>
        /// <param name="fov">탐색 시야 각도</param>
        /// <param name="searchDist">탐색 가능 거리</param>
        /// <param name="target">대상 오브젝트</param>
        /// <param name="angle">반환할 오브젝트의 각도</param>
        /// <param name="objectMask">타겟 레이어마스크</param>
        /// <returns></returns>
        private bool SearchWithinSight(Vector3 positionOffset, float fov, float searchDist, GameObject target, out float angle, int objectMask)
        {
            // 타겟 오브젝트가 null이면 false 반환
            if (target == null)
            {
                angle = 0;
                return false;
            }

            // 타겟 오브젝트에 대한 방향
            var dir = target.transform.position - _thisTransform.position;

            // Vector3.Angle 함수로 타겟 오브젝트의 각도를 구함
            angle = Vector3.Angle(dir, _thisTransform.forward);
            
            // 방향에서 높이를 제외
            dir.y = 0;

            // 탐색 가능 거리 안에 있으며, 시야 범위 내에 오브젝트가 위치하고 있는 경우
            if (dir.magnitude < searchDist && angle < fov * 0.5f)
            {
                // LineOfSight 함수를 통해 현재 오브젝트와 대상 오브젝트 사이에 충돌하는 것이 없는지에 대한 여부를 확인
                if (LineOfSight(positionOffset, target, objectMask))                
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 현재 오브젝트에서 대상 오브젝트를 향해 라인캐스트를 발사하여 충돌하는 다른 오브젝트가 없는지 확인하는 함수
        /// </summary>
        /// <param name="positionOffset"></param>
        /// <param name="targetObject"></param>
        /// <param name="objectMask"></param>
        /// <returns></returns>
        private bool LineOfSight(Vector3 positionOffset, GameObject targetObject, int objectMask)
        {
            // 현재 오브젝트와 타겟 오브젝트 사이에 충돌하는 것이 있는지를 검사할 라인캐스트 함수 활용
            if (Physics.Linecast(_thisTransform.TransformPoint(positionOffset),
                targetObject.transform.position + Vector3.up * positionOffset.y, out _hit, objectMask, QueryTriggerInteraction.Ignore))
            {
                // 타겟 오브젝트가 충돌 트랜스폼의 자식이거나, 타겟 오브젝트의 자식 트랜스폼이거나, 충돌 오브젝트와 타겟 오브젝트가 같은 경우 true 반환
                if (_hit.transform.IsChildOf(targetObject.transform) || targetObject.transform.IsChildOf(_hit.transform) || _hit.transform == targetObject.transform)
                    return true; 
            }
            // 어느 경우에도 속하지 않으면 false 반환
            return false;
        }
    }
}