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

        // ������ id�� ������ �ҷ�����_220503
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

        // ������ list, id�� ������ �ҷ�����_220503
        public Item GetItembyID(ItemList itemList, int id)
        {
            for (int i = 0; i < itemList.itemList.Count; i++)
            {
                if (itemList.itemList[i].id == id) 
                    return itemList.itemList[i];
            }

            return null;
        }

        // ������ id, type ���� ������ ������ �ҷ�����_220504
        public Item GetItem(int id, ItemType itemType, EquipmentType equipType)
        {
            int selectListIndex = 0; // �⺻������ ��� ������ ����Ʈ ����

            if (itemType == ItemType.Equipment)
            {
                // ���� �������� ��� List Index�� 1
                if (equipType == EquipmentType.Weapon)
                    selectListIndex = 1;
            }
            else
            {
                // ��Ÿ �������� ��� List Index�� 2
                selectListIndex = 2;
            }

            // ���õ� ����Ʈ �ε����� ���� ������ ����Ʈ �˻�
            for (int i = 0; i < itemLists[selectListIndex].itemList.Count; i++)
            {
                if (itemLists[selectListIndex].itemList[i].id == id)
                    return itemLists[selectListIndex].itemList[i];
            }

            return null;
        }
    }
}
