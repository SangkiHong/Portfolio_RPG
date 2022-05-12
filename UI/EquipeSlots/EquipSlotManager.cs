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
            // ��� ���� �ʱ�ȭ
            _equipSlots = equipSlotParent.GetComponentsInChildren<EquipSlot>();
        }

        // ��� ������ ����_220512
        public void EquipItem(Item item)
        {
            for (int i = 0; i < _equipSlots.Length; i++)
            {
                // ������ ��� Ÿ�԰� ������ ���� Ž��
                if (_equipSlots[i].slotEquipmentType.Equals(item.equipmentType))
                {
                    // �̹� �ٸ� ��� �Ҵ�Ǿ� �ִ� ��� �Ҵ�� ��� �κ��丮 ���Կ��� ���� ����
                    if (_equipSlots[i].IsAssigned)
                    {
                        // ���� ������ ������ ���� ���� ������Ʈ
                        GameManager.Instance.DataManager.UpdateItemData(_equipSlots[i].assignedItem, false);

                        uiManager.inventoryManager.FindSlotByItem(_equipSlots[i].assignedItem, true).EquipItem(false); 
                    }

                    // �ش� ���Կ� ��� ������ �Ҵ�
                    _equipSlots[i].AssignEquipment(item);
                }
            }
        }

        // ��� ������ ���� ����_220512
        public void UnequipItem(Item item)
        {
            for (int i = 0; i < _equipSlots.Length; i++)
            {
                // ������ ��� Ÿ�԰� ������ ���� Ž��
                if (_equipSlots[i].IsAssigned && _equipSlots[i].slotEquipmentType.Equals(item.equipmentType))
                {

                    // ���Կ� �Ҵ�� ��� ���� ����
                    _equipSlots[i].Unassign();
                }
            }
        }

        // ���� ����� �����۵��� ���ݷ� ����(�ּ� ��~ �ִ� ��)�� ����Ͽ� Vector2������ ����_220510
        public Vector2 CalDamageRange()
        {
            // ���� ������ ������ ���ݷ� + �� ������ ���� �ջ��� ���ݷ� ǥ��
            var baseDamage = (GameManager.Instance.DataManager.PlayerData.Level * 0.5f) + (GameManager.Instance.DataManager.PlayerData.Str * 0.5f) + (GameManager.Instance.DataManager.PlayerData.Level + 9);
            uint maxDamage = 0, minDamage = 0;

            // ������ �� ���� ������ ���� ��������
            for (int i = 0; i < _equipSlots.Length; i++)
            {
                // ����� ���� Ž��
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
