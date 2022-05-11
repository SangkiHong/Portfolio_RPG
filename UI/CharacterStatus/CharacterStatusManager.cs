using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SK
{
    public class CharacterStatusManager : MonoBehaviour
    {
        [System.Serializable]
        private struct StatsText
        {
            public Text statsName;
            public Text statsValue;
        }

        [SerializeField] private Text text_CharacterName;
        [SerializeField] private Transform equipSlotParent;
        [SerializeField] private Button[] buttons_CharacterRotating;

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
        private UI.EquipSlot[] _equipSlots;
        private Weapon primaryWeapon, secondaryWeapon;

        private void Awake()
        {
            for (int i = 0; i < buttons_CharacterRotating.Length; i++)
            {
                var tempIndex = i;
                //buttons_CharacterRotating[i].onClick.AddListener();
            }

            // 장비 슬롯 초기화
            _equipSlots = equipSlotParent.GetComponentsInChildren<UI.EquipSlot>();
        }

        private void Start()
        {
            _playerData = GameManager.Instance.DataManager.PlayerData;

            SetInfo();
        }

        // 케릭터 정보 창의 UI 표시_220510
        private void SetInfo()
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

            // 현재 착용한 무기의 공격력 + 힘 스탯을 통해 합산한 공격력 표시
            var baseDamage = (_playerData.Level * 0.5f) + (_playerData.Str * 0.5f) + (_playerData.Level + 9);
            uint maxDamage = 0,  minDamage = 0;

            // 착용한 주 무기 데이터 정보 가져오기
            if (_equipSlots[_equipSlots.Length - 2].IsAssigned)
            { 
                primaryWeapon = _equipSlots[_equipSlots.Length - 2].GetAssignedItem().equipmentData as Weapon;
                minDamage = primaryWeapon.AttackMinPower + (uint)baseDamage;
                maxDamage = primaryWeapon.AttackMaxPower + (uint)baseDamage;
            }
            // 착용한 보조 무기 데이터 정보 가져오기
            if (_equipSlots[_equipSlots.Length - 1].IsAssigned &&
                _equipSlots[_equipSlots.Length - 2].GetAssignedItem().equipmentType == EquipmentType.Weapon)
            { 
                secondaryWeapon = _equipSlots[_equipSlots.Length - 1].GetAssignedItem().equipmentData as Weapon;
                minDamage += secondaryWeapon.AttackMinPower + (uint)baseDamage;
                maxDamage += secondaryWeapon.AttackMaxPower + (uint)baseDamage;
            }

            text_Damage.statsValue.text = minDamage.ToString() + " - " + maxDamage.ToString();
            text_AttackSpeed.statsValue.text = _playerData.AttackSpeed.ToString();
            text_CriticalChance.statsValue.text = _playerData.CriticalChance.ToString();

            text_Armor.statsValue.text = _playerData.Armor.ToString();
            text_Speed.statsValue.text = _playerData.Speed.ToString();
            text_Avoidance.statsValue.text = _playerData.Avoidance.ToString();

            text_RecoverHP.statsValue.text = _playerData.RecoverHp.ToString();
            text_RecoverMP.statsValue.text = _playerData.RecoverMp.ToString();
            text_RecoverSP.statsValue.text = _playerData.RecoverSp.ToString();
        }
    }
}
