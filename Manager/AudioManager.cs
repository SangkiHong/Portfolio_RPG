using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using SK.Utilities;

namespace SK
{
    /* 작성자: 홍상기
     * 내용: 오디오(배경음, 효과음)에 관련된 기능을 가진 매니저 클래스
     * 작성일: 22년 6월 21일
     */

    // 사운드 풀링 소스 구조체
    [System.Serializable]
    public struct AudioPoolList
    {
        public int seedSize;
        public AudioClip audioClip;
        public AudioMixerGroup audioMixer;
        [Range(0.0f, 1.0f)] public float volume; // 재생 볼륨(0~1)
        public float maxPlayDistance; // 효과음 재생 가능 최대 거리(0인 경우 2D로 재생)
    }

    public class AudioManager : MonoBehaviour
    {
        private static AudioManager _instance;
        public static AudioManager Instance => _instance;

        public delegate void UpdateHandler();
        public event UpdateHandler OnUpdate;

        // 배경음악 믹서
        [SerializeField] private AudioMixerGroup BGMAudioMixer;
        // 오디오 소스 프리팹
        [SerializeField] private GameObject audioSourcePrefab;
        // 오디오 풀 데이터
        public Data.AudioData audioData;
        // 오디오 소스 배열을 저장할 딕셔너리
        private Dictionary<string, AudioSource[]> _audioSourceDictionary;

        private AudioSource _currentBGMAudioSource, _prevBGMAudioSource;
        private Transform _playerTransform;
        private float _crossTime, _crossElapsed;

        private void Awake()
        {
            if (_instance == null)
                _instance = this;
            else
            {
                Destroy(gameObject);
                return;
            }

            // 초기화
            Initialize();
        }

        private void Update()
            => OnUpdate?.Invoke();

        private void Initialize()
        {
            // 딕셔너리 초기화
            _audioSourceDictionary = new Dictionary<string, AudioSource[]>();

            GameObject sourceParentObject;
            GameObject sourceChildObject;
            AudioSource tmpSource;

            foreach (KeyValuePair<string, AudioPoolList> pool in audioData.audioDictionary)
            {
                // 풀 이름에 따라 부모 오브젝트 생성
                sourceParentObject = new GameObject(pool.Key);
                sourceParentObject.transform.SetParent(transform);
                int seedSize = pool.Value.seedSize;

                // 오디오 소스 딕셔너리의 배열 초기화
                AudioSource[] audioSources = new AudioSource[seedSize];
                _audioSourceDictionary.Add(pool.Key, audioSources);
                // 트랜스폼 캐싱 
                Transform tmpTr = sourceParentObject.transform;

                for (int i = 0; i < seedSize; i++)
                {
                    // 오디오 소스 컴포넌트를 가진 오브젝트 생성 
                    sourceChildObject = Instantiate(audioSourcePrefab, tmpTr);
                    tmpSource = sourceChildObject.GetComponent<AudioSource>();
                    tmpSource.clip = pool.Value.audioClip;
                    tmpSource.outputAudioMixerGroup = pool.Value.audioMixer;
                    tmpSource.volume = pool.Value.volume;
                    tmpSource.playOnAwake = false;
                    // BGM인 경우 Loop 설정
                    if (tmpSource.outputAudioMixerGroup == BGMAudioMixer)
                        tmpSource.loop = true;
                    // 3D 사운드 설정
                    if (pool.Value.maxPlayDistance > 0)
                    {
                        tmpSource.spatialBlend = 1;
                        tmpSource.maxDistance = pool.Value.maxPlayDistance;
                    }

                    // 컴포넌트를 딕셔너리에 추가
                    _audioSourceDictionary[pool.Key][i] = tmpSource;
                    tmpSource.gameObject.SetActive(false);
                }
            }
            Debug.Log("오디오 풀 준비 완료");
        }

