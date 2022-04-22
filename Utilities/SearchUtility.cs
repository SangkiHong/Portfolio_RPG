using System.Collections.Generic;
using UnityEngine;

namespace SK.Utilities
{
    public class SearchUtility
    {
        private readonly Transform _thisTransform;
        private readonly Collider[] _overlapColliders;
        private GameObject _catchedObject;

        public SearchUtility(Transform _transform) 
        {
            _thisTransform = _transform;
            _overlapColliders = new Collider[20]; // Max Amount for Catched Collider Buffer
        }
        
        public GameObject FindTarget(Vector3 positionOffset, float fov, float viewDist, LayerMask objectMask)
        {
            GameObject objectFound;
            _catchedObject = null;

            var hitCount = Physics.OverlapSphereNonAlloc(_thisTransform.TransformPoint(positionOffset), viewDist, _overlapColliders, objectMask, QueryTriggerInteraction.Ignore);
            if (hitCount > 0)
            {
                float minAngle = Mathf.Infinity;
                for (int i = 0; i < hitCount; ++i)
                {
                    float angle;

                    if ((objectFound = SearchWithinSight(positionOffset, fov, viewDist, _overlapColliders[i].gameObject, out angle, objectMask)) != null)
                    {
                        if (angle < minAngle)
                        {
                            minAngle = angle;
                            _catchedObject = objectFound;
                        }
                    }
                }
            }
            return _catchedObject;
        }

        public void FindTargets(Vector3 positionOffset, float fov, float viewDist, ref List<GameObject> objects, LayerMask objectMask)
        {
            var hitCount = Physics.OverlapSphereNonAlloc(_thisTransform.TransformPoint(positionOffset), viewDist, _overlapColliders, objectMask, QueryTriggerInteraction.Ignore);

            if (hitCount > 0)
            {
                for (int i = 0; i < hitCount; ++i)
                {
                    float angle = 0;

                    if ((_catchedObject = SearchWithinSight(positionOffset, fov, viewDist, _overlapColliders[i].gameObject, out angle, objectMask)) != null)
                    {
                        if (!objects.Find(x => x.Equals(_catchedObject)))
                            objects.Add(_catchedObject);
                    }
                }
            }
        }

        private GameObject SearchWithinSight(Vector3 positionOffset, float fov, float viewDist, GameObject target, out float angle, int objectMask)
        {
            if (target == null)
            {
                angle = 0;
                return null;
            }

            var dir = target.transform.position - _thisTransform.position;
            
            angle = Vector3.Angle(dir, _thisTransform.forward);
            dir.y = 0;

            if (dir.magnitude < viewDist && angle < fov * 0.5f)
            {
                if (LineOfSight(positionOffset, target, objectMask) != null)
                {
                    return target;
                }
            }

            return null;
        }

        private GameObject LineOfSight(Vector3 positionOffset, GameObject targetObject, int objectMask)
        {
            RaycastHit hit;
            if (Physics.Linecast(_thisTransform.TransformPoint(positionOffset),
                targetObject.transform.position + Vector3.up * positionOffset.y, out hit, objectMask, QueryTriggerInteraction.Ignore))
            {
                if (hit.transform.IsChildOf(targetObject.transform) || targetObject.transform.IsChildOf(hit.transform) || hit.transform == targetObject.transform)
                    return targetObject; 
            }
            return null;
        }
    }
}