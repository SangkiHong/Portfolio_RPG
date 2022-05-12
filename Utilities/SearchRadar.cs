using System;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace SK.Utilities
{
    public class SearchRadar : MonoBehaviour
    {
        [SerializeField]
        private bool debugRader;
        
        [Header("Seek & Wonder")]
        [SerializeField] internal float SeekDistance = 10f;
        public float SeekIdleDuration = 5f;

        [Header("Find Target")]
        public LayerMask objectLayerMask;
        [SerializeField] private float fieldOfViewAngle = 150;
        [SerializeField] internal float viewDistance = 15;
        [SerializeField] private Vector3 offset;

        private SearchUtility searchUtility;
        [SerializeField] GameObject findtarget;
        private Transform _thisTransform;
        private NavMeshHit _navHit;
        private Vector3 _originPos;
        
        private void Awake()
        {
            _thisTransform = transform;
            searchUtility = new SearchUtility(_thisTransform);
        }

        private void Start() => _originPos = _thisTransform.position;

        // Find Target
        public GameObject FindTarget()
        {
            findtarget = searchUtility.FindTarget(offset, fieldOfViewAngle, viewDistance, objectLayerMask);
            return findtarget;
        }

        // SEEK AND WONDER
        public Vector3 SeekAndWonder(float distance)
        {
            Vector3 randomDirection = Random.insideUnitSphere * distance;

            randomDirection += _originPos;

            NavMesh.SamplePosition(randomDirection, out _navHit, distance, NavMesh.AllAreas);

            return _navHit.position;
        }

        public void ResetOriginPos() => _originPos = _thisTransform.position;

        #region Debug Sight
        private void DrawLineOfSight(Vector3 positionOffset, float fieldOfViewAngle, float viewDistance)
        {
#if UNITY_EDITOR
            var oldColor = UnityEditor.Handles.color;
            var color = Color.yellow;
            color.a = 0.1f;
            UnityEditor.Handles.color = color;

            var halfFOV = fieldOfViewAngle * 0.5f;
            var beginDirection = Quaternion.AngleAxis(-halfFOV, transform.up) * transform.forward;
            UnityEditor.Handles.DrawSolidArc(transform.TransformPoint(positionOffset), transform.up, beginDirection, fieldOfViewAngle, viewDistance);

            UnityEditor.Handles.color = oldColor;
#endif
        }

        // Draw the line of sight
        private void OnDrawGizmosSelected()
        {
            if (debugRader) DrawLineOfSight(offset, fieldOfViewAngle, viewDistance);
        }
        #endregion
    }
}