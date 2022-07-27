using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SK.Location
{
    public enum Location 
    {
        HYDREA_Village,
        MESSENIA_Field,
        MYKONOS_Field,
        THERA_Field,
        SAMOS_Field
    }

    public class LocationManager : MonoBehaviour
    {
        [Header("Location Info UI")]
        [SerializeField] private Text text_Location;
        [SerializeField] private CanvasGroup group_EnterLocation;
        [SerializeField] private Text text_EnterLocation;
        [SerializeField] private float enterLocationShowDuration;

        [Header("Player Respawn")]
        public Vector3 respawnPoint;

        private LocationEntrance[] _locationEntrances;
        private Location _currentLocation;
        public Location CurrentLocation => _currentLocation;

        private readonly string _locationName_HYDREA_Village = "하이드리아 마을";
        private readonly string _locationName_MESSENIA_Field = "메세니아 지역";
        private readonly string _locationName_MYKONOS_Field = "미코노스 지역";
        private readonly string _locationName_THERA_Field = "테라 지역";
        private readonly string _locationName_SAMOS_Field = "사모스 지역";

        private bool _show;
        private float _canvasAlpha;
        private float _elapsed;

        private void Awake()
            => _locationEntrances = GetComponentsInChildren<LocationEntrance>();

        public void SetPlayerLocation(int locationIndex)
        {
            UpdatePlayerLocation((Location)locationIndex);

            for (int i = 0; i < _locationEntrances.Length; i++)
                _locationEntrances[i].FindPlayerLocation(_currentLocation);
        }

        public void UpdatePlayerLocation(Location location)
        {
            _currentLocation = location;

            // 위치 UI 업데이트
            switch (_currentLocation)
            {
                case Location.HYDREA_Village:
                    text_Location.text = _locationName_HYDREA_Village;
                    text_EnterLocation.text = _locationName_HYDREA_Village;
                    break;
                case Location.MESSENIA_Field:
                    text_Location.text = _locationName_MESSENIA_Field;
                    text_EnterLocation.text = _locationName_MESSENIA_Field;
                    break;
                case Location.MYKONOS_Field:
                    text_Location.text = _locationName_MYKONOS_Field;
                    text_EnterLocation.text = _locationName_MYKONOS_Field;
                    break;
                case Location.THERA_Field:
                    text_Location.text = _locationName_THERA_Field;
                    text_EnterLocation.text = _locationName_THERA_Field;
                    break;
                case Location.SAMOS_Field:
                    text_Location.text = _locationName_SAMOS_Field;
                    text_EnterLocation.text = _locationName_SAMOS_Field;
                    break;
            }

            // 지역 진입 안내 UI 표시
            _show = true;
            _canvasAlpha = 0;
            _elapsed = 0;
            SceneManager.Instance.OnUpdate += ShowInfoEnterLocation;
        }

        private void ShowInfoEnterLocation()
        {
            // 현재 진입 지역 안내 표시
            if (_show)
            {
                if (_canvasAlpha < 1)
                {
                    _canvasAlpha += Time.deltaTime;
                    if (_canvasAlpha >= 1) _canvasAlpha = 1;
                }
                else
                {
                    _elapsed += Time.deltaTime;
                    if (_elapsed >= enterLocationShowDuration)
                        _show = false;
                }
            }
            // 현재 진입 지역 안내 표시 사라짐
            else
            {
                _canvasAlpha -= Time.deltaTime;
                if (_canvasAlpha <= 0)
                {
                    _canvasAlpha = 0;
                    SceneManager.Instance.OnUpdate -= ShowInfoEnterLocation;
                }
            }
            group_EnterLocation.alpha = _canvasAlpha;
        }
    }
}