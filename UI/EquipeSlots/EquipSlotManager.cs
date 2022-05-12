using UnityEngine;

namespace SK.UI
{
    public class EquipSlotManager : MonoBehaviour
    {
        [SerializeField] private UIManager uiManager;
        [SerializeField] private Transform equipSlotParent;

        private EquipSlot[] _equipSlots;
        private Weapon _usingWeapon;
        private Vector2 attackRange;

        private void Awake()
        {
            // 장비 슬롯 초기화
            _equipSlots = equipSlotParent.GetComponentsInChildren<EquipSlot>();
        }

        // 장비 아이템 착용_220512
        public void EquipItem(Item item)
        {
            for (int i = 0; i < _equipSlots.Length; i++)
            {
                // 아이템 장비 타입과 동일한 슬롯 탐색
                if (_equipSlots[i].slotEquipmentType.Equals(item.equipmentType))
                {
                    // 이미 다른 장비가 할당되어 있는 경우 할당된 장비를 인벤토리 슬롯에서 착용 해제
                    if (_equipSlots[i].IsAssigned)
                    {
                        // 슬롯 아이템 데이터 착용 여부 업데이트
                        GameManager.Instance.DataManager.UpdateItemData(_equipSlots[i].assignedItem, false);

                        uiManager.inventoryManager.FindSlotByItem(_equipSlots[i].assignedItem, true).EquipItem(false); 
                    }

                    // 해당 슬롯에 장비 아이템 할당
                    _equipSlots[i].AssignEquipment(item);
                }
            }
        }

        // 장비 아이템 착용 해제_220512
        public void UnequipItem(Item item)
        {
            for (int i = 0; i < _equipSlots.Length; i++)
            {
                // 아이템 장비 타입과 동일한 슬롯 탐색
                if (_equipSlots[i].IsAssigned && _equipSlots[i].slotEquipmentType.Equals(item.equipmentType))
                {

                    // 슬롯에 할당된 장비를 착용 해제
                    _equipSlots[i].Unassign();
                }
            }
        }

        // 현재 착용된 아이템들의 공격력 범위(최소 값~ 최대 값)을 계산하여 Vector2형으로 전달_220510
        public Vector2 CalDamageRange()
        {
            // 현재 착용한 무기의 공격력 + 힘 스탯을 통해 합산한 공격력 표시
            var baseDamage = (GameManager.Instance.DataManager.PlayerData.Level * 0.5f) + (GameManager.Instance.DataManager.PlayerData.Str * 0.5f) + (GameManager.Instance.DataManager.PlayerData.Level + 9);
            uint maxDamage = 0, minDamage = 0;

            // 착용한 주 무기 데이터 정보 가져오기
            for (int i = 0; i < _equipSlots.Length; i++)
            {
                // 착용된 무기 탐색
                if (_equipSlots[i].IsAssigned && _equipSlots[i].assignedItem.equipmentType.Equals(EquipmentType.Weapon))
                {
                    _usingWeapon = _equipSlots[i].assignedItem.equipmentData as Weapon;

                    minDamage += _usingWeapon.AttackMinPower + (uint)baseDamage;
                    maxDamage += _usingWeapon.AttackMaxPower + (uint)baseDamage;
                }
            }
            attackRange.x = minDamage;
            attackRange.y = maxDamage;

            return attackRange;
        }
    }
}
