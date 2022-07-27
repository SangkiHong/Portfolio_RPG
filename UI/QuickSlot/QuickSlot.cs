using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace SK.UI
{
    /* 작성자: 홍상기
     * 내용: 퀵슬롯의 데이터와 UI를 관리하는 컴포넌트
     * 작성일: 22년 6월 20일
     */

    public class QuickSlot : SlotBase
    {
        public UnityAction<int, Data.SkillData> OnAssignSkill;
        public UnityAction<int, Item> OnAssignItem;

        [SerializeField] private bool isOnlySkill;
        [SerializeField] private Text text_ItemAmount;
        [SerializeField] private Slider coolTimeSlider;

        public Data.SkillData AssignedSkillData { get; private set; }
        public Item AssignedItem { get; private set; }
        public uint ItemAmount { get; private set; }
        public bool IsEquiped { get; private set; }

        private float _skillCoolTime;

        public void Assign(Data.SkillData skillData)
        {
            // 할당 시 다른 슬롯에 동일한 아이템이 있으면 해제하기 위해 이벤트 호출
            OnAssignSkill?.Invoke(slotID, skillData);
            // 슬롯 초기화
            Unassign();
            AssignedSkillData = skillData;
            _skillCoolTime = AssignedSkillData.skillCoolTime;
            base.Assign(skillData.skillIcon);
            if (text_ItemAmount)
                text_ItemAmount.gameObject.SetActive(false);
        }

        public bool Assign(Item item, uint amount, bool isEquiped)
        {
            // 사용 가능한 아이템이거나 장비 아이템인 경우 할당
            if (!isOnlySkill && (item.IsConsumable || item.ItemType == ItemType.Equipment))
            {
                // 할당 시 다른 슬롯에 동일한 아이템이 있으면 해제하기 위해 이벤트 호출
                OnAssignItem?.Invoke(slotID, item);

                // 슬롯 초기화
                Unassign();
                AssignedItem = item;

                base.Assign(item.ItemIcon);
                IsEquiped = isEquiped;
                ItemAmount = amount;
                UpdateAmountText();

                return true;
            }

            return false;
        }

        public override void Unassign()
        {
            base.Unassign();
            AssignedSkillData = null;
            AssignedItem = null;
            IsEquiped = false;
            ItemAmount = 0;
            coolTimeSlider.gameObject.SetActive(false);
            if (text_ItemAmount)
                text_ItemAmount.gameObject.SetActive(false);
        }

        // 퀵 슬롯 사용
        public void Execute()
        {
            // 할당되지 않은 슬롯인 경우 리턴
            if (!IsAssigned) return;

            // 아이템이 할당된 경우
            if (AssignedItem != null)
            {
                // 아이템 사용 또는 착용,해제
                if (UIManager.Instance.inventoryManager.UseItem(AssignedItem, ItemAmount, 1))
                {
                    if (AssignedItem.IsConsumable)
                        ItemAmount -= 1;

                    UpdateAmountText();
                }
            }
            else if (AssignedSkillData != null)
            {
                // 스킬 사용
                UIManager.Instance.skillManager.UseSkill(AssignedSkillData);
            }
        }

        // 수량 변경
        public void UpdateItemAmount(uint changeAmount)
        {
            ItemAmount = changeAmount;
            UpdateAmountText();
        }

        // 수량 표시 업데이트
        private void UpdateAmountText()
        {
            if (!text_ItemAmount) return;

            // 아이템의 갯수가 2 이상인 경우
            if (ItemAmount > 1 || IsEquiped)
            {
                // 일반 아이템인 경우 수량 표시
                if (AssignedItem.ItemType != ItemType.Equipment)
                    text_ItemAmount.text = ItemAmount.ToString();
                else // 장비 아이템인 경우 착용 표시
                    text_ItemAmount.text = Strings.ETC_EquipSign;

                text_ItemAmount.gameObject.SetActive(true);
            }
            else
                text_ItemAmount.gameObject.SetActive(false);
        }

        // 장비 착용 상태 업데이트
        public void UpdateEquipState(bool isEquiped)
        {
            IsEquiped = isEquiped;
            UpdateAmountText();
        }

        // 스킬 사용 시 쿨타임 표시
        public void UpdateCoolTime(float remainCoolTime)
        {
            var value = remainCoolTime / _skillCoolTime;
            if (value > 0)
            {
                if (!coolTimeSlider.gameObject.activeSelf)
                    coolTimeSlider.gameObject.SetActive(true);
                coolTimeSlider.value = value;
            }
            else
                coolTimeSlider.gameObject.SetActive(false);
        }
    }
}