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

        [SerializeField] private GameObject tempManagers;

        public ResourceList[] resourceList;

        void Awake()
        {
            Instance = this;

            if (GameManager.Instance == null)
                Instantiate(tempManagers);

            GameManager.Instance.SetMainScene();

            // �� ���� ���̺�κ��� �о���� �����Ϳ� ���߾� ���ҽ��� �ε��Ͽ� ��ġ
            if (resourceList != null && resourceList.Length > 0)
            {
                foreach (var resource in resourceList)
                {
                    if (resource.onLoad)
                        GameManager.Instance.DataManager.LoadResource(resource.dataName, resource.prefabFolderName, resource.parent);
                }
            }
        }

        private void Start()
        {
            GameManager.Instance.DataManager.InstantiatePlayer();
        }
    }
}
