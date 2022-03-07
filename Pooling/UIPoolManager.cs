using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPoolManager : MonoBehaviour
{
	Dictionary<string, GameObject> poolPrefabs = new Dictionary<string, GameObject>();
	Dictionary<string, GameObject> poolParents = new Dictionary<string, GameObject>();
	Dictionary<string, Queue<ObjectInstance>> poolFreeDictionary = new Dictionary<string, Queue<ObjectInstance>>();
	Dictionary<string, Dictionary<int, ObjectInstance>> poolUsedDictionary = new Dictionary<string, Dictionary<int, ObjectInstance>>();

	static UIPoolManager _instance;

	public static UIPoolManager instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = FindObjectOfType<UIPoolManager>();
			}
			return _instance;
		}
	}

	[SerializeField]
    private Transform UICanvas;

    [Serializable]
    public struct poolList
	{
        public int preLoadNum;
        public GameObject prefab;
    }

    public List<poolList> poolLists;

    private void Awake()
    {
        for (int i = 0; i < poolLists.Count; i++)
        {
			CreatePool(poolLists[i].prefab.name, poolLists[i].prefab, poolLists[i].preLoadNum);
		}
    }
	public void CreatePool(string poolKey, GameObject prefab, int poolSeedSize)
	{
		if (!poolPrefabs.ContainsKey(poolKey))
		{
			poolPrefabs.Add(poolKey, prefab);
			if (!prefab.GetComponent<PoolObject>())
			{
				prefab.AddComponent<PoolObject>();
			}
			prefab.GetComponent<PoolObject>().poolKey = poolKey;

			poolFreeDictionary.Add(poolKey, new Queue<ObjectInstance>());
			poolUsedDictionary.Add(poolKey, new Dictionary<int, ObjectInstance>());

			for (int i = 0; i < poolSeedSize; i++)
			{
				ObjectInstance newObject = new ObjectInstance(Instantiate(prefab) as GameObject);
				poolFreeDictionary[poolKey].Enqueue(newObject);
				newObject.SetParent(UICanvas);
			}
		}
	}

	public GameObject GetObject(string poolKey, Vector3 position)
	{
		if (poolPrefabs.ContainsKey(poolKey))
		{
			if (poolFreeDictionary[poolKey].Count > 0)
			{
				ObjectInstance obj = poolFreeDictionary[poolKey].Dequeue();
				poolUsedDictionary[poolKey].Add(obj.gameObject.GetInstanceID(), obj);
				obj.Awake(position, obj.gameObject.transform.rotation);
				return obj.gameObject;
			}
			else
			{
				ObjectInstance newObject = new ObjectInstance(Instantiate(poolPrefabs[poolKey]) as GameObject);
				poolUsedDictionary[poolKey].Add(newObject.gameObject.GetInstanceID(), newObject);
				newObject.SetParent(UICanvas);
				newObject.Awake(position, newObject.gameObject.transform.rotation);
				return newObject.gameObject;
			}
		}
		return null;
	}

	public void ReturnObjectToQueue(GameObject gameObject, PoolObject poolObject)
	{
		if (poolObject)
		{
			string poolKey = poolObject.poolKey;
			gameObject.SetActive(false);
			ObjectInstance obj = poolUsedDictionary[poolKey][gameObject.GetInstanceID()];
			poolUsedDictionary[poolKey].Remove(gameObject.GetInstanceID());
			poolFreeDictionary[poolKey].Enqueue(obj);
		}
	}

	private class ObjectInstance
	{
		public GameObject gameObject;
		Transform transform;
		PoolObject poolObjectScript;

		public ObjectInstance(GameObject objectInstance)
		{
			gameObject = objectInstance;
			transform = gameObject.transform;
			gameObject.SetActive(false);
			poolObjectScript = gameObject.GetComponent<PoolObject>();
		}

		public void Awake(Vector3 position, Quaternion rotation)
		{
			gameObject.SetActive(true);
			transform.position = position;
			transform.rotation = rotation;
			poolObjectScript.OnAwake();
		}

		public void SetParent(Transform parent)
		{
			transform.parent = parent;
		}
	}
}
