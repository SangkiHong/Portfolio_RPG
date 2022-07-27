using UnityEngine;
using UnityEngine.UI;

namespace SK.UI
{
    public class EnemyHpStateUIHandler : MonoBehaviour
    {
        [SerializeField] private GameObject hpBarObject;
        [SerializeField] private Text enemyName;
        [SerializeField] private Slider enemyHpSlider;
        [SerializeField] private float showDuration = 5; // 최대 UI 표시 시간

        private Enemy _targetEnemy;
        private State.Health _targetHealth;
        private uint _currentHp, _updateHp;
        private float _elapsed;

        // 적 HP 표시
        public void ShowEnemyHp(Enemy enemy)
        {
            _elapsed = 0;

            // 이미 할당된 적인 경우 할당 취소
            if (_targetEnemy != null && _targetEnemy == enemy) return;

            _targetEnemy = enemy;
            _targetHealth = _targetEnemy.health;
            _currentHp = _targetHealth.CurrentHp;

            // 적의 Hp가 0이 된 경우 꺼짐
            if (_currentHp == 0)
            {
                Hide();
                return;
            }

            // UI 표시
            if (!hpBarObject.activeSelf) hpBarObject.SetActive(true);
            // 적 이름 표시
            enemyName.text = _targetEnemy.enemyData.DisplayName.ToString();
            // 적 최대 Hp 할당
            enemyHpSlider.maxValue = _targetEnemy.health.MaxHp;
            // 현재 Hp 할당
            enemyHpSlider.value = _currentHp;
            // 적 Hp 업데이트
            UpdateHp();
        }

        // UI 표시 숨김
        public void Hide()
        {
            if (hpBarObject.activeSelf)
            {
                _targetEnemy = null;
                _targetHealth = null;
                hpBarObject.SetActive(false);
            }
        }

        private void UpdateHp()
        {
            _updateHp = _targetHealth.CurrentHp;
            if (_currentHp != _updateHp)
            {
                _currentHp = _updateHp;
                enemyHpSlider.value = _currentHp;
            }
        }

        // 표시 타이머 업데이트
        private void FixedUpdate()
        {
            if (!_targetHealth) return;

            _elapsed += Time.fixedDeltaTime;

            // 적 Hp 업데이트
            UpdateHp();

            if (_elapsed >= showDuration)
                Hide();
        }
    }
}