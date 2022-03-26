using UnityEngine;
using UnityEngine.Events;
using System.Linq;
using System.Collections.Generic;
using SK.Utilities;

namespace SK.Behavior
{
    [RequireComponent(typeof(EquipmentHolderManager))]
    public class Combat : MonoBehaviour
    {
        public UnityAction onAttack;
        
        [Header("Debug")]
        public bool debugAttackRange;
        
        [Header("Attack")]
        public bool canComboAttack;
        public float combatDistance = 3.5f;
        public float canComboDuration = 1.5f;

        [Header("Attack Search")]
        [SerializeField] private LayerMask targetLayerMask;
        [SerializeField] private LayerMask ignoreLayerMask;
        [SerializeField] private float attackAngle = 120;
        [SerializeField] private float attackDistance = 15;
        [SerializeField] private Vector3 offset;

        [Space]

        //public Vector3 attackColScale; // deprecated::Use OverlapBoxNonAlloc
        //public Vector3[] attackColsOffset; // deprecated::Use OverlapBoxNonAlloc

        [Header("Equipments")]
        public Equipments primaryEquipment;
        public Equipments secondaryEquipment;
        public EquipmentHolderManager equipmentHolderManager;

        private SearchUtility searchUtility;
        private Transform _transform;
        private Animator _anim;
        private List<GameObject> _targetBuff;
        //private Collider[] _colliderBuff; // deprecated::Use OverlapBoxNonAlloc
        //private List<Collider> _colliderList; // deprecated::Use OverlapBoxNonAlloc

        [System.NonSerialized] public Weapon currentUseWeapon;
        [System.NonSerialized] public int calculatedDamage;
        [System.NonSerialized] public bool isCriticalHit;

        private void Awake()
        {
            _transform = transform;
            _anim = GetComponent<Animator>();
            if (!equipmentHolderManager) equipmentHolderManager = GetComponentInChildren<EquipmentHolderManager>();

            equipmentHolderManager?.Init(); // 장비 초기화

            if (primaryEquipment) // 장비 착용(주무기)
                equipmentHolderManager.LoadEquipmentOnHook(primaryEquipment, true);
            
            if (secondaryEquipment) // 장비 착용(보조 장비)
                equipmentHolderManager.LoadEquipmentOnHook(secondaryEquipment, false);

            //_colliderBuff = new Collider[10]; // deprecated::Use OverlapBoxNonAlloc
            //_colliderList = new List<Collider>(); // deprecated::Use OverlapBoxNonAlloc
            searchUtility = new SearchUtility(_transform);
        }

        public void ExecuteAttack()
        {
            primaryEquipment.ExecuteAction(_anim, !canComboAttack);
            // TODO: Implement Dual Weapon Execution..
            //...
        }

        // Use LineCast & OverlapSphereNonAlloc
        public void Attack()
        {
            // Using Secondary Weapon
            if (_anim.GetBool(Strings.AnimPara_isSecondEquip))
                currentUseWeapon = (Weapon)secondaryEquipment;
            else
                currentUseWeapon = (Weapon)primaryEquipment;

            _targetBuff = searchUtility.FindTargets(offset, currentUseWeapon.GetAttackAngle(), attackDistance, targetLayerMask, ignoreLayerMask);

            if (_targetBuff != null && _targetBuff.Count > 0)
            {
                // 크리티컬 초기화
                isCriticalHit = false;

                // 치명타 확률, 배율 가져오기
                onAttack?.Invoke();

                for (int i = 0; i < _targetBuff.Count; i++)
                {
                    _targetBuff[i].GetComponent<IDamagable>()?.OnDamage(-calculatedDamage, _transform, isCriticalHit);
                }
            }
        }

        // deprecated::Use OverlapBoxNonAlloc
        /*public void Attack(int isLeft) // 0 = Right Weapon, 1 = Left Weapon
        {
            // 초기화
            int size; // Using Collider Amount
            if (isLeft == 0) // Right Weapon            
                size = ((Weapon)primaryEquipment).GetAttackColliderAmount();
            else // Left Weapon
                size = ((Weapon)secondaryEquipment).GetAttackColliderAmount();

            // Clear Collider Array
            for (int i = 0; i < _colliderBuff.Length; i++)
                if (_colliderBuff[i]) _colliderBuff[i] = null;

            // Clear List
            _colliderList.RemoveRange(0, _colliderList.Count);

            float v = 0;
            float interval = size > 1 ? (size - 1) * 45 / (size - 1) : 0;

            for (int i = 0; i < size; i++)
            {
                if (i % 2 == 1)
                {
                    v += 1;
                    v *= -interval;
                }
                else
                    v *= -1;

                var rotation = _transform.rotation;

                rotation *= Quaternion.Euler(interval * v * Vector3.up);

                if (Physics.OverlapBoxNonAlloc(_transform.position + _transform.TransformDirection(attackColsOffset[i]),
                    attackColScale * 0.5f, _colliderBuff,
                    rotation, targetLayer, QueryTriggerInteraction.Collide) > 0)
                {
                    for (int j = 0; j < _colliderBuff.Length; j++)
                    {
                        if (_colliderBuff[j] != null)
                            _colliderList.Add(_colliderBuff[j]);
                    }
                }
            }

            // Catch 된 콜라이더들에게 데미지 전달
            if (_colliderList.Count > 0)
            {
                // 크리티컬 초기화
                isCriticalHit = false;

                // 치명타 확률, 배율 가져오기
                onAttack?.Invoke();

                // 중복 List 제거
                _colliderList = _colliderList.Distinct(new ColliderComparer()).ToList();

                for (int i = 0; i < _colliderList.Count; i++)
                {
                    _colliderList[i].GetComponent<IDamagable>()?.OnDamage(-calculatedDamage, _transform, isCriticalHit);
                }
            }
        }

        // Collier 비교 함수
        private class ColliderComparer : IEqualityComparer<Collider>
        {
            public bool Equals(Collider x, Collider y)
            {
                if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                    return false;

                return x.GetHashCode() == y.GetHashCode();
            }

            public int GetHashCode(Collider obj)
            {
                if (obj == null)
                    return 0;

                return obj.GetHashCode();
            }
        }

        void OnDrawGizmosSelected()
        {
            if (attackColsOffset.Length > 0 && debugAttackCollier)
            {
                float interval = (attackColsOffset.Length - 1) * 45 / (attackColsOffset.Length - 1);
                float v = 0;
                for (int i = 0; i < attackColsOffset.Length; i++)
                {
                    var position = transform.position + transform.TransformDirection(attackColsOffset[i]);
                    var rotation = transform.rotation;

                    if (i % 2 == 1)
                    {
                        v += 1;
                        v *= -interval;
                    }
                    else                    
                        v *= -1;
                    
                    rotation *= Quaternion.Euler(v * Vector3.up);

                    Matrix4x4 rotationMatrix = Matrix4x4.TRS(position, rotation, attackColScale);
                    Gizmos.matrix = rotationMatrix;
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawWireCube(Vector3.zero, attackColScale);
                }
            }
        }*/

        #region Debug Sight
        private void DrawAttackRange(Vector3 positionOffset, float fieldOfViewAngle, float viewDistance)
        {
#if UNITY_EDITOR
            var oldColor = UnityEditor.Handles.color;
            var color = Color.red;
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
            if (debugAttackRange) DrawAttackRange(offset, attackAngle, attackDistance);
        }
        #endregion
    }
}