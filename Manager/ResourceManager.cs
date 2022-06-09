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

            // 씬 정보 테이블로부터 읽어들인 데이터에 맞추어 리소스를 로드하여 배치
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
