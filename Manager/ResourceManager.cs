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

        // 개발용 임시 게임매니저 프리펩
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

            // 씬매니저에 접근 가능할 때까지 대기 상태
            while (SceneManager.Instance == null)
                yield return ws;

            // 씬 정보 테이블로부터 읽어들인 데이터에 맞추어 리소스를 로드하여 배치
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
