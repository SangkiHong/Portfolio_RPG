using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SK
{
    public class ItemListManager : MonoBehaviour
    {
        public static ItemListManager Instance;

        public ItemList[] itemLists;

        private void Awake()
        {
            if (Instance != null) Destroy(this);
            
            Instance = this;
            DontDestroyOnLoad(this);
        }

        public Item GetItembyID(int _id)
        {
            for (int i = 0; i < itemLists.Length; i++)
            {
                for (int j = 0; j < itemLists[i].itemList.Count; j++)
                {
                    if (itemLists[i].itemList[j].id == _id) return itemLists[i].itemList[j];
                }
            }

            return null;
        }
    }
}
