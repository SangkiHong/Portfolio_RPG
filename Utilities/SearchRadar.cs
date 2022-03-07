using System;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace SK
{
    public class SearchRadar : MonoBehaviour
    {
        [SerializeField]
        private bool debugRader;
        
        [Header("SEEK AND WONDER")]
        public float SeekDistance = 10f;
        public float SeekIdleDuration = 5f;

        [Header("SEARCH PLAYER FindTarget")]
        public LayerMask objectLayerMask;
        [SerializeField]
        private LayerMask ignoreLayerMask;
        [SerializeField]
        private float fieldOfViewAngle = 150;
        [SerializeField]
        private float viewDistance = 15;
        [SerializeField]
        private Vector3 offset;
        [SerializeField]
        private Vector3 targetOffset;
        [SerializeField]
        private bool usePhysics2D;
        [SerializeField]
        private float angleOffset2D;

        public GameObject TargetObject { get; private set; }
        
        private Transform _thisTransform;

        private NavMeshHit _navHit;
        private Collider[] _overlapColliders;
        
        private void Awake()
        {
            _thisTransform = transform;
            _overlapColliders = new Collider[20];
        }
        
        // Find Target
        public bool FindTarget()
        {
            // Target is in ViewDistance then return True
            if (TargetObject && Vector3.Distance(_thisTransform.position, TargetObject.transform.position) <= viewDistance)
                return true;
            
            TargetObject = null;
            
            // ReSharper disable once AssignmentInConditionalExpression
            if (TargetObject = WithinSight(offset, fieldOfViewAngle, viewDistance, _overlapColliders, objectLayerMask, ignoreLayerMask, targetOffset))
                return true;
            
            return false;
        }

        // SEEK AND WONDER
        public Vector3 SeekAndWonder(float distance)
        {
            Vector3 randomDirection = Random.insideUnitSphere * distance;

            randomDirection += _thisTransform.position;

            NavMesh.SamplePosition(randomDirection, out _navHit, distance, NavMesh.AllAreas);

            return _navHit.position;
        }
        
        #region Utility Func
        private GameObject WithinSight(Vector3 positionOffset, float fov, float viewDist, Collider[] overlapColliders, LayerMask objectMask, LayerMask ignoreMask, Vector3 targetOff)
        {
            GameObject objectFound = null;
            var hitCount = Physics.OverlapSphereNonAlloc(_thisTransform.TransformPoint(positionOffset), viewDist, overlapColliders, objectMask, QueryTriggerInteraction.Ignore);
            if (hitCount > 0) {
                float minAngle = Mathf.Infinity;
                for (int i = 0; i < hitCount; ++i) {
                    float angle;
                    GameObject obj;
                    // Call the WithinSight function to determine if this specific object is within sight
                    if ((obj = WithinSight(positionOffset, fov, viewDist, overlapColliders[i].gameObject, targetOff, false, 0, out angle, ignoreMask)) != null) {
                        // This object is within sight. Set it to the objectFound GameObject if the angle is less than any of the other objects
                        if (angle < minAngle) {
                            minAngle = angle;
                            objectFound = obj;
                        }
                    }
                }
            }
            return objectFound;
        }
        
        private GameObject WithinSight(Vector3 positionOffset, float fov, float viewDist, GameObject target, Vector3 targetOff, bool use2D, float angleOff2D, out float angle, int ignoreMask)
        {
            if (target == null) {
                angle = 0;
                return null;
            }
            
            // The target object needs to be within the field of view of the current object
            var direction = target.transform.TransformPoint(targetOff) - _thisTransform.TransformPoint(positionOffset);
            if (use2D) {
                var eulerAngles = _thisTransform.eulerAngles;
                eulerAngles.z -= angleOff2D;
                angle = Vector3.Angle(direction, Quaternion.Euler(eulerAngles) * Vector3.up);
                direction.z = 0;
            } else {
                angle = Vector3.Angle(direction, _thisTransform.forward);
                direction.y = 0;
            }
            if (direction.magnitude < viewDist && angle < fov * 0.5f) {
                // The hit agent needs to be within view of the current agent
                if (LineOfSight(positionOffset, target, targetOff, use2D, ignoreMask) != null) {
                    return target; // return the target object meaning it is within sight
                }
            }
            // return null if the target object is not within sight
            return null;
        }
        
        private GameObject LineOfSight(Vector3 positionOffset, GameObject targetObject, Vector3 targetOff, bool use2D, int ignoreMask)
        {
            if (use2D) {
                RaycastHit2D hit;
                if (hit = Physics2D.Linecast(_thisTransform.TransformPoint(positionOffset), 
                    targetObject.transform.TransformPoint(targetOff), ~ignoreMask)) 
                {
                    if (hit.transform.IsChildOf(targetObject.transform) || targetObject.transform.IsChildOf(hit.transform)) 
                        return targetObject; // return the target object meaning it is within sight
                }
            } else {
                RaycastHit hit;
                if (Physics.Linecast(_thisTransform.TransformPoint(positionOffset), 
                    targetObject.transform.TransformPoint(targetOff), out hit, ~ignoreMask, QueryTriggerInteraction.Ignore)) 
                {
                    if (hit.transform.IsChildOf(targetObject.transform) || targetObject.transform.IsChildOf(hit.transform))
                        return targetObject; // return the target object meaning it is within sight
                }
            }
            return null;
        }
        
        private void DrawLineOfSight(Vector3 positionOffset, float fieldOfViewAngle, float angleOffset, float viewDistance, bool usePhysics2D)
        {
#if UNITY_EDITOR
            var oldColor = UnityEditor.Handles.color;
            var color = Color.yellow;
            color.a = 0.1f;
            UnityEditor.Handles.color = color;

            var halfFOV = fieldOfViewAngle * 0.5f + angleOffset;
            var beginDirection = Quaternion.AngleAxis(-halfFOV, (usePhysics2D ? transform.forward : transform.up)) * (usePhysics2D ? transform.up : transform.forward);
            UnityEditor.Handles.DrawSolidArc(transform.TransformPoint(positionOffset), (usePhysics2D ? transform.forward : transform.up), beginDirection, fieldOfViewAngle, viewDistance);

            UnityEditor.Handles.color = oldColor;
#endif
        }
        
        // Draw the line of sight
        private void OnDrawGizmosSelected()
        {
            if (debugRader) DrawLineOfSight(offset, fieldOfViewAngle, angleOffset2D, viewDistance, usePhysics2D);
        }
        #endregion
    }
}