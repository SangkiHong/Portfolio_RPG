using System.Text;
using UnityEngine;
using UnityEngine.UI;
using SK.State;

namespace SK.UI
{
    public class PlayerStateUIHandler : MonoBehaviour
    {
        [Header("State")]
        [SerializeField] private Slider hp_Slider;
        [SerializeField] private Text maxHp_Text;
        [SerializeField] private Slider mp_Slider;
        [SerializeField] private Text maxMp_Text;
        [SerializeField] private Slider sp_Slider;
        
        [Space]
        [SerializeField] private Text text_AttackPower;
        [SerializeField] private Text text_DefensePower;

        [Header("Level")]
        [SerializeField] private Text text_Level;
        [SerializeField] private Text text_Exp;

        private Health _playerHealth;
        private Mana _playerMana;
        private Stamina _playerStamina;

        private Data.PlayerData _playerData;
        private StringBuilder _stringBuilder = new StringBuilder();
        private readonly char _char_percent = '%';

        public void Initialize(Data.PlayerData playerData)
        {
            _playerData = playerData;
            _playerHealth = GameManager.Instance.Player.health;
            _playerMana = GameManager.Instance.Player.mana;
            _playerStamina = GameManager.Instance.Player.stamina;
            
            // 이벤트 함수 등록
            _playerHealth.OnChanged += UpdateHp;
            _playerHealth.OnDamaged += UpdateHp;
            _playerHealth.OnDead += OnDead;
            _playerMana.OnChanged += UpdateMp;
            _playerStamina.OnChanged += UpdateSp;

            // 상태 초기화
            // HP 초기화
            hp_Slider.maxValue = _playerHealth.MaxHp;
            hp_Slider.value = hp_Slider.maxValue;
            maxHp_Text.text = hp_Slider.maxValue.ToString();

            // 공격력 표시
            text_AttackPower.text = GameManager.Instance.Player.combat.CalculateDamage(true).ToString();

            // 방어력 표시
            text_DefensePower.text = playerData.Def.ToString();

            // 레벨 표시
            text_Level.text = playerData.Level.ToString();

            // 경험치 표시
            UpdateExp();
        }

        public void UpdateHp(uint value)
            => hp_Slider.value = value;

        public void SetMaxHp(uint value)
        {
            hp_Slider.maxValue = value;
            hp_Slider.value = hp_Slider.maxValue;
            maxHp_Text.text = hp_Slider.maxValue.ToString();
        }

        public void UpdateMp(uint value)
            => mp_Slider.value = value;

        public void SetMaxMp(uint value)
        {
            mp_Slider.maxValue = value;
            mp_Slider.value = mp_Slider.maxValue;
            maxMp_Text.text = mp_Slider.maxValue.ToString();
        }

        public void UpdateSp(uint value)
            => sp_Slider.value = value;

        public void SetMaxSp(uint value)
        {
            sp_Slider.maxValue = value;
            sp_Slider.value = sp_Slider.maxValue;
        }

        public void UpdateLevel(uint value)
            => text_Level.text = value.ToString();

        public void UpdateExp()
        {
            float progress = (float)_playerData.Exp / PlayerLevelManager.Instance.LevelUpExp;
            _stringBuilder.Clear();
            _stringBuilder.Append(string.Format("{0:0.00}", progress * 100));
            _stringBuilder.Append(_char_percent);
            text_Exp.text = _stringBuilder.ToString();
        }

        private void OnDead()
        {
            hp_Slider.value = 0;
            mp_Slider.value = 0;
            sp_Slider.value = 0;
        }
    }
}
