using UnityEngine;

namespace SK.Utilities
{
    public class HideTiles : MonoBehaviour
    {
        [SerializeField]
        private string tileTag;
        [SerializeField]
        private Vector3 tileSize;
        [SerializeField]
        private int maxDistance;
        
        private GameObject[] _tiles;
        private Transform _transform;
        
        // Use this for initialization
        void Start () 
        {
            _tiles = GameObject.FindGameObjectsWithTag (tileTag);
            _transform = transform;
            
            DeactivateDistantTiles ();
        }
        
        void DeactivateDistantTiles() 
        {
            Vector3 playerPosition = _transform.position;
            
            foreach (GameObject tile in _tiles) {
                Vector3 tilePosition = tile.gameObject.transform.position + (tileSize / 2f);
                
                float xDistance = Mathf.Abs(tilePosition.x - playerPosition.x);
                float zDistance = Mathf.Abs(tilePosition.z - playerPosition.z);
                
                if (xDistance + zDistance > maxDistance) {
                    tile.SetActive (false);
                } else {
                    tile.SetActive (true);
                }
            }
        }
        
        void Update () => DeactivateDistantTiles ();
    }
}
