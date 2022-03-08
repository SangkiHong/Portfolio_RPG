using System;
using UnityEngine;

namespace SK.Behavior
{
    public class Combat : MonoBehaviour
    {
        [Header("Debug")]
        public bool debugAttackCollier;
        
        [Header("Attack")]
        public float combatDistance = 3.5f;
        [Space]
        public float attackCooldown = 2.5f;
        [SerializeField] 
        private LayerMask targetLayer;
        public Vector3 attackColScale;
        public Vector3[] attackColsOffset;
        public int attackColSize;
        public float attackAngle;
        
        private Transform _transform;
        private Collider[] _colliders;

        private void Awake()
        {
            _transform = GetComponent<Transform>();
            _colliders = new Collider[5];
        }

        public void Attack(int ColIndex)
        {
            Debug.Assert(ColIndex < attackColSize || ColIndex == 999, "Collider Index Error!");

            int firstIndex = 0;
            int size = attackColSize; // Collider 갯수
            var interval = attackAngle / (attackColSize - 1);
            var initAngle = -attackAngle * 0.5f; 
            
            // Index 값이 고정일 때
            if (ColIndex != 999)
            {
                firstIndex = ColIndex;
                size = ColIndex + 1;
            }
            
            for (int i = firstIndex; i < size; i++)
            {
                var rotation = _transform.rotation;
                if (attackColSize > 1) rotation *= Quaternion.Euler((initAngle + i * interval) * Vector3.up);

                if (Physics.OverlapBoxNonAlloc(_transform.position + _transform.TransformDirection(attackColsOffset[i]),
                    attackColScale * 0.5f, _colliders,
                    rotation, targetLayer, QueryTriggerInteraction.Collide) > 0)
                {
                    Debug.Log(_colliders[0].name);
                }
            }
        }

        void OnDrawGizmosSelected()
        {
            if (debugAttackCollier)
            {
                var interval = attackAngle / (attackColSize - 1);
                var initAngle = -attackAngle * 0.5f;
                for (int i = 0; i < attackColSize; i++)
                {
                    var position = transform.position + transform.TransformDirection(attackColsOffset[i]);
                    var rotation = transform.rotation;
                    if (attackColSize > 1) rotation *= Quaternion.Euler((initAngle + i * interval) * Vector3.up);

                    Matrix4x4 rotationMatrix = Matrix4x4.TRS(position, rotation, attackColScale);
                    Gizmos.matrix = rotationMatrix;
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawWireCube(Vector3.zero * 0.5f, attackColScale);
                }
            }
        }
    }
}