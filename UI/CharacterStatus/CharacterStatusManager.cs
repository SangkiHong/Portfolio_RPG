using UnityEngine;
using UnityEngine.UI;

namespace SK.UI
{
    public class CharacterStatusManager : MonoBehaviour
    {
        [System.Serializable]
        private struct StatsText
        {
            public Text statsName;
            public Text statsValue;
        }

        [SerializeField] private UIManager uiManager;

        [SerializeField] private Text text_CharacterName;

        [SerializeField] private StatsText text_Level;
        [SerializeField] private StatsText text_Exp;

        [Space]

        [SerializeField] private StatsText text_MaxHp;
        [SerializeField] private StatsText text_MaxMp;
        [SerializeField] private StatsText text_MaxSp;

        [Space]

        [SerializeField] private StatsText text_Str;
        [SerializeField] private StatsText text_Dex;
        [SerializeField] private StatsText text_Int;

        [Space]

        [SerializeField] private StatsText text_Damage;
        [SerializeField] private StatsText text_AttackSpeed;
        [SerializeField] private StatsText text_CriticalChance;

        [Space]

        [SerializeField] private StatsText text_Armor;
        [SerializeField] private StatsText text_Speed;
        [SerializeField] private StatsText text_Avoidance;

        [Space]

        [SerializeField] private StatsText text_RecoverHP;
        [SerializeField] private StatsText text_RecoverMP;
        [SerializeField] private StatsText text_RecoverSP;

        private Data.PlayerData _playerData;
        private Vector2 _attackRange;

        public void Initialize()
        {
            _playerData = GameManager.Instance.DataManager.PlayerData;

            UpdateInformaion();
        }

        // 케릭터 정보 창의 UI 표시_220510
        public void UpdateInformaion()
        {
            // Text 표시
            text_CharacterName.text = _playerData.Name;
            text_Level.statsValue.text = _playerData.Level.ToString();
            text_Exp.statsValue.text = _playerData.Exp.ToString();

            text_MaxHp.statsValue.text = _playerData.MaxHp.ToString();
            text_MaxMp.statsValue.text = _playerData.MaxMp.ToString();
            text_MaxSp.statsValue.text = _playerData.MaxSp.ToString();

            text_Str.statsValue.text = _playerData.Str.ToString();
            text_Dex.statsValue.text = _playerData.Dex.ToString();
            text_Int.statsValue.text = _playerData.Int.ToString();

            _attackRange = uiManager.equipSlotManager.CalDamageRange();
            text_Damage.statsValue.text = 
                string.Format("{0:N0}", _attackRange.x) + " - " + string.Format("{0:N0}", _attackRange.y);
            text_AttackSpeed.statsValue.text = string.Format("{0:N1}", _playerData.AttackSpeed);
            text_CriticalChance.statsValue.text = (_playerData.CriticalChance * 100).ToString() + '%';

            text_Armor.statsValue.text = _playerData.Armor.ToString();
            text_Speed.statsValue.text = _playerData.Speed.ToString();
            text_Avoidance.statsValue.text = _playerData.Avoidance.ToString();

            text_RecoverHP.statsValue.text = _playerData.RecoverHp.ToString();
            text_RecoverMP.statsValue.text = _playerData.RecoverMp.ToString();
            text_RecoverSP.statsValue.text = _playerData.RecoverSp.ToString();
        }
    }
}
