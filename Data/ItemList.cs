using System.Collections.Generic;
using UnityEngine;

namespace Sangki
{
    [CreateAssetMenu(menuName = "Game Data/Item List", fileName = "ItemList_")]
    public class ItemList : ScriptableObject
    {
        public List<Item> itemList;
    }
}
