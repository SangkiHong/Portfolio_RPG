using UnityEngine;
using System.Collections.Generic;

namespace SK
{
    public class EquipmentHolderHook : MonoBehaviour
    {
        [SerializeField] internal GameObject currentModel;
        [SerializeField] private Transform parentOverride;

        internal Equipments assignedEquipment;

        // 모델 재사용을 위한 딕셔너리
        private Dictionary<string, GameObject> _modelContainer;

        internal void Initialize()
        {
            _modelContainer = new Dictionary<string, GameObject>();
        }

        public void UnloadWeapon()
        {
            if (currentModel != null)
            {
                currentModel.SetActive(false);
                currentModel = null;
            }
        }

        public void UnloadWeaponAndDestroy()
        {
            if (currentModel != null)
            {
                Destroy(currentModel);
            }
        }

        // 모델 로딩(장비 데이터를 통해서)
        public void LoadModel(Equipments loadEquipment)
        {
            if (assignedEquipment == null)
            {
                // 현재 할당된 장비를 변수에 저장
                assignedEquipment = loadEquipment;

                // 모델 컨테이너 딕셔너리에서 모델 이름과 비교 탐색하여 찾으면 currentModel에 할당
                var result = _modelContainer.TryGetValue(loadEquipment.modelPrefab.name, out currentModel);

                if (!result)
                {
                    // 딕셔너리에서 찾지 못했다면, 모델을 생성하여 currentModel에 할당
                    currentModel = Instantiate(assignedEquipment.modelPrefab);
                    // 딕셔너리에 모델 추가
                    _modelContainer.Add(loadEquipment.modelPrefab.name, currentModel);
                }

                // 모델이 정상적으로 할당이 되었다면
                if (currentModel != null)
                {
                    // 지정된 부모 트랜스폼으로 할당
                    if (parentOverride != null)
                        currentModel.transform.parent = parentOverride;
                    else // 지정된 부모 트랜스폼이 없다면 현재 트랜스폼에 할당
                        currentModel.transform.parent = transform;

                    // 위치, 회전, 스케일 값 초기화
                    currentModel.transform.localPosition = assignedEquipment.modelPrefab.transform.localPosition;
                    currentModel.transform.localRotation = assignedEquipment.modelPrefab.transform.localRotation;
                    currentModel.transform.localScale = assignedEquipment.modelPrefab.transform.localScale;

                    // 플레이어 상태 창에 장비 아이템 보이도록 Layer를 동일화 시킴
                    if (currentModel.transform.childCount > 0)
                        currentModel.transform.GetChild(0).gameObject.layer = gameObject.layer;
                }
            }
        }
    }
}