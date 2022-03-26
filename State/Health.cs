using UnityEngine;
using UnityEngine.Events;

namespace SK
{
    public sealed class Health : MonoBehaviour, IDamagable
    {
        public UnityAction onDamaged;
        public UnityAction onDead;

        public bool canDamage = true;
        //public bool isDamaged; // Blink System::deprecated

        public int currentHp;
        //[SerializeField] private float blinkTime = 0.5f; // Blink System::deprecated
        [System.NonSerialized] public Transform hitTransform;
        
        private Transform _transform;
        private bool _isCriticalHit;
        //private float _timer; // Blink System::deprecated
        private int _damageValue;

        private void Awake()
        {
            _transform = transform;
        }

        // Blink System::deprecated
        /*private void FixedUpdate()
        {
            if (isDamaged)
            {
                _timer += Time.fixedDeltaTime;
                if (_timer >= blinkTime) 
                    isDamaged = false;
            }
        }*/

        public void OnDamage(int damageValue, Transform tr, bool isCriticalHit)
        {
            //if (isDamaged) return; // Blink System::deprecated
            if (!canDamage) return;

            _damageValue = damageValue;
            hitTransform = tr;
            _isCriticalHit = isCriticalHit;

            onDamaged?.Invoke();
        }

        // 타격 입을 지에 대한 판정에 따라 호출됨
        public void Damaged()
        {
            //isDamaged = true; // Blink System::deprecated
            //_timer = 0;
            currentHp += _damageValue;
            UIPoolManager.Instance.GetObject(Strings.PoolName_DamagePoint, Vector3.zero).GetComponent<DamageUI>().
                Assign(_transform.position, _damageValue, _isCriticalHit); // Pop UI PoolObject

            if (currentHp <= 0) 
            {
                onDead?.Invoke(); 
                hitTransform = null;
            }
        }
    }
}
