using UnityEngine;

namespace SK
{
    public class AnimParas
    {
        public static readonly int AnimPara_isFight = Animator.StringToHash("isFight");
        public static readonly int animPara_isInteracting = Animator.StringToHash("isInteracting");
        public static readonly int AnimPara_MoveBlend = Animator.StringToHash("MoveBlend");
        public static readonly int AnimPara_isShield = Animator.StringToHash("isShield");
        public static readonly int AnimPara_isShielding = Animator.StringToHash("isShielding");
        public static readonly int AnimPara_Attack = Animator.StringToHash("Attack");
        public static readonly int AnimPara_Jump = Animator.StringToHash("Jump");
        public static readonly int AnimPara_Land = Animator.StringToHash("Land");
        public static readonly int AnimPara_ComboIndex = Animator.StringToHash("ComboIndex");
        public static readonly string AnimName_Roll = "Roll_Forward";
        public static readonly string AnimName_RollBack = "Roll_Backward";
        public static readonly string AnimName_DodgeRight = "Dodge_Right";
        public static readonly string AnimName_DodgeLeft = "Dodge_Left";
        public static readonly string AnimName_Equip_Sword = "Equip_Sword";
        public static readonly string AnimName_Unequip_Sword = "Unequip_Sword";
        public static readonly string AnimName_Equip_Shield = "Equip_Shield";
        public static readonly string AnimName_Unequip_Shield = "Unequip_Shield";
        public static readonly string AnimName_Shielding = "Shielding";
    }
}