using UnityEngine;

namespace SK
{
    public class DamageRangeHandler : MonoBehaviour
    {
        [SerializeField] Transform damageRangeDecal;

        private Vector3 _rangeScale;

        private void Awake()
            => _rangeScale = damageRangeDecal.localScale;

        public void ShowRange(Vector3 position, float range)
        {
            damageRangeDecal.position = position;
            _rangeScale.x = range;
            _rangeScale.z = range;
            damageRangeDecal.localScale = _rangeScale;
            damageRangeDecal.gameObject.SetActive(true);
        }
        
        public void Hide()
            => damageRangeDecal.gameObject.SetActive(false);
    }
}
