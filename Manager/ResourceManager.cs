using System.Collections;
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

        // ���߿� �ӽ� ���ӸŴ��� ������
        [SerializeField] private GameObject tempManagers;

        public ResourceList[] resourceList;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(this);
            }
            else
                Destroy(gameObject);

            if (GameManager.Instance == null)
                Instantiate(tempManagers);

            GameManager.Instance.SetMainScene();

            StartCoroutine(LoadResources());
        }

        IEnumerator LoadResources()
        {
            WaitForEndOfFrame ws = new WaitForEndOfFrame();

            // ���Ŵ����� ���� ������ ������ ��� ����
            while (SceneManager.Instance == null)
                yield return ws;

            // �� ���� ���̺�κ��� �о���� �����Ϳ� ���߾� ���ҽ��� �ε��Ͽ� ��ġ
            if (resourceList != null && resourceList.Length > 0)
            {
                foreach (var resource in resourceList)
                {
                    if (resource.onLoad)
                        GameManager.Instance.DataManager.LoadResource(resource.dataName, resource.prefabFolderName, resource.parent);
                }
            }
            yield return null;
        }

        private void Start()
        {
            GameManager.Instance.DataManager.InstantiatePlayer();
        }
    }
}
