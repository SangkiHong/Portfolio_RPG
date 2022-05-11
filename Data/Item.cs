using UnityEngine;

namespace SK
{
    public enum ItemType : int
    { 
        Default,
        Food,
        Buff,
        Quest,
        Equipment
    }

    public enum ItemGrade
    { 
        Normal,
        Magic,
        Rare,
        Epic,
        Unique,
        Legendary
    }

    public enum EquipmentType
    {
        Weapon,
        Armor,
        Shield,
        Gloves,
        Helmet,
        Pants,
        Belt,
        Boots,
        Ring
    }

    [System.Serializable]
    public class Item
    {
        public int id = 0;
        public string itemName = "New Item";
        public Sprite itemIcon = null;
        public ItemType itemType;
        public ItemGrade itemGrade;
        // 소모 가능한 아이템 여부
        public bool isConsumable = false;
        // 아이템 중첩 수량 가능 여부
        public bool isStackable = false;

        // 장비 타입
        public EquipmentType equipmentType;
        // 장비 데이터 세부 정보
        public Equipments equipmentData;
        // 아이템 무게
        public float weight;
        // 아이템 내구도
        public int durability;

        // 아이템 설명
        public string description;

        // 아이템 사용 가능 레벨
        public int requiredLevel;
        // 착용 기본 능력치(공격력/방어력)
        public int baseAbility;
        // 착용 추가 능력치(치명타/회피력)
        public int subAbility;

        // 착용 보너스 힘
        public int bonus_Str;
        // 착용 보너스 민첩
        public int bonus_Dex;
        // 착용 보너스 지능
        public int bonus_Int;

        // 아이템 사용 시 회복량
        public int recoverHPAmount;
        // 아이템 사용 시 버프 지속 시간
        public float buff_Duration;
        // 아이템 사용 시 힘 상승
        public int buff_Str;
        // 아이템 사용 시 민첩 상승
        public int buff_Dex;
        // 아이템 사용 시 지능 상승
        public int buff_Int;
    }
}
