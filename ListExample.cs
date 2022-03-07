using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SK
{
    public class Info
    {
        public int data;
    }
    
    public class ListExample : MonoBehaviour
    {
        private int[] _array;
        private const int ARRAYMAX = 50;

        private List<int> list;

        private List<GameObject> NPCs;
        
        void Start()
        {
            // Array
            _array = new int[ARRAYMAX];
            
            for (int i = 0; i < _array.Length; i++)
            {
                _array[i] = i;
            }

            // List
            list = new List<int>();
            list.Add(0);
            list.Add(1);
            list.Add(2);

            for (int i = 0; i < list.Count; i++)
            {
                Debug.Log(list[i]);
            }

            foreach (var l in list)
            {
                Debug.Log(l);
            }

            NPCs = new List<GameObject>(GameObject.FindGameObjectsWithTag("NPC"));

            for (int i = 0; i < NPCs.Count; i++)
            {
                NPCs[i].transform.position = Vector3.zero;
            }
        }
    }
}
