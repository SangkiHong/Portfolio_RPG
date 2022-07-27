using UnityEngine;
using System.Collections.Generic;
using SK.Utilities;

namespace SK.Behavior
{
    [RequireComponent(typeof(EquipmentHolderManager))]
    public class Combat : MonoBehaviour
    {
        public delegate void AttackSuccessHandler(bool isStrongAttack, bool isCriticalAttack);
        public delegate void RushAttackHandler(AnimationEvent animEvent);

        public event AttackSuccessHandler OnAttackSuccess;
        public event RushAttackHandler OnRushAttack;

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
        [SerializeField] private EquipmentHolderManager equipmentManager;
        [SerializeField] private Animator anim;
        private Combo _combo;
        private SearchUtility _searchUtility; // 범위 내 오브젝트를 탐색하는 기능의 컴포넌트

        private Transform _transform; // 트랜트폼 캐싱
        private SortedSet<int> _targetBuff; // 공격 범위 내 오브젝트의 인스턴스ID를 저장할 솔트셋
        private FSM.Unit _thisUnit; // 현재 유닛
        private FSM.Unit _targetUnit; // 타겟 유닛
        private Data.UnitBaseData _unitData; // 현재 유닛 데이터
        private Weapon _currentWeapon; // 현재 무기 정보

        private float _rushAttackInterval; // 돌진 공격 간격
        private bool _isAttackSuccess, // 공격 성공 여부
                     _isStrongAttack;  // 강공격 여부
        private bool _isEnemy;

        public GameObject Target { get; private set; }
        public Attack CurrentAttack { get; private set; } // 현재 공격 정보
        public bool CanComboAttack { get; private set; } = true; // 연속 공격 가능 상태
        public bool IsCriticalHit { get; private set; }

        private void Awake()
        {
            // 초기화
            _transform = transform;
            _targetBuff = new SortedSet<int>();
            if (!anim) anim = GetComponent<Animator>();
            if (!equipmentManager) equipmentManager = GetComponentInChildren<EquipmentHolderManager>();
            if (!_combo) _combo = GetComponent<Combo>();

            _searchUtility = GetComponent<SearchRadar>()?.searchUtility;
            if (_searchUtility == null) _searchUtility = new SearchUtility(_transform);
        }

        // 유닛 정보를 저장하는 함수
        public void Initialize(FSM.Unit unit, Data.UnitBaseData unitData, bool isEnemy = false)
        {
            _thisUnit = unit;
            _unitData = unitData;

            _isEnemy = isEnemy;
            Target = null;
            CanComboAttack = true;
        }

        // 타겟 지정하는 함수
        public void SetTarget(GameObject target) => Target = target;

        // 콤보 가능 상태 변경
        public void SetComboState(bool isOn) => CanComboAttack = isOn;

        #region Attack
        // 공격 버튼(좌우 클릭)에 의한 공격 실행하며 애니메이션 작동
        public void BeginAttack(bool isLeftAttack)
        {
            // 콤보 정보를 통해 전달 받은 공격 정보를 현재 공격 변수에 저장
            CurrentAttack = _combo.GetCombo(isLeftAttack);
            
            _currentWeapon = equipmentManager.GetUseWeapon(isLeftAttack);

            // 공격 중단 불가 상태 변경
            if (CurrentAttack.isUninterruptible)
                _thisUnit.onUninterruptible = true;

            // 공격이 유효하면 공격의 애니메이션 이름을 받아 애니메이션 실행(트랜지션을 0.2로 고정)
            if (CurrentAttack)
                anim.CrossFade(CurrentAttack.animName, 0.2f);
            else // 유효한 공격이 없으면
                Debug.Log("유효한 공격이 없음");
        }

        // 몬스터의 공격 정보를 받아 애니메이션 작동
        public void BeginAttack(Attack attack)
        {
            // 전달 받은 공격 정보를 변수에 저장
            CurrentAttack = attack;

            _currentWeapon = (Weapon)equipmentManager.CurrentUseEquipment;

            // 공격 중단 불가 상태 변경
            if (CurrentAttack.isUninterruptible)
                _thisUnit.onUninterruptible = true;

            // 공격의 애니메이션 이름을 받아 애니메이션 실행하며 트랜지션을 0.2로 고정함
            anim.CrossFade(CurrentAttack.animName, 0.2f);
        }

