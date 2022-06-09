using UnityEngine;
using System.Collections.Generic;
using SK.Utilities;

namespace SK.Behavior
{
    [RequireComponent(typeof(EquipmentHolderManager))]
    public class Combat : MonoBehaviour
    {        
        [Header("Debug")]
        public bool debugCombatRange;
        public bool debugAttackRange;

        [Header("Attack")]
        public float combatDistance = 3.5f;
        public float canComboDuration = 1.5f;

        [Header("Attack Search")]
        [SerializeField] private LayerMask targetLayerMask;
        [SerializeField] private float attackAngle = 120;
        [SerializeField] internal float attackDistance = 2;
        [SerializeField] private Vector3 offset;

        [Header("Reference")]
        public EquipmentHolderManager equipmentManager;
        [SerializeField] private Animator _anim;
        private Combo _combo;
        private SearchUtility _searchUtility; // 범위 내 오브젝트를 탐색하는 기능의 컴포넌트

        public GameObject Target { get; private set; }

        internal Attack currentAttack; // 현재 공격 정보
        internal bool attackExcuted; // 공격 판정 실행 여부
        internal bool canComboAttack = true; // 연속 공격 가능 상태
        internal bool isCriticalHit;

        private Transform _transform; // 트랜트폼 캐싱
        private SortedSet<int> _targetBuff; // 공격 범위 내 오브젝트의 인스턴스ID를 저장할 솔트셋
        private FSM.Unit _thisUnit; // 현재 유닛
        private Data.UnitBaseData _unitData; // 현재 유닛 데이터
        private Weapon _currentWeapon; // 현재 무기 정보

        private void Awake()
        {
            // 초기화
            _transform = transform;
            _targetBuff = new SortedSet<int>();
            if (!_anim) _anim = GetComponent<Animator>();
            if (!equipmentManager) equipmentManager = GetComponentInChildren<EquipmentHolderManager>();
            if (!_combo) _combo = GetComponent<Combo>();

            _searchUtility = GetComponent<SearchRadar>()?.searchUtility;
            if (_searchUtility == null) _searchUtility = new SearchUtility(_transform);
        }

        // 유닛 정보를 저장하는 함수
        public void SetUnitInfo(FSM.Unit unit, Data.UnitBaseData unitData)
        {
            _thisUnit = unit;
            _unitData = unitData;
        }

        // 타겟 지정하는 함수
        public void SetTarget(GameObject target) => Target = target;

        // 공격 버튼(좌우 클릭)에 의한 공격 실행하며 애니메이션 작동
        public void BeginAttack(bool isLeftAttack)
        {
            attackExcuted = false;

            // 콤보 정보를 통해 전달 받은 공격 정보를 현재 공격 변수에 저장
            currentAttack = _combo.GetCombo(isLeftAttack);
            
            _currentWeapon = equipmentManager.GetUseWeapon(isLeftAttack);

            // 공격이 유효하면 공격의 애니메이션 이름을 받아 애니메이션 실행(트랜지션을 0.2로 고정)
            if (currentAttack)
                _anim.CrossFade(currentAttack.animName, 0.2f);
            else // 유효한 공격이 없으면
                Debug.Log("유효한 공격이 없음");
        }

        // 몬스터의 공격 정보를 받아 애니메이션 작동
        public void BeginAttack(Attack attack)
        {
            attackExcuted = false;

            // 전달 받은 공격 정보를 변수에 저장
            currentAttack = attack;

            _currentWeapon = (Weapon)equipmentManager.currentUseEquipment;

            // 공격의 애니메이션 이름을 받아 애니메이션 실행하며 트랜지션을 0.2로 고정함
            _anim.CrossFade(currentAttack.animName, 0.2f);
        }

        // 특수 공격 실행(공격 타입, 타입의 인덱스)
        public void BeginSpecialAttack(AttackType attackType, int index = 0)
        {
            attackExcuted = false;

            // 콤보 정보를 통해 전달 받은 공격 정보를 현재 공격 변수에 저장
            currentAttack = _combo.ExecuteSpecialAttack(attackType, index);

            // 공격이 유효하면 공격의 애니메이션 이름을 받아 애니메이션 실행(트랜지션을 0.2로 고정)
            if (currentAttack)
                _anim.CrossFade(currentAttack.animName, 0.2f);
            else // 유효한 공격이 없으면
                Debug.Log("유효한 공격이 없음");
        }

        // LineCast & OverlapSphereNonAlloc 사용한 타격 구현(추가 사거리)
        // 애니메이션 이벤트로 호출
        public void Attack(float addDist)
        {
            if (attackExcuted) return;

            attackExcuted = true;

            // 공격 범위 안 타겟 탐색 및 타격(공격 각도, 추가 사거리)
            SearchAndInflictDamage(currentAttack.attackAngle, addDist);
        }

        // 주위 전체 범위 공격(추가 사거리)
        // 애니메이션 이벤트로 호출
        public void GlobalAttack(float addDist) => SearchAndInflictDamage(360, addDist);

        // 데미지 계산(레벨, 힘 스탯, 크리티컬 확률, 크리티컬 배수)
        public uint CalculateDamage()
        {
            // 현재 착용 중인 무기의 범위 값 중 랜덤으로 가져옴
            int weaponPower = Random.Range(_currentWeapon.AttackMinPower, _currentWeapon.AttackMaxPower + 1);

            // 현재 공격 액션의 고정 데미지 값을 가져옴
            uint attackActionPower = currentAttack != null ? currentAttack.attackPower : 1;

            // 값을 합산하여 공격 데미지를 구함(무기 공격력 + 공격 액션 데미지 + (레벨 / 2) + (힘 스탯 / 2) + (최소 데미지 10))
            var damage = weaponPower + attackActionPower + ((_unitData.Level * 0.5f) + (_unitData.Str * 0.5f) + (10));

            // 크리티컬 확률보다 랜덤 값이 더 낮게 나오면 데미지에 크리티컬 배율이 적용됨
            if (Random.value < _unitData.CriticalChance)
            {
                damage *= _unitData.CriticalMultiplier;
                isCriticalHit = true;
            }
            else // 크리티컬 초기화
                isCriticalHit = false;

            // 최종 계산된 공격 데미지를 변수에 저장
            return (uint)damage;
        }

        // 타겟 확인 및 타격
        private void SearchAndInflictDamage(int degree, float addDistance = 0)
        {
            _targetBuff.Clear(); // 타겟 리스트 버퍼 초기화

            // 공격 사거리 내 타겟 오브젝트들을 참조된 버퍼 리스트에 저장
             _searchUtility.SearchTargets(offset, degree, attackDistance + addDistance, ref _targetBuff, targetLayerMask);

            if (_targetBuff != null && _targetBuff.Count > 0)
            {
                // 씬 매니저의 GetUnit 함수를 통해 타겟 유닛 정보를 받은 다음 데미지를 전달
                foreach (var target in _targetBuff)
                    SceneManager.Instance.GetUnit(target)
                        .OnDamage(_thisUnit, CalculateDamage(), currentAttack.isStrongAttack);
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