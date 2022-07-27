using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SK.FX;

namespace SK
{
    /* 작성자: 홍상기
     * 요약: 이펙트 효과를 불러와 화면에 보여지게 하는 관리자 클래스
     * 작성일: 22년 6월 23일
     */

    // 이펙트 풀링 소스 구조체
    [System.Serializable]
    public struct EffectPoolList
    {
        public int seedSize;
        public GameObject effectPrefab;
        public string description;
    }

    public class EffectManager : MonoBehaviour
    {
        // 싱글톤 패턴
        public static EffectManager Instance { get; private set; }

        [SerializeField] private Data.EffectData effectData;

        // 이펙트 저장할 딕셔너리(키: 이펙트 ID, 값: 이펙트를 저장한 큐)
        private Dictionary<int, Queue<Effect>> _effectDic;
        // 사용 중인 이펙트를 저장할 딕셔너리(키: 이펙트 인스턴스 ID, 값: 이펙트)
        private Dictionary<int, Effect> _usingEffectDic;

        private Transform _playerTransform;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(this);
            }
            else
                Destroy(gameObject);

            Initialize();
        }

        private void Initialize()
        {
            // 딕셔너리 초기화
            _effectDic = new Dictionary<int, Queue<Effect>>();
            _usingEffectDic = new Dictionary<int, Effect>();

            Transform thisTransform = transform;
            Effect tempEffect = null;
            for (int i = 0; i < effectData.effectList.Count; i++)
            {
                // 첫 오브젝트 생성 및 Effect 클래스 가져오기
                tempEffect = Instantiate(effectData.effectList[i].effectPrefab, thisTransform).GetComponent<Effect>();
                tempEffect.Initialize();
                // 사용이 끝난 경우 호출되는 이벤트에 함수 할당
                tempEffect.OnEndPlay += PushToPool;

                // 이펙트 ID
                int effectId = tempEffect.effectId;

                // 해당 키로 생성한 적이 없다면 딕셔너리 추가 및 큐 생성
                if (!_effectDic.ContainsKey(effectId))
                    _effectDic.Add(effectId, new Queue<Effect>());
                _effectDic[effectId].Enqueue(tempEffect);

                // 이펙트 풀의 시드 수를 변수에 추가
                int seedSize = effectData.effectList[i].seedSize;
                // 시드가 2 이상이면 시드-1 만큼 추가 생성 및 큐에 추가
                if (seedSize > 1) 
                {
                    for (int j = 0; j < seedSize - 1; j++)
                    {
                        tempEffect = Instantiate(tempEffect, thisTransform);
                        tempEffect.Initialize();
                        // 사용이 끝난 경우 호출되는 이벤트에 함수 할당
                        tempEffect.OnEndPlay += PushToPool;
                        _effectDic[effectId].Enqueue(tempEffect);
                    }
                }
            }
        }

        public void PlayEffect(int effectId, Transform ownerTransform = null)
        {
            // 사용 가능한 이펙트가 있는 경우
            if (_effectDic[effectId].Count > 0)
            {
                // 큐에서 이펙트를 꺼냄
                Effect effect = _effectDic[effectId].Dequeue();

                // 플레이어 트랜스폼 가져옴
                if (_playerTransform == null)
                    _playerTransform = GameManager.Instance.Player.mTransform;

                // 이펙트를 재생
                effect.Play(ownerTransform != null ? ownerTransform : _playerTransform);
                _usingEffectDic.Add(effect.GetInstanceID(), effect);
            }
        }

        public void PlayEffect(int effectId, Vector3 position, Quaternion rotation)
        {
            // 사용 가능한 이펙트가 있는 경우
            if (_effectDic[effectId].Count > 0)
            {
                // 큐에서 이펙트를 꺼냄
                Effect effect = _effectDic[effectId].Dequeue();
                // 이펙트를 재생
                effect.Play(ref position, ref rotation);
                _usingEffectDic.Add(effect.GetInstanceID(), effect);
            }
        }

        private void PushToPool(int instanceId)
        {
            // 사용 중인 이펙트 딕셔너리에서 이펙트를 가져옴
            Effect effect = _usingEffectDic[instanceId];
            // 해당 이펙트 큐에 추가
            _effectDic[effect.effectId].Enqueue(effect);
            // 사용 중인 이펙트 딕셔너리에서 제거
            _usingEffectDic.Remove(instanceId);
        }
    }
}
