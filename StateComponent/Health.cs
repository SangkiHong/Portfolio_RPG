using UnityEngine;
using UnityEngine.Events;
using System.Collections;

namespace SK
{
    public sealed class Health : MonoBehaviour
    {
        public UnityAction onDead;

        [SerializeField] private bool canHitFx;
        [SerializeField] private bool canDeadFx;
        [SerializeField] private float hitColorDuration = 0.8f;
        [SerializeField] private float deadFxDuration = 1.5f;
        [SerializeField] private uint bonusHpPerLevel;
        [SerializeField] private uint bonusHpPerSTR;
        [SerializeField] private uint bonusHpPerDEX;
        [SerializeField] private uint bonusHpPerINT;

        private FSM.Unit _unit;
        private Transform _transform;
        private Material material;
        private Color _hitEmision = new Color(1, 0, 0, 0);
        private Color _currentEmision;
        private WaitForSeconds ws;

        private readonly string _emissionColor = "_EmissionColor"; 
        private readonly string _cutoffHeight = "_CutoffHeight";

        private bool _canDamage = true;
        private uint _currentHp;
        private float _timer;

        public uint CurrentHp => _currentHp;
        public bool CanDamage { set { _canDamage = value; } }

        public void Init(Transform tr, FSM.Unit unit)
        {
            _unit = unit;
            _transform = tr;

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

        private void Tick()
        {
            if (_timer < hitColorDuration)
            {
                _timer += Time.deltaTime;
                _currentEmision.r = Mathf.Lerp(1, 0, _timer / hitColorDuration);
                material.SetColor(_emissionColor, _currentEmision);
            }
            else
                // 유닛의 업데이트 이벤트 함수에서 해제
                _unit.OnUpdate -= Tick;
        }

        public void Init(uint maxHp) => _currentHp = maxHp;

        public void Init(uint level, uint s, uint d, uint i)
        {
            _currentHp = (level * bonusHpPerLevel)
                      + ((s - 1) * bonusHpPerSTR)
                      + ((d - 1) * bonusHpPerDEX)
                      + ((i - 1) * bonusHpPerINT);
        }

        // 타격 입을 지에 대한 판정에 따라 호출됨
        public void OnDamage(uint damage, bool isCritical)
        {
            _currentHp -= damage;

            // 데미지 수치 UI 표시
            UIPoolManager.Instance.GetObject(Strings.PoolName_DamagePoint, Vector3.zero).GetComponent<DamageUI>().
                Assign(_transform.position, damage, isCritical);

            // 피격 시 Emission 효과 발동
            if (canHitFx)
            {
                material.SetColor(_emissionColor, _hitEmision);

                _timer = 0;
                // 유닛의 업데이트 이벤트 함수에 등록
                _unit.OnUpdate += Tick;
            }

            // HP가 0이 되었을 경우 죽음
            if (CurrentHp <= 0) 
            {
                onDead?.Invoke();
            }
        }

        public void PlayDeadFx() => StartCoroutine(DeadFx());

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