        // 특수 공격 실행(공격 타입, 타입의 인덱스)
        public void BeginSpecialAttack(AttackType attackType, int index = 0)
        {
            // 콤보 정보를 통해 전달 받은 공격 정보를 현재 공격 변수에 저장
            CurrentAttack = _combo.ExecuteSpecialAttack(attackType, index);

            _currentWeapon = (Weapon)equipmentManager.CurrentUseEquipment;

            // 공격 중단 불가 상태 변경
            if (CurrentAttack.isUninterruptible)
                _thisUnit.onUninterruptible = true;

            // 공격이 유효하면 공격의 애니메이션 이름을 받아 애니메이션 실행(트랜지션을 0.2로 고정)
            if (CurrentAttack)
                anim.CrossFade(CurrentAttack.animName, 0.2f);
            else // 유효한 공격이 없으면
                Debug.Log("유효한 공격이 없음");
        }

        // LineCast & OverlapSphereNonAlloc 사용한 타격 구현(애니메이션 이벤트로 호출, floatParameter: 추가 사거리)
        public void Attack(AnimationEvent animationEvent)
        {
            // 공격 범위 안 타겟 탐색 및 타격(공격 각도, 추가 사거리)
            float addAttackDistance = 0;
            // 돌진 공격이 아닌 경우 추가 Float 파라미터를 추가 사거리로 받음
            if (_rushAttackInterval == 0)
                addAttackDistance = animationEvent.floatParameter;
            CalculateTargetAndDamage(CurrentAttack.attackAngle, addAttackDistance);

            int effectIndex = animationEvent.intParameter;
            if (_isEnemy && effectIndex == 2000) effectIndex = 2002;

            // 이펙트 효과
            if (effectIndex > 0) // 애니메이션 이벤트의 파라미터 값으로 이펙트 ID를 받아서 재생
            {
                // 슬래쉬 이펙트
                if (effectIndex >= 2000 && effectIndex < 3000)
                    EffectManager.Instance.PlayEffect(effectIndex, _transform.forward + _transform.position,
                        equipmentManager.rightHandHook.transform.rotation);
                else
                    EffectManager.Instance.PlayEffect(effectIndex, _transform);
            }

            // 애니메이션 파라미터를 통한 사운드 효과 재생
            if (animationEvent.stringParameter != string.Empty)
                AudioManager.Instance.PlayAudio(animationEvent.stringParameter, _transform);
        }

        // 광역 타격(애니메이션 이벤트로 호출, floatParameter: 추가 사거리)
        public void GlobalAttack(AnimationEvent animationEvent)
        {
            CalculateTargetAndDamage(360, animationEvent.floatParameter);

            // 이펙트 효과
            if (animationEvent.intParameter > 0) // 애니메이션 이벤트의 파라미터 값으로 이펙트 ID를 받아서 재생
                EffectManager.Instance.PlayEffect(animationEvent.intParameter, _transform.position, Quaternion.identity);

            // 애니메이션 파라미터를 통한 사운드 효과 재생
            if (animationEvent.stringParameter != string.Empty)
                AudioManager.Instance.PlayAudio(animationEvent.stringParameter, _transform);
        }

        // 돌진하며 공격 시작(애니메이션 이벤트로 호출, floatParameter: 공격 간격)
        public void RushAttack(AnimationEvent animationEvent)
        {
            // 돌격 공격 이벤트 함수 호출
            OnRushAttack?.Invoke(animationEvent);
            // 애니메이션 이벤트 매개변수로 전달된 값을 전달하여 돌진 공격 발생 간격을 변수에 저장
            _rushAttackInterval = animationEvent.floatParameter;
        }

        // 돌진 공격 마침
        public void RushAttackEnd()
        {
            // 돌격 공격 이벤트 함수 호출
            OnRushAttack?.Invoke(null);
            _rushAttackInterval = 0;
        }
        #endregion

