using UnityEngine;

namespace SK.State
{
    [RequireComponent(typeof(HitFX))]
    public sealed class Health : MonoBehaviour
    {
        public delegate void onDamagedHandler(uint amount);
        public delegate void onChangedHandler(uint amount);
        public delegate void onDeadHandler();
        public event onDamagedHandler OnDamaged;
        public event onChangedHandler OnChanged;
        public event onDeadHandler OnDead;

        [Header("Player Stat Bonus HP")]
        [SerializeField] private uint bonusHpPerSTR = 5;
        [SerializeField] private uint bonusHpPerDEX = 3;
        [SerializeField] private uint bonusHpPerINT = 3;

        private UI.DamagePointUIManager _damagePointUIManager;

        private Data.UnitBaseData _unitData;
        private Transform _transform;

        private uint _maxHp, _currentHp;
        private bool _canDamage = true;
        private bool _isPlayer;

        // Hp 회복 관련
        private bool _isRecovering;
        private float _elapsed;
        private float _recoverHpAmount; // 초당 Hp 회복량
        private float _leftRecoverAmount; // 잔여 소수점 Hp 회복량

        // 프로퍼티
        public uint MaxHp => _maxHp;
        public uint CurrentHp
        {
            get => _currentHp;
            private set
            {
                _currentHp = value;

                // 최대 HP 보다 많아진 경우 제한
                if (_currentHp > _maxHp)
                    _currentHp = _maxHp;
            }
        }
        public bool CanDamage => _canDamage;

        // 컴포넌트 초기화
        public void Initialize(Data.UnitBaseData data, Transform tr, bool isPlayer = false)
        {
            _unitData = data;
            _transform = tr;

            // 현재 체력 초기화
            uint unitHp = _unitData.Hp;

            // 플레이어인 경우 스탯 보너스 효과 추가
            if (_isPlayer = isPlayer)
            {
                // 최대 체력 설정
                _maxHp = (_unitData.Level * 10)
                    + ((_unitData.Str - 1) * bonusHpPerSTR)
                    + ((_unitData.Dex - 1) * bonusHpPerDEX)
                    + ((_unitData.Int - 1) * bonusHpPerINT);

                if (unitHp > 0) _currentHp = unitHp;
                else _currentHp = _maxHp;
                UI.UIManager.Instance.playerStateUIHandler.UpdateHp(_currentHp);

                // HP 수치가 최대값보다 적은 경우 회복 시작
                if (_currentHp < _maxHp)
                    Recovering();
            }
            // 몬스터인 경우 데이터의 HP를 그대로 사용
            else
            {
                // 최대 체력 설정
                _maxHp = _currentHp = data.Hp;
            }

            // 초당 Hp 회복량
            _recoverHpAmount = _unitData.RecoverHp;
        }

        private void Tick()
        {
            // 현재 수치가 최대값보다 작은 경우
            if (_currentHp < _maxHp)
            {
                _elapsed += Time.deltaTime;

                // 초당 Hp 회복
                if (_elapsed >= 1)
                {
                    RecoverHp(_recoverHpAmount);
                    _elapsed = 0;
                }
            }
            else // 최대 값에 다다랐을 경우 업데이트 해제
                Recovering(false);
        }

        // 최대 체력 설정
        public void SetMaxHp(Data.UnitBaseData data)
        {
            _maxHp = (data.Level * 10) + ((data.Str - 1) * bonusHpPerSTR)
                   + ((data.Dex - 1) * bonusHpPerDEX) + ((data.Int - 1) * bonusHpPerINT);
            UI.UIManager.Instance.playerStateUIHandler.SetMaxHp(_maxHp);
            _currentHp = _maxHp;
            OnChanged?.Invoke(_currentHp);
        }

        // 타격 가능 상태 변경
        public void SetDamagableState(bool isOn) => _canDamage = isOn;

        // 타격 입을 지에 대한 판정에 따라 호출됨
        public void OnDamage(uint damage, bool isCritical)
        {
            if (_currentHp == 0 || !_canDamage) return;

            if (_currentHp <= damage) _currentHp = 0;
            else _currentHp -= damage;
            Debug.Log($"{name}가 {damage}의 데미지를 받아 HP가 {CurrentHp} 가 되었습니다.");

            // 초기화
            if (_damagePointUIManager == null)
                _damagePointUIManager = UI.UIManager.Instance.damagePointUIManager;

            // 데미지 수치 UI 표시
            _damagePointUIManager.DisplayPoint(_transform.position, damage, isCritical);

            // 피격 당한 경우
            if (_currentHp > 0)
            {
                // 플레이어인 경우 피격 즉시 HP 회복 시작
                if (_isPlayer)
                {
                    Recovering();
                }

                OnDamaged?.Invoke(_currentHp);
            }
            // HP가 0이 된 경우
            else
            {
                // HP 회복 중단
                Recovering(false);

                OnDead?.Invoke();
            }
        }

        // 회복 시작 또는 중단
        public void Recovering(bool isRecovering = true)
        {
            if (_isRecovering != isRecovering)
            {
                _isRecovering = isRecovering;
                if (_isRecovering) SceneManager.Instance.OnUpdate += Tick;
                else SceneManager.Instance.OnUpdate -= Tick;
            }
        }

        // 체력 회복
        public void RecoverHp(float amount)
        {
            // 정수부, 소수부 분리
            uint integerAmount = (uint)amount;
            float leftAmount = amount - integerAmount;

            // 잔여 회복량에 소수부 추가
            _leftRecoverAmount += leftAmount;

            // 잔여 회복량이 1보다 크면 정수부 분리
            if (_leftRecoverAmount > 1)
            {
                uint integerLeftAmount = (uint)_leftRecoverAmount;
                // 잔여량에서 정수부 차감
                _leftRecoverAmount -= integerLeftAmount;
                integerAmount += integerLeftAmount;
            }
            CurrentHp += integerAmount;
            OnChanged?.Invoke(_currentHp);
        }

        // 체력 회복
        public void RecoverHp(uint amount)
        {
            CurrentHp += amount;
            OnChanged?.Invoke(_currentHp);
        }
    }
}