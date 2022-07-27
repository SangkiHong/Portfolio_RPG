using UnityEngine;

namespace SK.UI
{
    public class EquipSlotManager : MonoBehaviour
    {
        [SerializeField] private UIManager uiManager;
        [SerializeField] private Transform equipSlotParent;

        private EquipSlot[] _equipSlots;
        private Data.PlayerItemData _playerItemData;
        private Item _tempItem;
        private Weapon _usingWeapon;
        private Vector2 attackRange;

        private void Awake()
        {
            // ��� ���� �ʱ�ȭ
            _equipSlots = equipSlotParent.GetComponentsInChildren<EquipSlot>();

            // slot ID �Ҵ� �� ������ ������ �̺�Ʈ ���_220515
            for (int i = 0; i < _equipSlots.Length; i++)
            {
                _equipSlots[i].SetSlotID(i); // ���� ���� ID ����
                _equipSlots[i].OnLeftClickEvent += OpenItemSpecificsPanel; // ������ �ܼ� ��Ŭ�� �� �ߵ� �̺�Ʈ ���
            }
        }

        // �÷��̾� �����Ϳ� ���� ��� ���� �ʱ�ȭ_220512
        public void Initialize()
        {
            _playerItemData = GameManager.Instance.DataManager.PlayerItemData;

            // �ʱ� �����Ϳ� ���� ��� ���� ���� �Ҵ�_220513
            for (int i = 0; i < _playerItemData.items.Count; i++)
            {
                if (_playerItemData.items[i].isEquiped)
                {
                    _tempItem = _playerItemData.items[i].item;

                    // �ش� ������ �������� ��� Ÿ�Կ� �´� ��� ������ Ž���Ͽ� �Ҵ�
                    for (int j = 0; j < _equipSlots.Length; j++)
                    {
                        if (!_equipSlots[j].IsAssigned && _equipSlots[j].slotEquipmentType.Equals(_tempItem.EquipmentType))
                        {
                            _equipSlots[j].AssignEquipment(_tempItem);

                            // ���� �Ǵ� ���� �������� ��� ���� �� �ݿ�
                            if (_tempItem.EquipmentData && (_tempItem.EquipmentType.Equals(EquipmentType.Weapon) ||
                                                            _tempItem.EquipmentType.Equals(EquipmentType.Shield)))
                            {
                                GameManager.Instance.Player.equipmentHolder
                                    .AssignEquipment(_tempItem.EquipmentData, _equipSlots[j].isPrimaryWeapon, true);
                            }
                        }
                    }
                }
            }

            // ĳ���� ���� ������Ʈ_220513
            uiManager.characterStatusManager.UpdateInformaion();
        }

        // ��� ������ ����_220512
        public void EquipItem(Item item)
        {
            for (int i = 0; i < _equipSlots.Length; i++)
            {
                // ������ ��� Ÿ�԰� ������ ���� Ž��
                if (_equipSlots[i].slotEquipmentType == item.EquipmentType)
                {
                    // �̹� �ٸ� ��� �Ҵ�Ǿ� �ִ� ��� �Ҵ�� ��� �κ��丮 ���Կ��� ���� ����
                    if (_equipSlots[i].IsAssigned)
                    {
                        // ���� ������ ������ ���� ���� ������Ʈ
                        GameManager.Instance.DataManager.UpdateItemData(_equipSlots[i].AssignedItem, false);
                        // �κ��丮 ���� ����
                        uiManager.inventoryManager.FindSlotByItem(_equipSlots[i].AssignedItem, true).EquipItem(false); 
                    }

                    // �ش� ���Կ� ��� ������ �Ҵ�
                    _equipSlots[i].AssignEquipment(item);

                    // ĳ���� ���� ������Ʈ
                    CalDefense();

                    // ���� ȿ�� ���
                    AudioManager.Instance.PlayAudio(Strings.Audio_FX_Player_Equip);

                    // ĳ���� ���� ������Ʈ_220513
                    uiManager.characterStatusManager.UpdateInformaion();
                    return;
                }
            }
        }

        // ��� ������ ���� ����_220512
        public void UnequipItem(Item item)
        {
            for (int i = 0; i < _equipSlots.Length; i++)
            {
                // ������ ��� Ÿ�԰� ������ ���� Ž��
                if (_equipSlots[i].IsAssigned && _equipSlots[i].slotEquipmentType == item.EquipmentType)
                {
                    // ���Կ� �Ҵ�� ��� ���� ����
                    _equipSlots[i].Unassign();

                    // ĳ���� ���� ������Ʈ
                    CalDefense();

                    // ���� ȿ�� ���
                    AudioManager.Instance.PlayAudio(Strings.Audio_FX_Player_Unequip);

                    // ĳ���� ���� ������Ʈ_220513
                    uiManager.characterStatusManager.UpdateInformaion();
                    return;
                }
            }
        }

        // ���� ����� �����۵��� ���ݷ� ����(�ּ� �� ~ �ִ� ��)�� ����Ͽ� Vector2������ ��ȯ_220510
        public Vector2 CalDamageRange()
        {
            // ���� ������ ������ ���ݷ� + �� ������ ���� �ջ��� ���ݷ� ǥ��
            int baseDamage = (int)((GameManager.Instance.DataManager.PlayerData.Level * 0.5f)
                            + (GameManager.Instance.DataManager.PlayerData.Str * 0.5f) + (GameManager.Instance.DataManager.PlayerData.Level + 9));
            int maxDamage = 0, minDamage = 0;

            // ������ �� ���� ������ ���� ��������
            for (int i = 0; i < _equipSlots.Length; i++)
            {
                // ����� ���� Ž��
                if (_equipSlots[i].IsAssigned && _equipSlots[i].AssignedItem.EquipmentType.Equals(EquipmentType.Weapon))
                {
                    _usingWeapon = _equipSlots[i].AssignedItem.EquipmentData as Weapon;

                    minDamage += _usingWeapon.AttackMinPower + baseDamage;
                    maxDamage += _usingWeapon.AttackMaxPower + baseDamage;
                }
            }
            attackRange.x = minDamage;
            attackRange.y = maxDamage;

            return attackRange;
        }

        // ���� ����� ��� �����۵��� ������ �ջ��Ͽ� �ÿ��̾� �������� ������ ������Ʈ_220530
        public void CalDefense()
        {
            int totalDefense = 0;

            // ������ ��� ������ ���� ��������
            for (int i = 0; i < _equipSlots.Length; i++)
            {
                // ����� ��� Ž��
                if (_equipSlots[i].IsAssigned && !_equipSlots[i].AssignedItem.EquipmentType.Equals(EquipmentType.Weapon))
                {
                    totalDefense += _equipSlots[i].AssignedItem.BaseAbility;
                }
            }

            GameManager.Instance.DataManager.PlayerData.Def = (uint)totalDefense;
        }

        // ��� �������� Ŭ���� ��� ������ ���� ���� �г� ���̱�_220515
        private void OpenItemSpecificsPanel(int slotIndex)
        {
            if (_equipSlots[slotIndex].IsAssigned)
            {
                uiManager.inventoryManager.itemSpecificsPanel.
                    SetPanel(_equipSlots[slotIndex].AssignedItem, _equipSlots[slotIndex].transform.position.x);
            }
        }

        private void OnDestroy()
        {
            // Event ����
            for (int i = 0; i < _equipSlots.Length; i++)
            {
                _equipSlots[i].OnLeftClickEvent -= OpenItemSpecificsPanel;
            }
        }
    }
}
