using UnityEngine;

namespace SK
{
    public class HitFX : MonoBehaviour
    {
        [SerializeField] private bool canHitFx;
        [SerializeField] private bool canDeadFx;
        [SerializeField] private bool disappearObject = true;

        [SerializeField] private float hitColorDuration = 0.8f;
        [SerializeField] private float deadFxWaitTime = 1.5f;
        [SerializeField] private float deadFxSpeed = 0.5f;

        private State.Health _health;
        private Material _material;
        private Color _hitEmision = new Color(1, 0, 0, 0);
        private Color _currentEmision;

        private readonly string _string_IsDead = "_IsDead";
        private readonly string _string_EmissionColor = "_EmissionColor";
        private readonly string _string_CutoffHeight = "_CutoffHeight";

        private bool _isDamaged, _isDead;
        private float _cutoffHeight;
        private float _timer;

        private void Awake()
        {
            _health = GetComponent<State.Health>();
            _health.OnDamaged += Damaged;
            _health.OnDead += Dead;

            if (canHitFx || canDeadFx)
            {
                // Materials Instancing
                SkinnedMeshRenderer[] skinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
                _material = skinnedMeshRenderers[0].material;
                _material.EnableKeyword("_EMISSION");
                _material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
                foreach (var renderer in skinnedMeshRenderers)
                    renderer.material = _material;
            }
        }

        private void OnEnable()
        {
            _isDamaged = false;
            _isDead = false;

            // 머터리얼 설정
            _currentEmision.r = 0;
            _material.SetColor(_string_EmissionColor, _currentEmision);
            _material.SetFloat(_string_CutoffHeight, 1);
            _material.SetInt(_string_IsDead, 0);
        }

        // 필요한 경우 유닛 업데이트 이벤트에 등록하여 호출될 업데이트 함수
        private void Tick()
        {
            if (_isDamaged)
            {
                if (_timer < hitColorDuration)
                {
                    _timer += Time.deltaTime;
                    _currentEmision.r = Mathf.Lerp(1, 0, _timer / hitColorDuration);
                    _material.SetColor(_string_EmissionColor, _currentEmision);
                }
                else
                {
                    _isDamaged = false;
                    // 유닛의 업데이트 이벤트 함수에서 해제
                    SceneManager.Instance.OnUpdate -= Tick;
                }
            }

            if (_isDead)
            {
                _timer -= deadFxSpeed * Time.deltaTime;

                if (_timer > -0.5f)
                {
                    if (canDeadFx && _timer < 1)
                    {
                        _cutoffHeight = _timer;
                        _material.SetFloat(_string_CutoffHeight, _cutoffHeight);
                    }
                }
                else
                {
                    _isDead = false;
                    SceneManager.Instance.OnUpdate -= Tick;
                    if (disappearObject) gameObject.SetActive(false);
                }
            }
        }

        private void Damaged(uint currentHp)
        {
            // 피격 시 Emission 효과 발동
            if (canHitFx)
            {
                _isDamaged = true;
                _material.SetColor(_string_EmissionColor, _hitEmision);

                _timer = 0;
                // 유닛의 업데이트 이벤트 함수에 등록
                SceneManager.Instance.OnUpdate += Tick;
            }
        }

        private void Dead()
        {
            _isDead = true;
            _timer = 1 + deadFxWaitTime;
            // 머터리얼 변수 변경
            _material.SetInt(_string_IsDead, 1);
            // 유닛의 업데이트 이벤트 함수에 등록
            SceneManager.Instance.OnUpdate += Tick;
        }
    }
}