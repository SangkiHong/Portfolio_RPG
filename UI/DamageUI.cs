using UnityEngine;
using UnityEngine.UI;

namespace SK
{
    public class DamageUI : PoolObject
    {
        [SerializeField] private float offsetY;
        [SerializeField] private float randomPosValue = 0.5f;
        [SerializeField] private float floatingTime;
        [SerializeField] private float floatingHeight;
        [SerializeField] private Text textDamage;
        [SerializeField] private Color defaultColor;
        [SerializeField] private Color criticalColor;

        private Camera _camera;
        private Transform _transform;
        private Vector3 _anchoredPosition, _tempPos;
        private float _timers;

        private void Awake()
        {
            _transform = transform;
            _tempPos.y = offsetY;
        }

        private void OnEnable()
        {
            if (!_camera) _camera = Camera.main;
        }

        private void FixedTick()
        {
            // Updating UI Position
            _transform.position = _camera.WorldToScreenPoint(_anchoredPosition);

            FloatingText();
        }

        public void Assign(Vector3 position, uint damageValue, bool isCriticalHit)
        {
            SceneManager.Instance.OnFixedUpdate += FixedTick;

            // Set UI Position
            _tempPos.x = Random.Range(-randomPosValue, randomPosValue + 0.1f);
            _tempPos.z = Random.Range(-randomPosValue, randomPosValue + 0.1f);
            _anchoredPosition = position + _tempPos;
            
            SetUI(damageValue, isCriticalHit);
        }

        private void SetUI(uint value, bool isCriticalHit)
        {
            _timers = 0;
            textDamage.text = value.ToString();
            textDamage.color = isCriticalHit ? criticalColor : defaultColor;
            textDamage.gameObject.SetActive(true);
        }

        private void FloatingText()
        {
            if (textDamage.gameObject.activeInHierarchy)
            {
                _timers += Time.fixedDeltaTime;
                _transform.localPosition += Vector3.up * EasingFunction.EaseOutQuart(0, floatingHeight, _timers / floatingTime);

                // Text off
                if (_timers >= floatingTime)
                {
                    SceneManager.Instance.OnFixedUpdate -= FixedTick;
                    Done(true);
                }
            }
        }
    }
}
