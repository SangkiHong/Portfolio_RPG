using UnityEngine;

namespace SK
{
    [System.Serializable]
    public enum EquipType { Weapon, Shield }

    [System.Serializable]
    public enum SheathPosition { SwordSheath, Back }

    public class Equipments : ScriptableObject
    {
        public EquipType equipType; // 장비 유형
        public bool isPrimary = true;
        public bool isShield;

        public GameObject modelPrefab; // 장비 모델 프리팹
        public SheathPosition sheathPosition; // 장착 위치
        public Vector3 sheathModelPosition; // Sheath에서의 위치값
        public Vector3 sheathModelRotation; // Sheath에서의 회전값
    }
}