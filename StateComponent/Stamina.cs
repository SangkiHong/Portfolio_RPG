using UnityEngine;

namespace SK.State
{
    public class Stamina : MonoBehaviour
    {
        public delegate void onChangedHandler(uint amount);
        public event onChangedHandler OnChanged;

        [Header("Set Max Value")]
        [SerializeField] private uint defaultValue = 30;
        [SerializeField] private uint levelBonusValue = 2;
        [SerializeField] private uint dexBonusValue = 5;

        [Header("Recovering")]
        [SerializeField] private float recoverIntervalTime = 3;
        [SerializeField] private uint attackRecoverAmount = 2;

        private uint _maxSp;
        private uint _currentSp;

        // 회복 관련
        private bool _isUsing, _isRecovering;
        private float _recoverSpAmount;
        private float _leftRecoverAmount; // 잔여 소수점 Sp 회복량
        private float _elapsed;

        public uint MaxSp => _maxSp;
        public uint CurrentSp
        {
            get => _currentSp;
            set
            {
                _currentSp = value;

                // 최대 SP 보다 많아진 경우 제한
                if (_currentSp > _maxSp)
                    _currentSp = _maxSp;
            }
        }

        public void Initialize(Data.PlayerData unitData)
        {
            // 현재 SP 값 변수에 저장
            _currentSp = unitData.Sp;

            // DEX 스탯에 따른 보너스 효과 적용하여 최대 스테미나 설정
            SetMaxSp(unitData.Level, unitData.Dex);

            // 현재 SP 수치가 최대 SP보다 많거나 0보다 적은 경우
            if (_currentSp > _maxSp || _currentSp <= 0) _currentSp = _maxSp;

            UI.UIManager.Instance.playerStateUIHandler.UpdateSp(_currentSp);

            // 초당 Sp 회복량
            _recoverSpAmount = unitData.RecoverSp;

            // SP 수치가 최대값보다 적은 경우 회복 시작
            if (_currentSp < _maxSp)
                SceneManager.Instance.OnUpdate += Tick;

            OnChanged?.Invoke(_currentSp);
        }

        private void Tick()
        {
            // 현재 수치가 최대값보다 작은 경우
            if (_currentSp < _maxSp)
            {
                _elapsed += Time.deltaTime;

                // 사용 후 시간 경과 체크
                if (_isUsing)
                {
                    // 회복 가능 시간이 경과됨
                    if (_elapsed >= recoverIntervalTime)
                    {
                        _isUsing = false;
                        _elapsed = 0;
                    }
                }
                // SP 회복 시작
                else
                {
                    // 초당 SP 회복
                    if (_elapsed >= 1)
                    {
                        RecoverSp(_recoverSpAmount);
                        _elapsed = 0;
                    }
                }
            }
            else // 최대 값에 다다랐을 경우 업데이트 해제
                StopRecovering();
        }

        // 최대 스테미나 설정
        public void SetMaxSp(uint level, uint dex)
        {
            _maxSp = defaultValue + (level * levelBonusValue) + dex * dexBonusValue;
            UI.UIManager.Instance.playerStateUIHandler.SetMaxSp(_maxSp);
            _currentSp = _maxSp;
            OnChanged?.Invoke(_currentSp);
        }

        // 스테미나 회복
        public void RecoverSp(float amount = 0)
        {
            // 0이 전달된 경우 최대 값의 5% 회복
            if (amount == 0)
                CurrentSp += attackRecoverAmount;
            else
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
                CurrentSp += integerAmount;
            }

            // 이벤트 호출
            OnChanged?.Invoke(_currentSp);
        }

        // 스테미나 사용
        public bool UseSp(uint amount)
        {
            if (_currentSp >= amount)
            {
                CurrentSp -= amount;

                // 이벤트 호출
                OnChanged?.Invoke(_currentSp);

                // SP 사용 여부
                if (!_isUsing) _isUsing = true;

                // SP 회복 시작
                if (!_isRecovering)
                {
                    _isRecovering = true;
                    SceneManager.Instance.OnUpdate += Tick;
                }

                _elapsed = 0;
                return true;
            }

            return false;
        }

        // 회복 중단
        public void StopRecovering()
        {
            if (_isRecovering)
            {
                _isRecovering = false;
                SceneManager.Instance.OnUpdate -= Tick;
            }
        }
    }
}
