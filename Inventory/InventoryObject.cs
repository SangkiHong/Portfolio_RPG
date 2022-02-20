using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sangki {
    [CreateAssetMenu(fileName = "New Inventory", menuName = "Inventory System/Inventory")]
    public class InventoryObject : ScriptableObject
    {
        public List<InventorySlot> Container = new List<InventorySlot>();
        public void AddItem(Item _item, int _amount)
        {
            bool hasItem = false;
            for (int i = 0; i < Container.Count; i++)
            {
                if (Container[i].item == _item)
                {
                    Container[i].AddAmount(_amount);
                    hasItem = true;
                    break;
                }
            }
            if (!hasItem) 
            {
                Container.Add(new InventorySlot(_item, _amount));
            }
        }
    }

    [System.Serializable]
    public class InventorySlot 
    {
        public Item item;
        public int amount;
        public InventorySlot(Item _item, int _amount)
        {
            item = _item;
            amount = _amount;
        }

        public void AddAmount(int value) => amount += value;
    }
}
