using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using SK.Utilities;

namespace SK.Behavior
{
    [RequireComponent(typeof(EquipmentHolderManager))]
    public class Combat : MonoBehaviour
    {
        public UnityAction onAttack;
        
        [Header("Debug")]
        public bool debugCombatRange;
        public bool debugAttackRange;

        [Header("Attack")]
        public float combatDistance = 3.5f;
        public float canComboDuration = 1.5f;
        [SerializeField] private float impactMotionTime = 1;
        [SerializeField] private AnimationCurve impactMotionCurve;

        [Header("Attack Search")]
        [SerializeField] private LayerMask targetLayerMask;
        [SerializeField] private float attackAngle = 120;
        [SerializeField] internal float attackDistance = 15;
        [SerializeField] private Vector3 offset;

        [Header("Equipments")]
        public Weapon primaryEquipment;
        public Equipments secondaryEquipment;
        public EquipmentHolderManager equipmentHolderManager;

        private GameObject targetObject;
        public GameObject TargetObject => targetObject;

        private Transform _transform;
        private Animator _anim;
        private SearchUtility searchUtility;
        [SerializeField]
        private List<GameObject> _targetBuff;

        internal Alert alert;
        internal Weapon currentUseWeapon;
        internal int calculatedDamage;

        internal bool attackExcuted;
        private bool _isCriticalHit, _isImpact;
        private float _elapsed;

        private void Awake()
        {
            _transform = transform;
            _targetBuff = new List<GameObject>();

            _anim = GetComponent<Animator>();
            
            if (!equipmentHolderManager) equipmentHolderManager = GetComponentInChildren<EquipmentHolderManager>();

            equipmentHolderManager?.Init(); // 장비 초기화

            if (primaryEquipment) // 장비 착용(주무기)
                equipmentHolderManager.LoadEquipmentOnHook(primaryEquipment, true);
            
            if (secondaryEquipment) // 장비 착용(보조 장비)
                equipmentHolderManager.LoadEquipmentOnHook(secondaryEquipment, false);

            searchUtility = new SearchUtility(_transform);
        }

        private void FixedUpdate()
        {
            if (_isImpact)
            {
                _elapsed += Time.deltaTime;
                if (_elapsed >= impactMotionTime)
                {
                    _elapsed = impactMotionTime;
                    _isImpact = false;
                }
                _anim.SetFloat(Strings.AnimPara_AnimSpeed, impactMotionCurve.Evaluate(_elapsed / impactMotionTime));
            }
        }

        public void ExecuteAttack(bool comboAttack = true)
        {
            attackExcuted = false;
            primaryEquipment.ExecuteAction(_anim, comboAttack);
            // Secondary Attack 구현 필요...
        }

        public void ExcuteSpecialAttack(AttackType attackType)
        {
            attackExcuted = false;
            primaryEquipment.ExecuteAction(_anim, attackType);
        }

        // Use LineCast & OverlapSphereNonAlloc
        public void Attack()
        {
            if (attackExcuted) return;

            attackExcuted = true;
            // Using Secondary Weapon
            if (_anim.GetBool(Strings.AnimPara_isSecondEquip))
                currentUseWeapon = (Weapon)secondaryEquipment;
            else
                currentUseWeapon = primaryEquipment;

            SearchAndInflictDamage(currentUseWeapon.GetAttackAngle());
        }

        public void GlobalAttack() => SearchAndInflictDamage(360);

        private void ImpactMotion() 
        {
            _elapsed = 0;
            _isImpact = true;
        }

        private void SearchAndInflictDamage(int degree)
        {
            _targetBuff.Clear();

             searchUtility.FindTargets(offset, degree, attackDistance, ref _targetBuff, targetLayerMask);

            if (_targetBuff != null && _targetBuff.Count > 0)
            {
                // 크리티컬 초기화
                _isCriticalHit = false;

                // 치명타 확률, 배율 가져오기
                onAttack?.Invoke();

                for (int i = 0; i < _targetBuff.Count; i++)
                {
                    _targetBuff[i].GetComponent<IDamagable>()?.OnDamage(calculatedDamage, _transform, _isCriticalHit);

                    // 타격 효과
                    ImpactMotion();
                }
            }
        }

        public void SetTarget(GameObject target) => targetObject = target;
        
        public int CalculateDamage(int level, int strength, float criticalChance, float criticalMultiplier)
        {
            // Calculate Damage
            int weaponPower = Random.Range(currentUseWeapon.AttackMinPower, currentUseWeapon.AttackMaxPower + 1);
            var damage = (level * 0.5f) + (strength * 0.5f) + (weaponPower * 0.5f) + (level + 9);

            // Critical Chance
            if (Random.value < criticalChance)
            {
                damage *= criticalMultiplier;
                _isCriticalHit = true;
            }

            return (int)damage;
        }

        #region Debug
        private void DrawAttackRange(Vector3 positionOffset, float fieldOfViewAngle, float viewDistance, Color color)
        {
#if UNITY_EDITOR
            var oldColor = UnityEditor.Handles.color;
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
            if (debugCombatRange) DrawAttackRange(offset, 360, combatDistance, Color.magenta);
            if (debugAttackRange) DrawAttackRange(offset, attackAngle, attackDistance, Color.red);
        }
        #endregion
    }
}