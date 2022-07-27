using UnityEngine;

namespace SK.FX
{
    public class Effect : MonoBehaviour
    {
        public delegate void EndPlayHandler(int instancId);
        public event EndPlayHandler OnEndPlay;

        // 이펙트 고유 ID
        public int effectId;
        // 이펙트 재생 시간
        [SerializeField] private float playDuration;
        // 사용 트랜스폼의 회전 값 반영 여부
        [SerializeField] private bool dependRotation;
        // 사용 트랜스폼의 전방 회전 값 반영 여부
        [SerializeField] private bool dependForward;
        // 오프셋 회전 값 반영 여부
        [SerializeField] private bool applyOffset = true;
        // 오프셋 회전 값
        [SerializeField] private Quaternion offsetRotation = Quaternion.Euler(-90, 0, 0);
        // 이펙트 재생 위치 Y 값
        [SerializeField] private float offsetY;

        [Space]
        // 이펙트 재생 시 효과음 재생 여부
        [SerializeField] private bool playSoundEffect;
        // 재생할 효과음 ID
        [SerializeField] private string soundKey;

        // 캐싱
        private GameObject _gameObject;
        private Transform _transform;
        private ParticleSystem _particle;
        
        // 이펙트 인스턴스 ID
        private int _instancId;

        // 재생 시간
        private float _elapsed;

        public void Initialize()
        {
            _gameObject = gameObject;
            _transform = transform;
            _particle = GetComponent<ParticleSystem>();
            _instancId = GetInstanceID();

            _gameObject.SetActive(false);
        }

        // 이펙트 재생
        public void Play(Transform effectOwner)
        {
            if (playDuration == 0) return;

            // 재생 기준 트랜스폼의 회전 값을 반영
            if (dependRotation)
                _transform.SetPositionAndRotation(offsetY > 0 ? effectOwner.position + Vector3.up * offsetY : effectOwner.position,
                    applyOffset ? offsetRotation * effectOwner.rotation : effectOwner.rotation);
            else if (dependForward)
            {
                var forward = effectOwner.forward;
                forward.y = 0;

                _transform.SetPositionAndRotation(offsetY > 0 ? effectOwner.position + Vector3.up * offsetY : effectOwner.position,
                applyOffset ? offsetRotation * Quaternion.Euler(forward) : Quaternion.Euler(forward));
            }
            else
                _transform.SetPositionAndRotation(offsetY > 0 ? effectOwner.position + Vector3.up * offsetY : effectOwner.position, 
                    applyOffset ? offsetRotation : Quaternion.identity);

            _gameObject.SetActive(true);

            // 효과음 재생
            PlaySoundEffect();

            // 재생 중인지에 대한 모니터링 함수를 씬 매니저 업데이트에 추가
            SceneManager.Instance.OnFixedUpdate += PlayingMonitoring;
        }

        public void Play(ref Vector3 position, ref Quaternion rotation)
        {
            if (playDuration == 0) return;

            _transform.SetPositionAndRotation(position + Vector3.up * offsetY, rotation);

            _gameObject.SetActive(true);

            // 효과음 재생
            PlaySoundEffect();

            // 재생 중인지에 대한 모니터링 함수를 씬 매니저 업데이트에 추가
            SceneManager.Instance.OnFixedUpdate += PlayingMonitoring;
        }

        private void PlayingMonitoring()
        {
            if (_elapsed >= playDuration)
            {
                _elapsed = 0;
                PushToPool();
                SceneManager.Instance.OnFixedUpdate -= PlayingMonitoring;
            }
            _elapsed += Time.fixedDeltaTime;
        }

        // 재생 시간이 끝나면 다시 풀로 넣도록 이벤트 함수 호출
        private void PushToPool()
        {
            _gameObject.SetActive(false);
            OnEndPlay?.Invoke(_instancId);
        }

        private void PlaySoundEffect()
        {
            if (playSoundEffect)
                AudioManager.Instance.PlayAudio(soundKey, _transform);
        }

        private void OnDestroy()
            => OnEndPlay = null;
    }
}