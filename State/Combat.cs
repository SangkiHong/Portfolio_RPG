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
        internal uint calculatedDamage;

        internal bool attackExcuted;
        private bool _isCriticalHit;

        private void Awake()
        {
            // 초기화
            _transform = transform;
            _targetBuff = new List<GameObject>();
            _anim = GetComponent<Animator>();
            if (!equipmentHolderManager) 
                equipmentHolderManager = GetComponentInChildren<EquipmentHolderManager>();
            
            // 장비 초기화
            equipmentHolderManager?.Init();
            // 장비 착용(주 무기)
            if (primaryEquipment) 
                equipmentHolderManager.LoadEquipmentOnHook(primaryEquipment, true);
            // 장비 착용(보조 장비)
            if (secondaryEquipment) 
                equipmentHolderManager.LoadEquipmentOnHook(secondaryEquipment, false);

            // 현재 무기 디폴드 값으로 주 무기 할당
            currentUseWeapon = primaryEquipment;
            searchUtility = new SearchUtility(_transform);
        }

        // 공격 실행(콤보 공격 여부)
        public void ExecuteAttack(bool comboAttack = true)
        {
            attackExcuted = false;
            // 주 무기 공격 실행
            primaryEquipment.ExecuteAction(_anim, comboAttack);
            // 보조 무기 구현 필요...
        }

        // 특수 공격 실행(공격 타입, 타입의 인덱스)
        public void ExcuteSpecialAttack(AttackType attackType, int index = 0)
        {
            attackExcuted = false;
            // 주 무기 특수 공격 실행
            primaryEquipment.ExecuteSpecialAction(_anim, attackType, index);
        }

        // LineCast & OverlapSphereNonAlloc 사용한 타격 구현(추가 사거리)
        public void Attack(float addDist)
        {
            if (attackExcuted) return;

            attackExcuted = true;

            // 보조 장비 착용 여부 확인
            if (_anim.GetBool(Strings.AnimPara_isSecondEquip))
                currentUseWeapon = (Weapon)secondaryEquipment;
            else
                currentUseWeapon = primaryEquipment;

            // 공격 범위 안 타겟 탐색 및 타격(공격 각도, 추가 사거리)
            SearchAndInflictDamage(currentUseWeapon.currentAttack.attackAngle, addDist);
        }

        // 주변 전체 공격
        public void GlobalAttack(float addDist) => SearchAndInflictDamage(360, addDist);

        // 타겟 지정
        public void SetTarget(GameObject target) => targetObject = target;

        // 데미지 계산(레벨, 힘 스탯, 크리티컬 확률, 크리티컬 배수)
        public void CalculateDamage(uint level, uint strength, float criticalChance, float criticalMultiplier)
        {
            // Calculate Damage
            int weaponPower = Random.Range((int)currentUseWeapon.AttackMinPower, (int)currentUseWeapon.AttackMaxPower + 1);
            var damage = weaponPower + ((level * 0.5f) + (strength * 0.5f) + (level + 9));

            // Critical Chance
            if (Random.value < criticalChance)
            {
                damage *= criticalMultiplier;
                _isCriticalHit = true;
            }

            calculatedDamage = (uint)damage;
        }

        // 타겟 확인 및 타격
        private void SearchAndInflictDamage(int degree, float addDistance = 0)
        {
            _targetBuff.Clear();

             searchUtility.FindTargets(offset, degree, attackDistance + addDistance, ref _targetBuff, targetLayerMask);

            if (_targetBuff != null && _targetBuff.Count > 0)
            {
                // 크리티컬 초기화
                _isCriticalHit = false;

                // 치명타 확률, 배율 가져오기
                onAttack?.Invoke();

                for (int i = 0; i < _targetBuff.Count; i++)
                {
                    // 타겟에게 데미지 전달
                    _targetBuff[i].GetComponent<IDamagable>()?
                        .OnDamage(calculatedDamage, _transform, _isCriticalHit, currentUseWeapon.currentAttack.isStrongAttack);

                    // 타격 효과 시전

                }
            }
        }

        // 디버깅
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