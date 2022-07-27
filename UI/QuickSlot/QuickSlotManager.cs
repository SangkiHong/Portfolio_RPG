using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace SK.UI
{
    /* �ۼ���: ȫ���
     * ����: ���� ȭ���� �����԰� UI�� �����ϴ� ������ Ŭ����
     * �ۼ���: 22�� 6�� 20��
     */

    public class QuickSlotManager : MonoBehaviour
    {
        [SerializeField] private QuickSlotData quickSlotData;
        [SerializeField] private QuickSlot[] quickSlots;
        
        private RectTransform[] _quickSlotRT;

        public IReadOnlyCollection<QuickSlot> QuickSlots => quickSlots;

        private PlayerInput _playerInput;
        private QuickSlot _targetSlot;

        private void Awake()
        {
            // ������ RectTransform�� �迭�� ����
            _quickSlotRT = new RectTransform[quickSlots.Length];
            for (int i = 0; i < quickSlots.Length; i++)
            {
                // ���� ID �Ҵ�
                quickSlots[i].SetSlotID(i);

                // �̺�Ʈ �Ҵ�
                quickSlots[i].OnDragEndEvent += OnDragEnd;
                quickSlots[i].OnAssignItem += CheckSameItem;
                quickSlots[i].OnAssignSkill += CheckSameSkill;
                
                // RectTransform �Ҵ�
                _quickSlotRT[i] = quickSlots[i].transform as RectTransform;
            }

            // �����Ϳ� ���� �� ���� �Ҵ�
            for (int i = 0; i < quickSlotData.slotInfoList.Count; i++)
            {
                var index = quickSlotData.slotInfoList[i].slotIndex;

                // ��ų�� ���
                if (quickSlotData.slotInfoList[i].isSkill)
                    quickSlots[index].Assign(quickSlotData.slotInfoList[i].skill);
                // �������� ���
                else
                    quickSlots[index].Assign(quickSlotData.slotInfoList[i].item,
                        quickSlotData.slotInfoList[i].itemAmount, quickSlotData.slotInfoList[i].isEquiped);
            }
        }

        // �� ���� ����Ű �ʱ�ȭ
        public void Initialize(PlayerInput playerInput)
        {
            _playerInput = playerInput;

            // ����Ű 1~0������ ����Ű �Ҵ�
            _playerInput.actions["QuickSlot_1"].started += x => { quickSlots[0].Execute(); };
            _playerInput.actions["QuickSlot_2"].started += x => { quickSlots[1].Execute(); };
            _playerInput.actions["QuickSlot_3"].started += x => { quickSlots[2].Execute(); };
            _playerInput.actions["QuickSlot_4"].started += x => { quickSlots[3].Execute(); };
            _playerInput.actions["QuickSlot_5"].started += x => { quickSlots[4].Execute(); };
            _playerInput.actions["QuickSlot_6"].started += x => { quickSlots[5].Execute(); };
            _playerInput.actions["QuickSlot_7"].started += x => { quickSlots[6].Execute(); };
            _playerInput.actions["QuickSlot_8"].started += x => { quickSlots[7].Execute(); };
            _playerInput.actions["QuickSlot_9"].started += x => { quickSlots[8].Execute(); };
            _playerInput.actions["QuickSlot_0"].started += x => { quickSlots[9].Execute(); };

            // ��ų ���� ���� ����Ű �Ҵ�
            _playerInput.actions["QuickSlot_SkillQ"].started += x => { quickSlots[10].Execute(); };
            _playerInput.actions["QuickSlot_SkillE"].started += x => { quickSlots[11].Execute(); };
        }

        public void AssignSkill(Data.SkillData skillData, bool isLeftSide)
        {
            var targetIndex = isLeftSide ? 10 : 11;
            quickSlots[targetIndex].Assign(skillData);
        }

        // �ش� �������� ��ϵǾ� �ִٸ�, �����Կ��� �Ҵ� ����
        public void UnassignItem(Item item)
        {
            for (int i = 0; i < quickSlots.Length; i++)
            {
                if (quickSlots[i].IsAssigned && quickSlots[i].AssignedItem != null &&
                    quickSlots[i].AssignedItem == item)
                {
                    quickSlots[i].Unassign();
                    return;
                }
            }
        }

        // �ش� ��ų�� �Ҵ�� �� ������ ��ȯ
        public QuickSlot GetSlotBySkill(Data.SkillData skillData)
        {
            for (int i = 0; i < quickSlots.Length; i++)
            {
                if (quickSlots[i].IsAssigned && quickSlots[i].AssignedSkillData
                    && quickSlots[i].AssignedSkillData == skillData)
                    return quickSlots[i];
            }

            return null;
        }

        // �� ���Կ� ����� �Ͽ� �������� ��ȯ
        public QuickSlot TryDropSlot(PointerEventData eventData)
        {
            var dropPos = eventData.position;
            Vector3 rectPos;
            float width, height;

            for (int i = 0; i < _quickSlotRT.Length; i++)
            {
                rectPos = _quickSlotRT[i].position;
                width = _quickSlotRT[i].rect.width * 0.5f;
                height = _quickSlotRT[i].rect.height * 0.5f;

                if (rectPos.x - width <= dropPos.x && dropPos.x <= rectPos.x + width &&
                    rectPos.y - height <= dropPos.y && dropPos.y <= rectPos.y + height)
                    return quickSlots[i]; 
            }

            return null;
        }

        // �� ���Կ� �Ҵ�� ������ ���� ���� ������Ʈ
        public void CheckEquipState(Item item, bool isEquiped)
        {
            for (int i = 0; i < quickSlots.Length; i++)
            {
                if (quickSlots[i].IsAssigned && quickSlots[i].AssignedItem != null && quickSlots[i].AssignedItem == item)
                {
                    quickSlots[i].UpdateEquipState(isEquiped);
                    break;
                }
            }
        }

        // �� ���Կ� �Ҵ�� ������ ���� ������Ʈ
        public void CheckItemState(Item item, uint changeAmount)
        {
            for (int i = 0; i < quickSlots.Length; i++)
            {
                if (quickSlots[i].IsAssigned && quickSlots[i].AssignedItem != null && quickSlots[i].AssignedItem == item)
                {
                    quickSlots[i].UpdateItemAmount(changeAmount);
                    break;
                }
            }
        }

        #region Event
        // �� ������ �巡���Ͽ� ����� ��� ȣ��Ǵ� �̺�Ʈ �Լ�
        private void OnDragEnd(int slotID, PointerEventData eventData)
        {
            _targetSlot = TryDropSlot(eventData);

            // Ÿ�� ������ ���� �ƴ� ���
            if (_targetSlot != null)
            {
                // ���� ���Կ� ����� ��� ����
                if (quickSlots[slotID] == _targetSlot) return;

                if (quickSlots[slotID].AssignedSkillData)
                    _targetSlot.Assign(quickSlots[slotID].AssignedSkillData);
                else if (quickSlots[slotID].AssignedItem != null)
                    _targetSlot.Assign(quickSlots[slotID].AssignedItem, quickSlots[slotID].ItemAmount, quickSlots[slotID].IsEquiped);
            }
            // �� ���Կ� �Ҵ���� ���� ��� �Ҵ� ����
            else
                quickSlots[slotID].Unassign();
        }
        
        // ������ �������� ��ϵ� �� Ȯ�� �� �Ҵ� ���� �̺�Ʈ �Լ�
        private void CheckSameItem(int slotID, Item item)
        {
            for (int i = 0; i < quickSlots.Length; i++)
            {
                if (i != slotID && quickSlots[i].IsAssigned &&
                    quickSlots[i].AssignedItem != null && quickSlots[i].AssignedItem == item)
                    quickSlots[i].Unassign();
            }
        }

        // ������ ��ų�� ��ϵ� �� Ȯ�� �� �Ҵ� ���� �̺�Ʈ �Լ�
        private void CheckSameSkill(int slotID, Data.SkillData skillData)
        {
            for (int i = 0; i < quickSlots.Length; i++)
            {
                if (i != slotID && quickSlots[i].IsAssigned && 
                    quickSlots[i].AssignedSkillData != null && quickSlots[i].AssignedSkillData == skillData)
                    quickSlots[i].Unassign();
            }
        }
        #endregion

        private void OnDisable()
        {
            quickSlotData.slotInfoList.Clear();

            // ���� ������ ����
            for (int i = 0; i < quickSlots.Length; i++)
            {
                if (quickSlots[i].IsAssigned)
                {
                    bool isSkill = quickSlots[i].AssignedItem == null;
                    quickSlotData.slotInfoList.Add(new QuickSlotData.SlotInfo(
                        i, isSkill, isSkill ? quickSlots[i].AssignedSkillData : null,
                        !isSkill ? quickSlots[i].AssignedItem : null,
                        isSkill ? 0 : quickSlots[i].ItemAmount,
                        isSkill ? false : quickSlots[i].IsEquiped
                    ));
                }
            }

            for (int i = 0; i < quickSlots.Length; i++)
            {
                // �̺�Ʈ �Ҵ� ����
                quickSlots[i].OnAssignItem -= CheckSameItem;
                quickSlots[i].OnAssignSkill -= CheckSameSkill;
            }
        }

        private void OnApplicationQuit()
        {
            // ����Ű �Ҵ� ����
            _playerInput.actions["QuickSlot_1"].started -= x => { quickSlots[0].Execute(); };
            _playerInput.actions["QuickSlot_2"].started -= x => { quickSlots[1].Execute(); };
            _playerInput.actions["QuickSlot_3"].started -= x => { quickSlots[2].Execute(); };
            _playerInput.actions["QuickSlot_4"].started -= x => { quickSlots[3].Execute(); };
            _playerInput.actions["QuickSlot_5"].started -= x => { quickSlots[4].Execute(); };
            _playerInput.actions["QuickSlot_6"].started -= x => { quickSlots[5].Execute(); };
            _playerInput.actions["QuickSlot_7"].started -= x => { quickSlots[6].Execute(); };
            _playerInput.actions["QuickSlot_8"].started -= x => { quickSlots[7].Execute(); };
            _playerInput.actions["QuickSlot_9"].started -= x => { quickSlots[8].Execute(); };
            _playerInput.actions["QuickSlot_0"].started -= x => { quickSlots[9].Execute(); };
            _playerInput.actions["QuickSlot_SkillQ"].started -= x => { quickSlots[10].Execute(); };
            _playerInput.actions["QuickSlot_SkillE"].started -= x => { quickSlots[11].Execute(); };
        }
    }
}
