using UnityEngine;

namespace SK
{
    public class Strings
    {
        #region Animation
        public static readonly int AnimPara_onCombat = Animator.StringToHash("onCombat");
        public static readonly int animPara_isInteracting = Animator.StringToHash("isInteracting");
        public static readonly int AnimPara_MoveBlend = Animator.StringToHash("MoveBlend");
        public static readonly int AnimPara_Sideways = Animator.StringToHash("Sideways");
        public static readonly int AnimPara_EquipShield = Animator.StringToHash("EquipShield");
        public static readonly int AnimPara_isAttacking = Animator.StringToHash("isAttacking");
        public static readonly int AnimPara_isShielding = Animator.StringToHash("isShielding");
        public static readonly int AnimPara_isChangingEquipState = Animator.StringToHash("isChangingEquipState");
        public static readonly int AnimPara_Jump = Animator.StringToHash("Jump");
        public static readonly int AnimPara_Land = Animator.StringToHash("Land");
        public static readonly int AnimPara_Damaged = Animator.StringToHash("Damaged");
        public static readonly int AnimPara_StrongDamaged = Animator.StringToHash("StrongDamaged");
        public static readonly int AnimPara_Dead = Animator.StringToHash("Dead");
        public static readonly int AnimPara_onRushAttack = Animator.StringToHash("onRushAttack");
        public static readonly string AnimName_Roll = "Roll_Forward";
        public static readonly string AnimName_RollBack = "Roll_Backward";
        public static readonly string AnimName_DodgeRight = "Dodge_Right";
        public static readonly string AnimName_DodgeLeft = "Dodge_Left";
        public static readonly string AnimName_Equip_SwordSheath = "Equip_SwordSheath";
        public static readonly string AnimName_Unequip_SwordSheath = "Unequip_SwordSheath";
        public static readonly string AnimName_Equip_BackSheath = "Equip_BackSheath";
        public static readonly string AnimName_Unequip_BackSheath = "Unequip_BackSheath";
        public static readonly string AnimName_Equip_Shield = "Equip_Shield";
        public static readonly string AnimName_Unequip_Shield = "Unequip_Shield";
        public static readonly string AnimName_Shielding = "Shielding";
        public static readonly string AnimName_Shield_Hit = "Shield_Hit";
        public static readonly string AnimName_Weapon_Block = "Weapon_Block";
        #endregion

        #region Pool
        public static readonly string PoolName_DamagePoint = "DamagePoint";
        public static readonly string PoolName_QuestTitle = "QuestTitle";
        public static readonly string PoolName_QuestTask = "QuestTask";
        #endregion

        #region Info Text
        public static readonly string Info_DeleteItem_TItle = "아이템 삭제 확인";
        public static readonly string Info_DeleteItem_Info = "아이템 삭제를 원하시면 확인 버튼을 누르세요.";
        public static readonly string Info_SellItem_Title = "아이템 판매 확인";
        public static readonly string Info_SellItem_Info = "아이템 판매를 원하시면 확인 버튼을 누르세요.";
        public static readonly string Info_UnequipItem_Title = "아이템 착용 해제 확인";
        public static readonly string Info_UnequipItem_Info = "아이템을 먼저 착용 해제하세요.";
        public static readonly string Info_NotEnoughCurruncy_Title = "금액 부족 확인";
        public static readonly string Info_NotEnoughCurruncy_Info = "구매하기에 부족한 금액입니다.";
        public static readonly string Info_NotEnoughSlot_Title = "슬롯 공간 부족";
        public static readonly string Info_NotEnoughSlot_Info = "인벤토리 공간이 꽉 찼습니다.";
        #endregion

        #region Dialog
        public static readonly string Dialog_InventoryFull = "Dialog_InventoryFull";
        #endregion

        #region Quest
        public static readonly string QuestMiniInfo_Bar = "- ";
        public static readonly string QuestMiniInfo_OpenBraket = " (";
        public static readonly string QuestMiniInfo_CloseBraket = ")";
        public static readonly string QuestMiniInfo_Slash = " / ";
        public static readonly string QuestMiniInfo_Success = "<color=#FDE50DFF> 완료 </color>";
        #endregion

        #region Skill
        public static readonly string Skill_ActiveSkill = "액티브 스킬";
        public static readonly string Skill_Passive = "패시브 스킬";
        #endregion

        #region Audio
        // BGM
        public static readonly string Audio_BGM_Boss = "BGM_Boss";
        public static readonly string Audio_BGM_Combat = "BGM_Combat";
        public static readonly string Audio_BGM_Death = "BGM_Death";
        public static readonly string Audio_BGM_Field = "BGM_Field";
        public static readonly string Audio_BGM_Village = "BGM_Village";

