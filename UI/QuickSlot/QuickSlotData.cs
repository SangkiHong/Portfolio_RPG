using System.Collections.Generic;
using UnityEngine;
using SK.Data;

namespace SK.UI
{
    [CreateAssetMenu(fileName = "QuickSlotData", menuName = "Game Data/QuickSlot")]
    public class QuickSlotData : ScriptableObject
    {
        [System.Serializable]
        public struct SlotInfo
        {
            public int slotIndex;
            public bool isSkill;
            public SkillData skill;
            public Item item;
            public uint itemAmount;
            public bool isEquiped;

            public SlotInfo(int _slotIndex, bool _isSkill, SkillData _skill,
                Item _item, uint _itemAmount, bool _isEquiped)
            {
                slotIndex = _slotIndex;
                isSkill = _isSkill;
                skill = _skill;
                item = _item;
                itemAmount = _itemAmount;
                isEquiped = _isEquiped;
            }
        }

        [SerializeField]
        public List<SlotInfo> slotInfoList;
    }
}