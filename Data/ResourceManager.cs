using System.IO;
using UnityEngine;

namespace SK
{
    public class ResourceManager : MonoBehaviour
    {
        [System.Serializable]
        public struct ResourceList
        {
            public bool onLoad;
            public string dataName;
            public string prefabFolderName;
            public Transform parent;
        }

        public static ResourceManager Instance { get; private set; }

        public ResourceList[] resourceList;

        private readonly string _enemyTag = "Enemy";

        void Awake()
        {
            Instance = this;
            if (resourceList != null && resourceList.Length > 0)
            {
                foreach (var item in resourceList)
                {
                    if (item.onLoad)
                        LoadResource(item.dataName, item.prefabFolderName, item.parent);
                }
            }
        }

        private GameObject LoadResource(string dataName, string prefabFolderName, Transform parentTransform = null)
        {
            string path = Path.Combine(Application.dataPath, Path.Combine("Resources", "Data", dataName)) + ".csv";

            using (StreamReader sr = new StreamReader(path))
            {
                string line = string.Empty;
                int lineNum = 0;
                GameObject obj = null;
                while ((line = sr.ReadLine()) != null)
                {
                    string[] row = line.Split(new char[] { ',' }); // ,¥‹¿ß∑Œ Split
                    if (row[0] == "Index") continue;

                    obj = Resources.Load(Path.Combine(prefabFolderName, row[1])) as GameObject;
                    obj = parentTransform != null ? Instantiate(obj, parentTransform) : Instantiate(obj);
                    obj.transform.position = new Vector3(float.Parse(row[2]), float.Parse(row[3]), float.Parse(row[4]));
                    obj.transform.rotation = Quaternion.Euler(float.Parse(row[5]), float.Parse(row[6]), float.Parse(row[7]));
                    obj.transform.localScale = new Vector3(float.Parse(row[8]), float.Parse(row[9]), float.Parse(row[10]));

                    // Enemy Information
                    if (obj.CompareTag(_enemyTag))
                        obj.GetComponent<Enemy>().isPatrol = bool.Parse(row[11]);
                }
                sr.Close();
                return obj;
            }
        }

        public GameObject LoadPlayerCharacter()
        {
            return LoadResource("PlayerData", "Player");
        }
    }
}
