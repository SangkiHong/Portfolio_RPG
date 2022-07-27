using UnityEngine;

namespace SK.Data
{
    public enum ItemListType: int
    {
        Equipment,
        Weapon,
        Props
    }

    public class ItemListManager : MonoBehaviour
    {
        public ItemList[] itemLists; // 0: Equipment, 1: Weapon, 2: Props

        // 아이템 id로 아이템을 탐색하여 반환_220503
        public Item GetItembyID(int id)
        {
            for (int i = 0; i < itemLists.Length; i++)
            {
                for (int j = 0; j < itemLists[i].itemList.Count; j++)
                {
                    if (itemLists[i].itemList[j].Id == id) 
                        return itemLists[i].itemList[j];
                }
            }

            return null;
        }

        // 아이템 list, id로 아이템을 탐색하여 반환_220503
        public Item GetItembyID(ItemListType listType, int id)
        {
            int listIndex = (int)listType;
            for (int i = 0; i < itemLists[listIndex].itemList.Count; i++)
            {
                if (itemLists[listIndex].itemList[i].Id == id) 
                    return itemLists[listIndex].itemList[i];
            }

            return null;
        }

        // 아이템 list 인덱스, id로 아이템을 탐색하여 반환_220610
        public Item GetItembyID(int itemListIndex, int id)
        {
            for (int i = 0; i < itemLists[itemListIndex].itemList.Count; i++)
            {
                if (itemLists[itemListIndex].itemList[i].Id == id)
                    return itemLists[itemListIndex].itemList[i];
            }

            return null;
        }

        // 아이템 id, type 등의 정보로 아이템을 탐색하여 반환_220504
        public Item GetItem(int id, ItemType itemType, EquipmentType equipType)
        {
            // 기본값으로 장비 아이템 리스트 인덱스 값
            int selectListIndex = 0; 

            if (itemType == ItemType.Equipment)
            {
                // 무기 아이템일 경우
                if (equipType == EquipmentType.Weapon)
                    selectListIndex = 1;
            }
            else // 기타 아이템일 경우
                selectListIndex = 2;

            // 선택된 리스트 인덱스를 통해 아이템 리스트 검색
            for (int i = 0; i < itemLists[selectListIndex].itemList.Count; i++)
            {
                if (itemLists[selectListIndex].itemList[i].Id == id)
                    return itemLists[selectListIndex].itemList[i];
            }

            return null;
        }

        // 장비 데이터를 통해 아이템을 탐색하여 반환_220621
        public Item GetItem(Equipments equipment)
        {
            // 무기 아이템 리스트에서 탐색
            for (int i = 0; i < itemLists[1].itemList.Count; i++)
            {
                if (itemLists[1].itemList[i].EquipmentData == equipment)
                    return itemLists[1].itemList[i];
            }
            // 장비 아이템 리스트에서 탐색
            for (int i = 0; i < itemLists[0].itemList.Count; i++)
            {
                if (itemLists[0].itemList[i].EquipmentData == equipment)
                    return itemLists[0].itemList[i];
            }
            return null;
        }
    }
}
