using UnityEngine;
using System.Collections.Generic;

namespace SK
{
    public class EquipmentHolderHook : MonoBehaviour
    {
        private GameObject _currentEquipmentModel;
        public GameObject CurrentEquipmentModel => _currentEquipmentModel;

        private EquipmentHolderManager _equipmentHolderManager;

        // 모델 재사용을 위한 딕셔너리(키: 모델 이름, 값: 모델 오브젝트)
        private Dictionary<string, GameObject> _modelContainer = new Dictionary<string, GameObject>();

        // 현재 할당된 장비 데이터
        public Equipments AssignedEquipment { get; private set; }

        internal void Initialize(EquipmentHolderManager manager)
            => _equipmentHolderManager = manager;
        
        public bool IsLoaded => _currentEquipmentModel != null;

        public void UnloadWeapon()
        {
            if (_currentEquipmentModel != null)
            {
                _currentEquipmentModel.SetActive(false);
                _currentEquipmentModel = null;

                // 방패 해제 시 플레이어 애니메이터 파라미터 변경
                if (AssignedEquipment.isShield)
                    _equipmentHolderManager.anim.SetBool(Strings.AnimPara_EquipShield, false);

                AssignedEquipment = null;
            }
        }

        public void UnloadWeaponAndDestroy()
        {
            if (_currentEquipmentModel != null)
            {
                _modelContainer.Remove(_currentEquipmentModel.name);
                Destroy(_currentEquipmentModel);

                AssignedEquipment = null;
            }
        }

        // 모델 로딩(장비 데이터를 통해서)
        public void LoadModel(Equipments loadEquipment, Transform equipmentSheath)
        {
            // 모델이 있는 경우
            if (loadEquipment.modelPrefab != null)
            {
                // 현재 할당된 장비를 변수에 저장
                AssignedEquipment = loadEquipment;

                // 모델 컨테이너 딕셔너리에서 모델 이름과 비교 탐색하여 찾으면 currentModel에 할당
                var result = _modelContainer.TryGetValue(loadEquipment.modelPrefab.name, out _currentEquipmentModel);

                if (!result)
                {
                    // 딕셔너리에서 찾지 못했다면, 모델을 생성하여 currentModel에 할당
                    _currentEquipmentModel = Instantiate(AssignedEquipment.modelPrefab);
                    // 딕셔너리에 모델 추가
                    _modelContainer.Add(loadEquipment.modelPrefab.name, _currentEquipmentModel);
                }

                // 모델이 정상적으로 할당이 되었다면
                if (_currentEquipmentModel != null)
                {
                    _currentEquipmentModel.SetActive(true);

                    // 지정된 부모 트랜스폼으로 할당
                    if (equipmentSheath != null)
                        _currentEquipmentModel.transform.parent = equipmentSheath;
                    else // 지정된 부모 트랜스폼이 없다면 현재 트랜스폼에 할당
                        _currentEquipmentModel.transform.parent = transform;

                    // 위치, 회전, 스케일 값 초기화
                    _currentEquipmentModel.transform.localPosition = AssignedEquipment.sheathModelPosition;
                    _currentEquipmentModel.transform.localRotation = Quaternion.Euler(AssignedEquipment.sheathModelRotation);
                    _currentEquipmentModel.transform.localScale = AssignedEquipment.modelPrefab.transform.localScale;

                    // 플레이어 상태 창에 장비 아이템 보이도록 Layer를 동일화 시킴
                    if (_currentEquipmentModel.transform.childCount > 0)
                        _currentEquipmentModel.transform.GetChild(0).gameObject.layer = gameObject.layer;
                }
            }
        }
    }
}