        #region Calculate
        // 데미지 계산(레벨, 힘 스탯, 크리티컬 확률, 크리티컬 배수)
        public uint CalculateDamage(bool normalDamage = false)
        {
            // 현재 착용 중인 무기의 범위 값 중 랜덤으로 가져옴
            int weaponPower = _currentWeapon != null ? 
                            Random.Range(_currentWeapon.AttackMinPower, _currentWeapon.AttackMaxPower + 1) : 0;

            // 현재 공격 액션의 고정 데미지 값을 가져옴
            uint attackActionPower = CurrentAttack != null ? CurrentAttack.attackPower : 1;

            // 값을 합산하여 공격 데미지를 구함(무기 공격력 + 공격 액션 데미지 + (레벨 / 2) + (힘 스탯 / 2) + (최소 데미지 10))
            var damage = weaponPower + attackActionPower + ((_unitData.Level * 0.5f) + (_unitData.Str * 0.5f) + (10));

            // 크리티컬 확률보다 랜덤 값이 더 낮게 나오면 데미지에 크리티컬 배율이 적용됨
            if (!normalDamage && (IsCriticalHit = (Random.value < _unitData.CriticalChance)))
                damage *= _unitData.CriticalMultiplier;
            
            // 최종 계산된 공격 데미지를 변수에 저장
            return (uint)damage;
        }

        // 타겟 확인 및 타격
        private void CalculateTargetAndDamage(int degree, float addDistance = 0)
        {
            // 공격 성공 여부 초기화
            _isAttackSuccess = false;
            // 공격 강도
            _isStrongAttack = CurrentAttack.isStrongAttack;

            _targetBuff.Clear(); // 타겟 리스트 버퍼 초기화

            // 공격 사거리 내 타겟 오브젝트들을 참조된 버퍼 리스트에 저장
             _searchUtility.SearchTargets(offset, degree, attackDistance + addDistance, ref _targetBuff, targetLayerMask);

            if (_targetBuff != null && _targetBuff.Count > 0)
            {
                // 씬 매니저의 GetUnit 함수를 통해 타겟 유닛 정보를 받은 다음 데미지를 전달
                foreach (var target in _targetBuff)
                {
                    _targetUnit = SceneManager.Instance.GetUnit(target);

                    if (_targetUnit != null)
                    {
                        _targetUnit.OnDamage(_thisUnit, CalculateDamage(), _isStrongAttack);
                        _isAttackSuccess = true;

                        // 플레이어가 공격한 적이 사망한 경우
                        if (!_isEnemy && _targetUnit.isDead)
                            Data.DataManager.Instance.AddExp(((Enemy)_targetUnit).enemyData.Exp);

                        // 사운드 효과(크리티컬)
                        if (IsCriticalHit)
                        {
                            // 사운드 효과
                            int randomIndex = Random.Range(0, Strings.Audio_FX_Hit_HeavyImpact.Length);
                            AudioManager.Instance.PlayAudio(Strings.Audio_FX_Hit_HeavyImpact[randomIndex], _thisUnit.transform);
                        }
                        // 사운드 효과(강공격)
                        else if (_isStrongAttack)
                        {
                            // 사운드 효과
                            int randomIndex = Random.Range(0, Strings.Audio_FX_Hit_MiddleImpact.Length);
                            AudioManager.Instance.PlayAudio(Strings.Audio_FX_Hit_MiddleImpact[randomIndex], _thisUnit.transform);
                        }
                        // 사운드 효과(일반)
                        else
                        {
                            // 사운드 효과
                            int randomIndex = Random.Range(0, Strings.Audio_FX_Hit_NormalImpact.Length);
                            AudioManager.Instance.PlayAudio(Strings.Audio_FX_Hit_NormalImpact[randomIndex], _thisUnit.transform);
                        }
                    }

                    // 플레이어가 마지막에 공격한 적의 HP 표시
                    if (!_isEnemy)
                        UI.UIManager.Instance.enemyHpStateUIHandler.ShowEnemyHp((Enemy)_targetUnit);
                }
            }

            // 공격 성공 시 이벤트 호출
            if (_isAttackSuccess) OnAttackSuccess?.Invoke(_isStrongAttack, IsCriticalHit);
        }
        #endregion

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