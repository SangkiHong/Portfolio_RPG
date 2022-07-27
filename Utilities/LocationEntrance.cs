using UnityEngine;

namespace SK.Location
{
    public class LocationEntrance : MonoBehaviour
    {
        [Header("trigger")]
        [SerializeField] private Location enterLocation;
        [SerializeField] private LocationEntrance linkedEntrance;

        [Header("BGM")]
        [SerializeField] private string BGMAudioKey;
        [SerializeField] private float crossPlayTime;

        private readonly string _tag_Player = "Player";

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(_tag_Player)) EnterLocation();
        }

        private void EnterLocation(bool updateLocation = true)
        {
            // 현재 위치 정보 전달
            if (updateLocation) 
                SceneManager.Instance.locationManager.UpdatePlayerLocation(enterLocation);

            // 배경 음악 재생
            if (BGMAudioKey != string.Empty)
                AudioManager.Instance.PlayBackGroundMusic(BGMAudioKey, crossPlayTime);

            // 반대편 입구 켜짐
            linkedEntrance.gameObject.SetActive(true);
            // 현재 입구 꺼짐
            gameObject.SetActive(false);
        }

        public void FindPlayerLocation(Location playerLocation)
        {
            if (playerLocation == enterLocation)
                EnterLocation(false);
        }

        /*private void OnDisable()
            => SceneManager.Instance.OnFixedUpdate -= FixedTick;*/
    }
}