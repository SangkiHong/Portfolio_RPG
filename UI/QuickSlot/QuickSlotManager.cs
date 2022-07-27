using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace SK.UI
{
    /* 작성자: 홍상기
     * 내용: 게임 화면의 퀵슬롯과 UI를 관리하는 관리자 클래스
     * 작성일: 22년 6월 20일
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
            // 슬롯의 RectTransform을 배열에 저장
            _quickSlotRT = new RectTransform[quickSlots.Length];
            for (int i = 0; i < quickSlots.Length; i++)
            {
                // 슬롯 ID 할당
                quickSlots[i].SetSlotID(i);

                // 이벤트 할당
                quickSlots[i].OnDragEndEvent += OnDragEnd;
                quickSlots[i].OnAssignItem += CheckSameItem;
                quickSlots[i].OnAssignSkill += CheckSameSkill;
                
                // RectTransform 할당
                _quickSlotRT[i] = quickSlots[i].transform as RectTransform;
            }

            // 데이터에 따른 퀵 슬롯 할당
            for (int i = 0; i < quickSlotData.slotInfoList.Count; i++)
            {
                var index = quickSlotData.slotInfoList[i].slotIndex;

                // 스킬인 경우
                if (quickSlotData.slotInfoList[i].isSkill)
                    quickSlots[index].Assign(quickSlotData.slotInfoList[i].skill);
                // 아이템인 경우
                else
                    quickSlots[index].Assign(quickSlotData.slotInfoList[i].item,
                        quickSlotData.slotInfoList[i].itemAmount, quickSlotData.slotInfoList[i].isEquiped);
            }
        }

        // 퀵 슬롯 단축키 초기화
        public void Initialize(PlayerInput playerInput)
        {
            _playerInput = playerInput;

            // 숫자키 1~0까지의 단축키 할당
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

            // 스킬 전용 슬롯 단축키 할당
            _playerInput.actions["QuickSlot_SkillQ"].started += x => { quickSlots[10].Execute(); };
            _playerInput.actions["QuickSlot_SkillE"].started += x => { quickSlots[11].Execute(); };
        }

        public void AssignSkill(Data.SkillData skillData, bool isLeftSide)
        {
            var targetIndex = isLeftSide ? 10 : 11;
            quickSlots[targetIndex].Assign(skillData);
        }

        // 해당 아이템이 등록되어 있다면, 퀵슬롯에서 할당 해제
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

        // 해당 스킬이 할당된 퀵 슬롯을 반환
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

        // 퀵 슬롯에 드랍을 하여 퀵슬롯을 반환
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

        // 퀵 슬롯에 할당된 장비들의 착용 상태 업데이트
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

        // 퀼 슬롯에 할당된 아이템 상태 업데이트
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
        // 퀵 슬롯을 드래그하여 드랍한 경우 호출되는 이벤트 함수
        private void OnDragEnd(int slotID, PointerEventData eventData)
        {
            _targetSlot = TryDropSlot(eventData);

            // 타겟 슬롯이 널이 아닌 경우
            if (_targetSlot != null)
            {
                // 같은 슬롯에 드랍한 경우 리턴
                if (quickSlots[slotID] == _targetSlot) return;

                if (quickSlots[slotID].AssignedSkillData)
                    _targetSlot.Assign(quickSlots[slotID].AssignedSkillData);
                else if (quickSlots[slotID].AssignedItem != null)
                    _targetSlot.Assign(quickSlots[slotID].AssignedItem, quickSlots[slotID].ItemAmount, quickSlots[slotID].IsEquiped);
            }
            // 퀵 슬롯에 할당되지 않은 경우 할당 해제
            else
                quickSlots[slotID].Unassign();
        }
        
        // 동일한 아이템이 등록된 지 확인 후 할당 해제 이벤트 함수
        private void CheckSameItem(int slotID, Item item)
        {
            for (int i = 0; i < quickSlots.Length; i++)
            {
                if (i != slotID && quickSlots[i].IsAssigned &&
                    quickSlots[i].AssignedItem != null && quickSlots[i].AssignedItem == item)
                    quickSlots[i].Unassign();
            }
        }

        // 동일한 스킬이 등록된 지 확인 후 할당 해제 이벤트 함수
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

            // 슬롯 데이터 저장
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
                // 이벤트 할당 해제
                quickSlots[i].OnAssignItem -= CheckSameItem;
                quickSlots[i].OnAssignSkill -= CheckSameSkill;
            }
        }

        private void OnApplicationQuit()
        {
            // 단축키 할당 해제
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
