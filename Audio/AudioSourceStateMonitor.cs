using UnityEngine;

namespace SK
{
    public class AudioSourceStateMonitor : MonoBehaviour
    {
        private AudioSource _audioSource;

        private void Awake()
            => _audioSource = GetComponent<AudioSource>();

        private void OnEnable()
        {
            if (!_audioSource.loop) AudioManager.Instance.OnUpdate += Monitoring;
        }

        private void Monitoring()
        {
            if (_audioSource.clip != null && !_audioSource.isPlaying)
                gameObject.SetActive(false);
        }

        private void OnDisable()
        {
            if (!_audioSource.loop) AudioManager.Instance.OnUpdate -= Monitoring;
        }
    }
}