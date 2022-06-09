using UnityEngine;

namespace SK.Behavior
{
    [RequireComponent(typeof(Combat))]
    public class Alert
    {
        private Enemy _temp;
        private readonly GameObject _gameObject;
        private readonly Transform _transform;
        private readonly Collider[] _colliderBuff;
        private readonly int _enemyLayerMask;
        private readonly float _alertRange;

        public Alert(GameObject gameObject, Transform transform, float alertRange)
        {
            _gameObject = gameObject;
            _transform = transform;
            _alertRange = alertRange;
            _enemyLayerMask = 1 << gameObject.layer; // 레이어 마스크 초기화
            _colliderBuff = new Collider[10]; // 범위 내 유닛을 탐색하기 위한 콜라이더 배열 버퍼
        }

        public void SendAlert(GameObject target)
        {
            if (Physics.OverlapSphereNonAlloc(_transform.position, _alertRange, _colliderBuff,
                _enemyLayerMask, QueryTriggerInteraction.Collide) > 0)
            {
                foreach (var col in _colliderBuff)
                {
                    if (col != null && col.gameObject != _gameObject)
                    {
                        // 씬 매니저 GetUnit 함수를 통해 범위 내의 유닛 정보를 반환 받음
                        _temp = (Enemy)SceneManager.Instance.GetUnit(col.gameObject.GetInstanceID());

                        // 유닛이 죽지 않고 선택된 타겟이 없으며 경계 알림이 가능하다면
                        if (!_temp.isDead && !_temp.combat.Target && _temp.canAlert)
                            // 유닛의 GetAlrert 함수를 호출하여 현재 타겟을 전달
                            _temp.GetAlert(target);
                    }
                }
            }
        }
    }
}
