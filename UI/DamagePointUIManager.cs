using System.Collections.Generic;
using UnityEngine;

namespace SK.UI
{
    public class DamagePointUIManager
    {
        public DamagePointUIManager()
            => _damageUIDic = new Dictionary<int, DamageUI>();

        // DamageUI를 저장할 딕셔너리(키: 게임오브젝트의 InstacneID)
        private Dictionary<int, DamageUI> _damageUIDic;
        private GameObject _tempObj;
        private DamageUI _tempUI;

        public void DisplayPoint(Vector3 position, uint damageValue, bool isCriticalHit)
        {
            // UI오브젝트 풀에서 UI를 가져옴
            _tempObj = UIPoolManager.Instance.GetObject(Strings.PoolName_DamagePoint, Vector3.zero);

            // TryGetValue를 통해 이미 저장된 정보를 가져와 UI에 할당
            // 딕셔너리에 없는 경우 컴포넌트를 가져와 딕셔너리에 저장
            if (_damageUIDic.Count != 0 && _damageUIDic.TryGetValue(_tempObj.GetInstanceID(), out _tempUI))
            {
                _tempUI.Assign(position, damageValue, isCriticalHit);
            }
            else
            {
                _tempUI = _tempObj.GetComponent<DamageUI>();
                _tempUI.Assign(position, damageValue, isCriticalHit);
                _damageUIDic.Add(_tempObj.GetInstanceID(), _tempUI);
            }
        }
    }
}
