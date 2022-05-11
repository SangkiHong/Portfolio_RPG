using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SK.Data
{
    public class ItemListManager : MonoBehaviour
    {
        public static ItemListManager Instance;

        public ItemList[] itemLists; // 0: Equipment, 1: Weapon, 2: Props

        private void Awake()
        {
            if (Instance != null) Destroy(this);
            
            Instance = this;
            DontDestroyOnLoad(this);
        }

        // 아이템 id로 아이템 불러오기_220503
        public Item GetItembyID(int id)
        {
            for (int i = 0; i < itemLists.Length; i++)
            {
                for (int j = 0; j < itemLists[i].itemList.Count; j++)
                {
                    if (itemLists[i].itemList[j].id == id) 
                        return itemLists[i].itemList[j];
                }
            }

            return null;
        }

        // 아이템 list, id로 아이템 불러오기_220503
        public Item GetItembyID(ItemList itemList, int id)
        {
            for (int i = 0; i < itemList.itemList.Count; i++)
            {
                if (itemList.itemList[i].id == id) 
                    return itemList.itemList[i];
            }

            return null;
        }

        // 아이템 id, type 등의 정보로 아이템 불러오기_220504
        public Item GetItem(int id, ItemType itemType, EquipmentType equipType)
        {
            int selectListIndex = 0; // 기본값으로 장비 아이템 리스트 선택

            if (itemType == ItemType.Equipment)
            {
                // 무기 아이템일 경우 List Index는 1
                if (equipType == EquipmentType.Weapon)
                    selectListIndex = 1;
            }
            else
            {
                // 기타 아이템일 경우 List Index는 2
                selectListIndex = 2;
            }

            // 선택된 리스트 인덱스를 통해 아이템 리스트 검색
            for (int i = 0; i < itemLists[selectListIndex].itemList.Count; i++)
            {
                if (itemLists[selectListIndex].itemList[i].id == id)
                    return itemLists[selectListIndex].itemList[i];
            }

            return null;
        }
    }
}