        // 스트링 키를 가지고 딕셔너리에 접근하여 해당 오디오 소스를 재생
        public void PlayAudio(string audioKey, Transform playTransform)
        {
            if (_playerTransform == null && GameManager.Instance.Player)
                _playerTransform = GameManager.Instance.Player.mTransform;

            for (int i = 0; i < _audioSourceDictionary[audioKey].Length; i++)
            {
                // 재생되지 않은 오디오소스를 탐색
                if (!_audioSourceDictionary[audioKey][i].gameObject.activeSelf)
                {
                    // 3D 재생인 경우
                    if (_audioSourceDictionary[audioKey][i].spatialBlend == 1)
                    {
                        float maxDistance = _audioSourceDictionary[audioKey][i].maxDistance;
                        // 최적화를 위해 제곱하여 플레이어와의 거리를 비교
                        maxDistance *= maxDistance;
                        float playerDistance = MyMath.Instance.GetDistance(playTransform.position, _playerTransform.position);

                        // 효과음의 최대 거리가 플레이어와의 거리보다 먼 경우 재생하지 않음
                        if (playerDistance > maxDistance) return;
                    }

                    _audioSourceDictionary[audioKey][i].gameObject.SetActive(true);
                    _audioSourceDictionary[audioKey][i].transform.position = playTransform.position;
                    _audioSourceDictionary[audioKey][i].Play();
                    return;
                }
            }
        }

        // 스트링 키를 가지고 딕셔너리에 접근하여 해당 오디오 소스를 재생
        public void PlayAudio(string audioKey)
        {
            if (_playerTransform == null && GameManager.Instance.Player)
                _playerTransform = GameManager.Instance.Player.mTransform;

            for (int i = 0; i < _audioSourceDictionary[audioKey].Length; i++)
            {
                if (!_audioSourceDictionary[audioKey][i].isPlaying)
                {
                    if (_playerTransform)
                        _audioSourceDictionary[audioKey][i].transform.position = _playerTransform.position;

                    _audioSourceDictionary[audioKey][i].gameObject.SetActive(true);
                    _audioSourceDictionary[audioKey][i].Play();
                    return;
                }
            }
        }

        // 페이드 인 아웃으로 배경음악 플레이(크로스 플레이)
        public void PlayBackGroundMusic(string audioKey, float crossPlayTime)
        {
            // 키에 해당하는 오디오소스가 없거나 현재 재생 중인 배경음악과 동일한 키가 전달되었다면 리턴
            if (!_audioSourceDictionary.ContainsKey(audioKey) || 
                _currentBGMAudioSource == _audioSourceDictionary[audioKey][0])
                return;

            // 현재 재생중인 배경음악 오디오소스를 이전 배경음악 변수로 교체
            if (_currentBGMAudioSource)
                _prevBGMAudioSource = _currentBGMAudioSource;

            // 새로운 배경음악 소스를 딕셔너리에서 가져옴
            _currentBGMAudioSource = _audioSourceDictionary[audioKey][0];
            _currentBGMAudioSource.gameObject.SetActive(true);

            // 씬 매니저가 있는 경우
            if (SceneManager.Instance)
            {
                // 새로운 배경음악의 음량 초기화
                _currentBGMAudioSource.volume = 0;
                // 변수 초기화
                _crossTime = crossPlayTime;
                _crossElapsed = 0;
                // 씬 매니저의 업데이트 이벤트에 함수 등록
                SceneManager.Instance.OnUpdate += CrossPlay;
            }

            // 새로운 배경음악 재생
            _currentBGMAudioSource.Play();
        }

        // 크로스 플레이
        private void CrossPlay()
        {
            _crossElapsed += Time.deltaTime;

            // 크로스 중
            if (_crossElapsed < _crossTime)
            {
                // 새로운 배경음의 음량을 서서히 올려줌
                _currentBGMAudioSource.volume = _crossElapsed / _crossTime;
                // 이전 배경음의 음량을 서서히 낮춤
                if (_prevBGMAudioSource)
                    _prevBGMAudioSource.volume = _crossTime - _crossElapsed;
            }
            // 크로스 완료
            else
            {
                _currentBGMAudioSource.volume = 1;

                if (_prevBGMAudioSource)
                {
                    _prevBGMAudioSource.volume = 0;
                    _prevBGMAudioSource.Stop();
                    _prevBGMAudioSource.gameObject.SetActive(true);
                    _prevBGMAudioSource = null;
                }
                SceneManager.Instance.OnUpdate -= CrossPlay;
            }
        }
    }
}
