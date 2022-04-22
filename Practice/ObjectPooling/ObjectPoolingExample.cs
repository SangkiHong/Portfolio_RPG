using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SK
{
    public class ObjectPoolingExample : MonoBehaviour
    {
        List<GameObject> objPoolList;

        private void Start()
        {
            objPoolList = new List<GameObject>();
        }

        public GameObject GetObject()
        {
            if (objPoolList.Count > 0)
            {
                foreach (var obj in objPoolList)
                {
                    if (!obj.activeSelf)
                        return obj;
                }
            }
            GameObject newObj = new GameObject();
            objPoolList.Add(newObj);
            return newObj;
        }
    }
}
