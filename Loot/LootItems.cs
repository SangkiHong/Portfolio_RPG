using System.Collections.Generic;
using UnityEngine;
using SK.Utilities;

namespace SK.Loot
{
    /* 작성자: 홍상기
     * 내용: 전리품(드랍) 아이템 오브젝트 관리 컴포넌트
     * 작성일: 22년 7월 19일
     */

    public class LootItems : MonoBehaviour
    {
        public delegate void UnassignHandler(int instanceID);
        public delegate void CanLootingHandler(int instanceID);
        public delegate void FarAwayHandler(int instanceID);
        public event UnassignHandler OnUnassign;
        public event CanLootingHandler OnLootDistance;
        public event FarAwayHandler OnFarAway;

        [SerializeField] private float lootingDistance;

        private Dictionary<Item, int> lootItemDic;
        public IReadOnlyDictionary<Item, int> LootItemDic => lootItemDic;

        private Transform _transform, _playerTransform;

        private bool _canLooting;
        private bool _isAssigned;
        public bool IsAssigned => _isAssigned;

        private void Awake()
        {
            _transform = transform;
            lootItemDic = new Dictionary<Item, int>();

            // 최적화 거리 비교를 위해 제곱
            lootingDistance *= lootingDistance;
        }

        // 전리품 리스트에 아이템 추가
        public void AddItem(Item lootItem, int amount)
        {
            _isAssigned = true;
            lootItemDic.Add(lootItem, amount);
        }

        // 전리품 리스트에서 아이템 삭제
        public void TakeItem(Item lootItem)
        {
            // 드랍 아이템이 금화 또는 잼인 경우
            if (lootItem.Id < 0)
            {
                if (lootItem.Id == -1) // 드랍 아이템이 금화인 경우
                    Data.DataManager.Instance.AddGold((uint)lootItemDic[lootItem]);
                else if (lootItem.Id == -2) // 드랍 아이템이 잼인 경우
                    Data.DataManager.Instance.AddGem((uint)lootItemDic[lootItem]);
            }
            // 아이템 추가
            else
            {
                Data.DataManager.Instance.AddItem(lootItem, (uint)lootItemDic[lootItem], true);
            }

            // 리스트에서 제거
            lootItemDic.Remove(lootItem);
            
            // 남은 아이템이 없는 경우
            if (lootItemDic.Count == 0)
            {
                // 할당 해제를 매니저에게 알리기 위한 이벤트 호출
                OnUnassign?.Invoke(GetInstanceID());
                _isAssigned = false;
                gameObject.SetActive(false);
            }
        }

        // 전리품 할당 해제
        public void Unassign()
        {
            lootItemDic.Clear();
            // 할당 해제를 매니저에게 알리기 위한 이벤트 호출
            OnUnassign?.Invoke(GetInstanceID());
            _isAssigned = false;
            gameObject.SetActive(false);
        }

        // 플레이어와의 거리 비교
        private void FixedUpdate()
        {
            if (_playerTransform == null)
                _playerTransform = GameManager.Instance.Player.mTransform;

            if (_playerTransform)
            {
                // 플레이어와 거리 비교하여 루팅 가능 거리에 도달한 경우
                if (lootingDistance >= MyMath.Instance.GetDistance(_transform.position, _playerTransform.position))
                {
                    if (!_canLooting)
                    {
                        _canLooting = true;
                        OnLootDistance?.Invoke(GetInstanceID());
                    }
                }
                else
                {
                    if (_canLooting)
                    {
                        _canLooting = false;
                        OnFarAway?.Invoke(GetInstanceID());
                    }
                }
            }
        }
    }
}