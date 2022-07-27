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

        // ������ id�� �������� Ž���Ͽ� ��ȯ_220503
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

        // ������ list, id�� �������� Ž���Ͽ� ��ȯ_220503
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

        // ������ list �ε���, id�� �������� Ž���Ͽ� ��ȯ_220610
        public Item GetItembyID(int itemListIndex, int id)
        {
            for (int i = 0; i < itemLists[itemListIndex].itemList.Count; i++)
            {
                if (itemLists[itemListIndex].itemList[i].Id == id)
                    return itemLists[itemListIndex].itemList[i];
            }

            return null;
        }

        // ������ id, type ���� ������ �������� Ž���Ͽ� ��ȯ_220504
        public Item GetItem(int id, ItemType itemType, EquipmentType equipType)
        {
            // �⺻������ ��� ������ ����Ʈ �ε��� ��
            int selectListIndex = 0; 

            if (itemType == ItemType.Equipment)
            {
                // ���� �������� ���
                if (equipType == EquipmentType.Weapon)
                    selectListIndex = 1;
            }
            else // ��Ÿ �������� ���
                selectListIndex = 2;

            // ���õ� ����Ʈ �ε����� ���� ������ ����Ʈ �˻�
            for (int i = 0; i < itemLists[selectListIndex].itemList.Count; i++)
            {
                if (itemLists[selectListIndex].itemList[i].Id == id)
                    return itemLists[selectListIndex].itemList[i];
            }

            return null;
        }

        // ��� �����͸� ���� �������� Ž���Ͽ� ��ȯ_220621
        public Item GetItem(Equipments equipment)
        {
            // ���� ������ ����Ʈ���� Ž��
            for (int i = 0; i < itemLists[1].itemList.Count; i++)
            {
                if (itemLists[1].itemList[i].EquipmentData == equipment)
                    return itemLists[1].itemList[i];
            }
            // ��� ������ ����Ʈ���� Ž��
            for (int i = 0; i < itemLists[0].itemList.Count; i++)
            {
                if (itemLists[0].itemList[i].EquipmentData == equipment)
                    return itemLists[0].itemList[i];
            }
            return null;
        }
    }
}
