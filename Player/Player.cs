using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SK 
{
    public class Player : MonoBehaviour
    {
        public InventoryObject inventory;

        readonly string _Tag_Item = "Item";

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(_Tag_Item))
            {
                var item = other.GetComponent<ItemObject>();
                if (item != null)
                {
                    inventory.AddItem(ItemListManager.Instance.GetItembyID(item.itemID), 1);
                    Destroy(other.gameObject);
                }
            }
        }
    }
}