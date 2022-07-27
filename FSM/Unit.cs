using UnityEngine;
using UnityEngine.Events;
using SK.State;

namespace SK.FSM
{
    public abstract class Unit : MonoBehaviour
    {
        // 업데이트 이벤트
        public UnityAction OnUpdate;

        [Header("Component")]
        public Animator anim;
        public Behavior.Combat combat;
        public Health health;

        // 유닛 트랜스폼 캐싱
        public Transform mTransform { get; private set; }
        public Collider mCollider { get; private set; }

        // 시간 관련 변수
        internal float deltaTime, fixedDeltaTime;
        // 상태
        internal bool isDead, isInteracting, onUninterruptible;

        public virtual void Awake()
        {
            // 초기화
            mTransform = transform;
            mCollider = GetComponent<Collider>();
            if (!anim) anim = GetComponent<Animator>();
            if (!combat) combat = GetComponent<Behavior.Combat>();
            if (!health) health = GetComponent<Health>();
        }

        public virtual void OnEnable()
        {
            // 초기화
            isDead = false;
            isInteracting = false;
            onUninterruptible = false;

            // Event 등록
            health.OnDead += OnDead;
        }

        // 씬매니저를 통한 고정 업데이트로 호출될 함수
        public abstract void FixedTick();

        // 씬매니저를 통한 업데이트로 호출될 함수
        public abstract void Tick();

        // 유닛이 타격을 입으면 호출되는 가상함수
        public abstract void OnDamage(Unit attacker, uint damage, bool isStrong);

        // 오브젝트로부터 피해를 입으면 호출되는 가상함수
        public abstract void OnDamage(Transform damagableObject, uint damage, bool isStrong);

        // 유닛이 죽은 경우 호출되는 가상함수
        public abstract void OnDead();

        // 사운드 재생
        public void PlaySound(string audioKey)
            => AudioManager.Instance.PlayAudio(audioKey, mTransform);

        // 파티클 효과 재생
        public void PlayEffect(int effectId)
            => EffectManager.Instance.PlayEffect(effectId, mTransform);

        // 발걸음 소리 재생
        public abstract void FootStepSound();

        public virtual void OnDisable()
        {
            // event 해제
            health.OnDead -= OnDead;
        }
    }
}