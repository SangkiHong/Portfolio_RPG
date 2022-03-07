using UnityEngine;

namespace SK
{
    public enum ItemType
    { 
        Default,
        Food,
        Posion,
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
        Boots,
        Ring,
        Necklace
    }

    [System.Serializable]
    public class Item
    {
        public int id = 0;
        public string itemName = "New Item";
        public Texture2D itemIcon = null;
        public ItemType itemType;
        public ItemGrade itemGrade;
        public bool isConsumable = false;
        public bool isStackable = false;

        public EquipmentType equipmentType;
        public int requiredLevel;
        public int stat_Str;
        public int stat_Dex;
        public int stat_Int;

        public int recoverHP;
        public int buff_Str;
        public int buff_Dex;
        public int buff_Int;
    }
}
