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

        // ȸ�� ����
        private bool _isUsing, _isRecovering;
        private float _recoverSpAmount;
        private float _leftRecoverAmount; // �ܿ� �Ҽ��� Sp ȸ����
        private float _elapsed;

        public uint MaxSp => _maxSp;
        public uint CurrentSp
        {
            get => _currentSp;
            set
            {
                _currentSp = value;

                // �ִ� SP ���� ������ ��� ����
                if (_currentSp > _maxSp)
                    _currentSp = _maxSp;
            }
        }

        public void Initialize(Data.PlayerData unitData)
        {
            // ���� SP �� ������ ����
            _currentSp = unitData.Sp;

            // DEX ���ȿ� ���� ���ʽ� ȿ�� �����Ͽ� �ִ� ���׹̳� ����
            SetMaxSp(unitData.Level, unitData.Dex);

            // ���� SP ��ġ�� �ִ� SP���� ���ų� 0���� ���� ���
            if (_currentSp > _maxSp || _currentSp <= 0) _currentSp = _maxSp;

            UI.UIManager.Instance.playerStateUIHandler.UpdateSp(_currentSp);

            // �ʴ� Sp ȸ����
            _recoverSpAmount = unitData.RecoverSp;

            // SP ��ġ�� �ִ밪���� ���� ��� ȸ�� ����
            if (_currentSp < _maxSp)
                SceneManager.Instance.OnUpdate += Tick;

            OnChanged?.Invoke(_currentSp);
        }

        private void Tick()
        {
            // ���� ��ġ�� �ִ밪���� ���� ���
            if (_currentSp < _maxSp)
            {
                _elapsed += Time.deltaTime;

                // ��� �� �ð� ��� üũ
                if (_isUsing)
                {
                    // ȸ�� ���� �ð��� �����
                    if (_elapsed >= recoverIntervalTime)
                    {
                        _isUsing = false;
                        _elapsed = 0;
                    }
                }
                // SP ȸ�� ����
                else
                {
                    // �ʴ� SP ȸ��
                    if (_elapsed >= 1)
                    {
                        RecoverSp(_recoverSpAmount);
                        _elapsed = 0;
                    }
                }
            }
            else // �ִ� ���� �ٴٶ��� ��� ������Ʈ ����
                StopRecovering();
        }

        // �ִ� ���׹̳� ����
        public void SetMaxSp(uint level, uint dex)
        {
            _maxSp = defaultValue + (level * levelBonusValue) + dex * dexBonusValue;
            UI.UIManager.Instance.playerStateUIHandler.SetMaxSp(_maxSp);
            _currentSp = _maxSp;
            OnChanged?.Invoke(_currentSp);
        }

        // ���׹̳� ȸ��
        public void RecoverSp(float amount = 0)
        {
            // 0�� ���޵� ��� �ִ� ���� 5% ȸ��
            if (amount == 0)
                CurrentSp += attackRecoverAmount;
            else
            {
                // ������, �Ҽ��� �и�
                uint integerAmount = (uint)amount;
                float leftAmount = amount - integerAmount;

                // �ܿ� ȸ������ �Ҽ��� �߰�
                _leftRecoverAmount += leftAmount;

                // �ܿ� ȸ������ 1���� ũ�� ������ �и�
                if (_leftRecoverAmount > 1)
                {
                    uint integerLeftAmount = (uint)_leftRecoverAmount;
                    // �ܿ������� ������ ����
                    _leftRecoverAmount -= integerLeftAmount;
                    integerAmount += integerLeftAmount;
                }
                CurrentSp += integerAmount;
            }

            // �̺�Ʈ ȣ��
            OnChanged?.Invoke(_currentSp);
        }

        // ���׹̳� ���
        public bool UseSp(uint amount)
        {
            if (_currentSp >= amount)
            {
                CurrentSp -= amount;

                // �̺�Ʈ ȣ��
                OnChanged?.Invoke(_currentSp);

                // SP ��� ����
                if (!_isUsing) _isUsing = true;

                // SP ȸ�� ����
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

        // ȸ�� �ߴ�
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
