using System.Collections.Generic;
using UnityEngine;

namespace SK.Utilities
{
    public class SearchUtility
    {
        private Transform _thisTransform;
        private GameObject _catchedObject;
        private Collider[] _overlapColliders;
        private List<GameObject> _catchedGameObjects;

        public SearchUtility(Transform _transform) 
        {
            _thisTransform = _transform;
            _overlapColliders = new Collider[20]; // Max Amount for Catched Collider Buffer
            _catchedGameObjects = new List<GameObject>();
        }
        
        public GameObject FindTarget(Vector3 positionOffset, float fov, float viewDist, LayerMask objectMask, LayerMask ignoreMask)
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
                    // Call the WithinSight function to determine if this specific object is within sight
                    if ((objectFound = SearchWithinSight(positionOffset, fov, viewDist, _overlapColliders[i].gameObject, out angle, ignoreMask)) != null)
                    {
                        // This object is within sight. Set it to the objectFound GameObject if the angle is less than any of the other objects
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

        public List<GameObject> FindTargets(Vector3 positionOffset, float fov, float viewDist, LayerMask objectMask, LayerMask ignoreMask)
        {
            _catchedGameObjects.Clear(); // Initialize List

            var hitCount = Physics.OverlapSphereNonAlloc(_thisTransform.TransformPoint(positionOffset), viewDist, _overlapColliders, objectMask, QueryTriggerInteraction.Ignore);
            if (hitCount > 0)
            {
                for (int i = 0; i < hitCount; ++i)
                {
                    float angle = 0;
                    // Call the WithinSight function to determine if this specific object is within sight
                    if ((_catchedObject = SearchWithinSight(positionOffset, fov, viewDist, _overlapColliders[i].gameObject, out angle, ignoreMask)) != null)                    
                        _catchedGameObjects.Add(_catchedObject);                    
                }
            }
            return _catchedGameObjects;
        }

        private GameObject SearchWithinSight(Vector3 positionOffset, float fov, float viewDist, GameObject target, out float angle, int ignoreMask)
        {
            if (target == null)
            {
                angle = 0;
                return null;
            }

            // The target object needs to be within the field of view of the current object
            var direction = target.transform.TransformPoint(positionOffset) - _thisTransform.TransformPoint(positionOffset);
            
            angle = Vector3.Angle(direction, _thisTransform.forward);
            direction.y = 0;

            if (direction.magnitude < viewDist && angle < fov * 0.5f)
            {
                // The hit agent needs to be within view of the current agent
                if (LineOfSight(positionOffset, target, ignoreMask) != null)
                {
                    return target; // return the target object meaning it is within sight
                }
            }

            return null;
        }

        private GameObject LineOfSight(Vector3 positionOffset, GameObject targetObject, int ignoreMask)
        {
            RaycastHit hit;
            if (Physics.Linecast(_thisTransform.TransformPoint(positionOffset),
                targetObject.transform.TransformPoint(positionOffset), out hit, ~ignoreMask, QueryTriggerInteraction.Ignore))
            {
                if (hit.transform.IsChildOf(targetObject.transform) || targetObject.transform.IsChildOf(hit.transform))
                    return targetObject; // return the target object meaning it is within sight
            }
            
            return null;
        }
    }
}