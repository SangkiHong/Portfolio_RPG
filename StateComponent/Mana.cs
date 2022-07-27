using UnityEngine;

namespace SK.State
{
    public class Mana : MonoBehaviour
    {
        public delegate void onChangedHandler(uint amount);
        public event onChangedHandler OnChanged;

        [SerializeField] private uint defaultValue = 10;
        [SerializeField] private uint levelBonusValue = 4;
        [SerializeField] private uint intBonusValue = 5;

        private uint _maxMp;
        private uint _currentMp;

        // Mp 회복 관련
        private bool _isRecovering;
        private float _elapsed;
        private float _recoverMpAmount; // 초당 Mp 회복량
        private float _leftRecoverAmount; // 잔여 소수점 Mp 회복량

        public uint MaxMp => _maxMp;
        public uint CurrentMp
        {
            get => _currentMp;
            set
            {
                _currentMp = value;

                // 최대 MP 보다 많아진 경우 제한
                if (_currentMp > _maxMp)
                    _currentMp = _maxMp;
            }
        }

        public void Initialize(Data.UnitBaseData unitData, bool applyStatBonus= false)
        {
            _currentMp = unitData.Mp;

            // 플레이어인 경우 INT 스탯에 따른 보너스 효과 적용
            if (applyStatBonus)
            {
                // 최대 마력 할당
                SetMaxMp(unitData.Level, unitData.Int);

                // 현재 MP 수치가 최대 MP보다 많거나 0보다 적은 경우
                if (_currentMp > _maxMp || _currentMp <= 0) _currentMp = _maxMp;
                UI.UIManager.Instance.playerStateUIHandler.UpdateMp(_currentMp);

                // 초당 Mp 회복량
                _recoverMpAmount = unitData.RecoverMp;

                // MP 수치가 최대값보다 적은 경우 회복 시작
                if (_currentMp < _maxMp)
                {
                    _isRecovering = true;
                    SceneManager.Instance.OnUpdate += Tick;
                }
            }
            // 몬스터인 경우 데이터 그대로 적용
            else
            {
                // 최대 마력 설정
                _maxMp = _currentMp;
            }
            OnChanged?.Invoke(_currentMp);
        }

        private void Tick()
        {
            // 현재 수치가 최대값보다 작은 경우
            if (_currentMp < _maxMp)
            {
                _elapsed += Time.deltaTime;

                // 초당 Mp 회복
                if (_elapsed >= 1)
                {
                    RecoverMp(_recoverMpAmount);
                    _elapsed = 0;
                }
            }
            else // 최대 값에 다다랐을 경우 업데이트 해제
                StopRecovering();
        }

        // 최대 마력 설정
        public void SetMaxMp(uint level, uint Intel)
        {
            _maxMp = defaultValue + (level * levelBonusValue) + Intel * intBonusValue;
            UI.UIManager.Instance.playerStateUIHandler.SetMaxMp(_maxMp);
            _currentMp = _maxMp;
            OnChanged?.Invoke(_currentMp);
        }

        // 마력 회복
        public void RecoverMp(float amount)
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
            CurrentMp += integerAmount;
            OnChanged?.Invoke(_currentMp);
        }

        // 마력 사용
        public bool UseMp(uint amount)
        {
            if (_currentMp >= amount)
            {
                CurrentMp -= amount;
                
                // MP 회복
                if (!_isRecovering)
                {
                    _isRecovering = true;
                    SceneManager.Instance.OnUpdate += Tick;
                }

                OnChanged?.Invoke(_currentMp);
                return true;
            }

            // TODO: MP 부족 안내 UI 표시


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