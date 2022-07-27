using System;
using System.Collections.Generic;
using UnityEngine;

public class UIPoolManager : MonoBehaviour
{
	readonly Dictionary<string, GameObject> _poolPrefabs = new Dictionary<string, GameObject>();
	readonly Dictionary<string, Queue<ObjectInstance>> _poolFreeDictionary = new Dictionary<string, Queue<ObjectInstance>>();
	readonly Dictionary<string, Dictionary<int, ObjectInstance>> _poolUsedDictionary = new Dictionary<string, Dictionary<int, ObjectInstance>>();

	static UIPoolManager _instance;

	public static UIPoolManager Instance
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
    private Transform uiCanvas;

    [Serializable]
    public struct PoolList
	{
        public int preLoadNum;
        public GameObject prefab;
    }

    public List<PoolList> poolLists;

    private void Awake()
    {	    
        for (int i = 0; i < poolLists.Count; i++)
        {
			CreatePool(poolLists[i].prefab.name, poolLists[i].prefab, poolLists[i].preLoadNum);
		}
    }
	public void CreatePool(string poolKey, GameObject prefab, int poolSeedSize)
	{
		if (!_poolPrefabs.ContainsKey(poolKey))
		{
			_poolPrefabs.Add(poolKey, prefab);
			if (!prefab.GetComponent<PoolObject>())
			{
				prefab.AddComponent<PoolObject>();
			}
			prefab.GetComponent<PoolObject>().poolKey = poolKey;

			_poolFreeDictionary.Add(poolKey, new Queue<ObjectInstance>());
			_poolUsedDictionary.Add(poolKey, new Dictionary<int, ObjectInstance>());

			for (int i = 0; i < poolSeedSize; i++)
			{
				ObjectInstance newObject = new ObjectInstance(Instantiate(prefab) as GameObject);
				_poolFreeDictionary[poolKey].Enqueue(newObject);
				newObject.SetParent(uiCanvas);
			}
		}
	}

	public GameObject GetObject(string poolKey, Vector3 position, Transform parent = null)
	{
		if (_poolPrefabs.ContainsKey(poolKey))
		{
			if (_poolFreeDictionary[poolKey].Count > 0)
			{
				ObjectInstance obj = _poolFreeDictionary[poolKey].Dequeue();
				_poolUsedDictionary[poolKey].Add(obj.GO.GetInstanceID(), obj);
				if (parent == null) obj.SetParent(uiCanvas); else obj.SetParent(parent);
				obj.Awake(position, obj.GO.transform.rotation);
				return obj.GO;
			}
			else
			{
				ObjectInstance newObject = new ObjectInstance(Instantiate(_poolPrefabs[poolKey]) as GameObject);
				_poolUsedDictionary[poolKey].Add(newObject.GO.GetInstanceID(), newObject);
				if (parent == null) newObject.SetParent(uiCanvas); else newObject.SetParent(parent);
				newObject.Awake(position, newObject.GO.transform.rotation);
				return newObject.GO;
			}
		}
		return null;
	}

	public void ReturnObjectToQueue(GameObject go, PoolObject poolObject)
	{
		if (poolObject)
		{
			string poolKey = poolObject.poolKey;
			var instanceID = go.GetInstanceID();
			ObjectInstance obj = _poolUsedDictionary[poolKey][instanceID];
			_poolUsedDictionary[poolKey].Remove(instanceID);
			_poolFreeDictionary[poolKey].Enqueue(obj);
			go.transform.SetParent(uiCanvas);
			go.SetActive(false);
		}
	}

	private class ObjectInstance
	{
		public readonly GameObject GO;
		readonly Transform _transform;
		readonly PoolObject _poolObjectScript;

		public ObjectInstance(GameObject objectInstance)
		{
			GO = objectInstance;
			_transform = GO.transform;
			GO.SetActive(false);
			_poolObjectScript = GO.GetComponent<PoolObject>();
		}

		public void Awake(Vector3 position, Quaternion rotation)
		{
			GO.SetActive(true);
			_transform.position = position;
			_transform.rotation = rotation;
			_poolObjectScript.OnAwake();
		}

		public void SetParent(Transform parent)
		{
			_transform.SetParent(parent);
		}
	}
}
