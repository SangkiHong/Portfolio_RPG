using UnityEngine;
using UnityEngine.Events;
using System.Collections;

namespace SK
{
    public sealed class Health : MonoBehaviour, IDamagable
    {
        public UnityAction onDamaged;
        public UnityAction onDead;

        public int CurrentHp => _currentHp;
        private int _currentHp;
        [SerializeField] private bool canHitFx;
        [SerializeField] private bool canDeadFx;
        [SerializeField] private float hitColorDuration = 0.8f;
        [SerializeField] private float deadFxDuration = 1.5f;
        [SerializeField] private int bonusHpPerLevel;
        [SerializeField] private int bonusHpPerSTR;
        [SerializeField] private int bonusHpPerDEX;
        [SerializeField] private int bonusHpPerINT;

        [System.NonSerialized] public bool canDamage = true;
        [System.NonSerialized] public Transform hitTransform;

        private Transform _transform;
        private Material material;
        private Color _hitEmision = new Color(1, 0, 0, 0);
        private Color _currentEmision;
        private WaitForSeconds ws;
        private readonly string _emissionColor = "_EmissionColor"; 
        private readonly string _cutoffHeight = "_CutoffHeight";
        private bool _isCriticalHit, _isChangedColor;
        private float _timer;
        private int _damageValue;

        private void Awake()
        {
            _transform = transform;

            if (canHitFx || canDeadFx)
            {
                // Materials Instancing
                SkinnedMeshRenderer[] skinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
                material = skinnedMeshRenderers[0].material;
                material.EnableKeyword("_EMISSION");
                material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
                foreach (var renderer in skinnedMeshRenderers)
                    renderer.material = material;

                ws = new WaitForSeconds(0.05f);
            }
        }

        public void Init(int maxHp) => _currentHp = maxHp;
        public void Init(int level, int s, int d, int i)
        {
            _currentHp = (level * bonusHpPerLevel)
                      + ((s - 1) * bonusHpPerSTR)
                      + ((d - 1) * bonusHpPerDEX)
                      + ((i - 1) * bonusHpPerINT);
        }

        public void OnDamage(int damageValue, Transform tr, bool isCriticalHit)
        {
            if (!canDamage) return;

            _damageValue = damageValue;
            hitTransform = tr;
            _isCriticalHit = isCriticalHit;

            if (canHitFx)
            {
                _timer = hitColorDuration;
                material.SetColor(_emissionColor, _hitEmision);

                if (!_isChangedColor)
                    StartCoroutine(HitColorChange());
            }

            onDamaged?.Invoke();
        }

        // 타격 입을 지에 대한 판정에 따라 호출됨
        public void Damaged()
        {
            _currentHp += _damageValue;
            UIPoolManager.Instance.GetObject(Strings.PoolName_DamagePoint, Vector3.zero).GetComponent<DamageUI>().
                Assign(_transform.position, _damageValue, _isCriticalHit); // Pop UI PoolObject

            if (CurrentHp <= 0) 
            {
                onDead?.Invoke(); 
                hitTransform = null;
            }
        }

        public void PlayDeadFx() => StartCoroutine(DeadFx());

        IEnumerator HitColorChange()
        {
            _isChangedColor = true;
            var changeTime = hitColorDuration * 0.5f;

            while (_timer > 0)
            {
                _timer -= 0.05f;
                if (_timer <= changeTime && _timer <= 1)
                {
                    _currentEmision.r = _timer;
                    material.SetColor(_emissionColor, _currentEmision);
                }
                yield return ws; 
            }

            _isChangedColor = false;
        }

        IEnumerator DeadFx()
        {
            _timer = deadFxDuration + 1.5f;
            while (_timer > -1)
            {
                _timer -= 0.05f;
                if (_timer <= 1.5f)
                {
                    material.SetFloat(_cutoffHeight, _timer);
                }
                yield return ws;
            }
            gameObject.SetActive(false);
        }
    }
}
