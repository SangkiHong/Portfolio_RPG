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
        public SheathPosition sheathPosition; // 장착 위치
        public GameObject modelPrefab; // 장비 모델 프리팹

        public virtual void ExecuteAction(Animator anim, bool setDefault = false) { }
        public virtual void ExecuteSpecialAction(Animator anim, AttackType attackType, int index) { }
    }
}