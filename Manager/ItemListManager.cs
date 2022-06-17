using UnityEngine;

namespace SK.Data
{
    public class ItemListManager : MonoBehaviour
    {
        public ItemList[] itemLists; // 0: Equipment, 1: Weapon, 2: Props

        // ������ id�� ������ �ҷ�����_220503
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

        // ������ list, id�� ������ �ҷ�����_220503
        public Item GetItembyID(ItemList itemList, int id)
        {
            for (int i = 0; i < itemList.itemList.Count; i++)
            {
                if (itemList.itemList[i].Id == id) 
                    return itemList.itemList[i];
            }

            return null;
        }

        // ������ list �ε���, id�� ������ �ҷ�����_220610
        public Item GetItembyID(int itemListIndex, int id)
        {
            for (int i = 0; i < itemLists[itemListIndex].itemList.Count; i++)
            {
                if (itemLists[itemListIndex].itemList[i].Id == id)
                    return itemLists[itemListIndex].itemList[i];
            }

            return null;
        }

        // ������ id, type ���� ������ ������ �ҷ�����_220504
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
    }
}