        // UI
        public static readonly string Audio_UI_OnButton = "UI_OnButton";
        public static readonly string Audio_UI_QuestAccept = "UI_QuestAccept";
        public static readonly string Audio_UI_CompleteQuest = "UI_CompleteQuest";
        public static readonly string Audio_UI_BuyItem = "UI_BuyItem";
        public static readonly string Audio_UI_SellItem = "UI_SellItem";
        public static readonly string Audio_FX_LevelUp = "FX_LevelUp";
        public static readonly string Audio_FX_NewLocation = "FX_NewLocation";
        public static readonly string Audio_FX_Player_Heal = "FX_Player_Heal";

        // Impact
        public static readonly string[] Audio_FX_Hit_NormalImpact = { "FX_Hit_NormalImpact_1", "FX_Hit_NormalImpact_2", "FX_Hit_NormalImpact_3" };
        public static readonly string[] Audio_FX_Hit_MiddleImpact = { "FX_Hit_MiddleImpact_1", "FX_Hit_MiddleImpact_2" };
        public static readonly string[] Audio_FX_Hit_HeavyImpact = { "FX_Hit_HeavyImpact_1", "FX_Hit_HeavyImpact_2" };
        public static readonly string[] Audio_FX_Hit_ShieldImpact = { "FX_Hit_ShieldImpact_1", "FX_Hit_ShieldImpact_2", "FX_Hit_ShieldImpact_3" };

        // Player
        public static readonly string Audio_FX_Player_Equip = "FX_Player_Equip";
        public static readonly string Audio_FX_Player_Unequip = "FX_Player_Unequip";
        public static readonly string Audio_FX_Player_Unsheath = "FX_Player_Unsheath";
        public static readonly string Audio_FX_Player_Jump = "FX_Player_Jump";
        public static readonly string Audio_FX_Player_Land = "FX_Player_Land";
        public static readonly string[] Audio_FX_Player_Footstep = { "FX_Player_Footstep_1", "FX_Player_Footstep_2" };
        public static readonly string[] Audio_FX_Player_Movement = { "FX_Player_Movement_1", "FX_Player_Movement_2", "FX_Player_Movement_3",
                                                                     "FX_Player_Movement_4", "FX_Player_Movement_5", "FX_Player_Movement_6",};
        // Weapon
        public static readonly string[] Audio_FX_WeaponSword = { "FX_WeaponSword_1", "FX_WeaponSword_2", "FX_WeaponSword_3", "FX_WeaponSword_4",
                                                                 "FX_WeaponSword_5", "FX_WeaponSword_6", "FX_WeaponSword_7", "FX_WeaponSword_8" };
        public static readonly string[] Audio_FX_Whoosh = { "FX_Whoosh_1", "FX_Whoosh_2", "FX_Whoosh_3", "FX_Whoosh_4" };

        // Voice
        public static readonly string[] Audio_FX_Voice_Attack = { "FX_Voice_Attack_1", "FX_Voice_Attack_2" };
        public static readonly string[] Audio_FX_Voice_Death = { "FX_Voice_Death_1", "FX_Voice_Death_2" };
        public static readonly string[] Audio_FX_Voice_Grunt_Male = { "FX_Voice_Grunt_Male_1", "FX_Voice_Grunt_Male_2", "FX_Voice_Grunt_Male_3", "FX_Voice_Grunt_Male_4" };
        public static readonly string[] Audio_FX_Voice_Hit = { "FX_Voice_Hit_1", "FX_Voice_Hit_2" };
        public static readonly string[] Audio_FX_Voice_Pain = { "FX_Voice_Pain_1", "FX_Voice_Pain_2" };
        public static readonly string[] Audio_FX_Voice_Player_Pain = { "FX_Voice_Player_Pain_1", "FX_Voice_Player_Pain_2" };
        public static readonly string   Audio_FX_Voice_Shout = "FX_Voice_Shout";

        public static readonly string[] Audio_FX_Voice_Troll_Attack = { "FX_Voice_Troll_Attack_1", "FX_Voice_Troll_Attack_2" };
        public static readonly string[] Audio_FX_Voice_Troll_Pain = { "FX_Voice_Troll_Pain_1", "FX_Voice_Troll_Pain_2", "FX_Voice_Troll_Pain_3" };
        public static readonly string   Audio_FX_Voice_Troll_Shout = "FX_Voice_Troll_Shout";
        public static readonly string   Audio_FX_Voice_Troll_Death = "FX_Voice_Troll_Death";

        // Enemy
        public static readonly string[] Audio_FX_Footstep_Normal = { "FX_Footstep_Normal_1", "FX_Footstep_Normal_2", "FX_Footstep_Normal_3", 
                                                                     "FX_Footstep_Normal_4", "FX_Footstep_Normal_5", "FX_Footstep_Normal_6" };
        public static readonly string[] Audio_FX_Footstep_Heavy = { "FX_Footstep_Heavy_1", "FX_Footstep_Heavy_2" };
        public static readonly string Audio_FX_Troll_JumpAttack = "FX_Troll_JumpAttack";
        #endregion

        #region ETC
        public static readonly string Tag_Player = "Player";
        public static readonly string ETC_Enemy = "Enemy";
        public static readonly string ETC_EquipSign = "E";
        #endregion
    }
}