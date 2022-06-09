using UnityEngine;
using UnityEngine.Events;

namespace SK.FSM
{
    public class Unit : MonoBehaviour
    {
        // 업데이트 이벤트
        public UnityAction OnUpdate;

        // 컴포넌트
        public Animator anim;
        public Behavior.Combat combat;
        public Health health;

        // 유닛 트랜스폼 캐싱
        internal Transform mTransform;

        // 시간 관련 변수
        internal float deltaTime, fixedDeltaTime;

        public virtual void Awake()
        {
            // 초기화
            mTransform = transform;
            if (!anim) anim = GetComponent<Animator>();
            if (!combat) combat = GetComponent<Behavior.Combat>();
            if (!health) health = GetComponent<Health>();
            health.Init(mTransform, this);
        }

        public virtual void OnEnable()
        {
            // Event 등록
            health.onDead += OnDead;

            // 씬 매니저의 유닛 관리 대상에 추가
            SceneManager.Instance.AddUnit(this);
        }

        // 씬매니저를 통한 고정 업데이트로 호출될 함수
        public virtual void FixedTick() { }

        // 씬매니저를 통한 업데이트로 호출될 함수
        public virtual void Tick() { OnUpdate?.Invoke(); }

        // 유닛이 타격을 입으면 호출되는 가상함수
        public virtual void OnDamage(Unit attacker, uint damage, bool isStrong) { }

        // 유닛이 죽은 경우 호출되는 가상함수
        public virtual void OnDead() { }

        public virtual void OnDisable()
        {
            // event 해제
            health.onDead -= OnDead;

            // 씬 매니저의 유닛 관리 대상에서 해제
            if (SceneManager.Instance) 
                SceneManager.Instance.RemoveUnit(gameObject.GetInstanceID());
        }

    }
}