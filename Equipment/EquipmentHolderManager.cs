using UnityEngine;

namespace SK
{
    public class EquipmentHolderManager : MonoBehaviour
    {
        [Header("Equipment")]
        public Weapon primaryEquipment;
        public Equipments secondaryEquipment;

        [Header("Hook")]
        public EquipmentHolderHook rightHandHook;
        public EquipmentHolderHook leftHandHook;
        public EquipmentHolderHook shieldHook;

        [SerializeField] private Transform sheathPrimary;
        [SerializeField] private Transform sheathBack;

        internal Equipments currentUseEquipment;

        private bool _isEquipPrimary, _isEquipSecondary; // 착여 여부 확인용 Bool값
        private Transform _targetTransform;

        private void Awake()
        {
            // Hook 초기화
            if (rightHandHook) rightHandHook.Initialize();
            if (leftHandHook) leftHandHook.Initialize();
            if (shieldHook) shieldHook.Initialize();

            // 죽을 경우 장비 해제 이벤트 등록
            GetComponent<Health>().onDead += UnloadEquipment;

            // 몬스터인 경우 사전 장비 장착
            if (primaryEquipment) { 
                LoadEquipmentOnHook(primaryEquipment, true);
                currentUseEquipment = primaryEquipment;
            }
            if (secondaryEquipment) LoadEquipmentOnHook(secondaryEquipment, false);
        }

        public void AssignEquipment(Equipments equipment, bool isPrimary)
        {
            if (equipment == null) return;

            if (isPrimary) // 장비 착용(주 무기)
                primaryEquipment = equipment as Weapon;
            else // 장비 착용(보조 장비)
                secondaryEquipment = equipment;

            LoadEquipmentOnHook(equipment, isPrimary);
        }

        // 착용 위치 지정 및 모델 로딩
        public void LoadEquipmentOnHook(Equipments equipment, bool isPrimary)
        {
            if (isPrimary)
                rightHandHook.LoadModel(equipment);
            else
            { 
                // 보조 장비가 방패인 경우
                if (equipment.equipType.Equals(EquipType.Shield))
                    shieldHook.LoadModel(equipment);
                else // 보조 장비가 무기인 경우
                    leftHandHook.LoadModel(equipment);
            }
        }

        // 장비 착용(애니메이션 이벤트 함수로도 사용)
        public void Equip(int isPrimary = 0)
        {
            if (isPrimary == 0) // 주무기
            {
                if (!_isEquipPrimary)
                {
                    _isEquipPrimary = true;

                    // 현재 할당된 주 무기의 트랜스폼을 변수에 할당
                    _targetTransform = rightHandHook.currentModel.transform;

                    // 모델의 부모 트랜스폼을 Hook으로 지정
                    _targetTransform.parent = rightHandHook.transform;
                }
            }
            else // 보조 무기
            {
                if (!_isEquipSecondary)
                {
                    _isEquipSecondary = true;

                    // 현재 할당된 보조 장비의 트랜스폼을 변수에 할당
                    _targetTransform = shieldHook.assignedEquipment != null ? 
                        shieldHook.currentModel.transform : leftHandHook.currentModel.transform;

                    // 정상적인 할당이 이뤄지지 않았다면 return
                    if (!_targetTransform) return;

                    // 모델의 부모 트랜스폼을 Hook으로 지정
                    _targetTransform.parent = shieldHook.assignedEquipment != null ?
                        shieldHook.transform : leftHandHook.transform;
                }
            }
            
            // 타겟 트랜스폼의 로컬 위치, 로컬 회전, 스케일 값을 초기화
            _targetTransform.localPosition = Vector3.zero;
            _targetTransform.localRotation = Quaternion.identity;
            _targetTransform.localScale = Vector3.one;
        }

        // 장비 착용 해제(애니메이션 이벤트 함수로도 사용)
        public void Unequip(int isPrimary = 0)
        {
            if (isPrimary == 0)
            {
                if (_isEquipPrimary)
                {
                    _isEquipPrimary = false;

                    // 현재 할당된 주 무기의 트랜스폼을 변수에 할당
                    _targetTransform = rightHandHook.currentModel.transform;

                    // 모델의 부모 트랜스폼을 Sheath로 지정
                    _targetTransform.parent = sheathPrimary;
                }
            }
            else
            {
                if (_isEquipSecondary)
                {
                    _isEquipSecondary = false;

                    // 현재 할당된 보조 장비의 트랜스폼을 변수에 할당
                    _targetTransform = shieldHook.assignedEquipment != null ?
                        shieldHook.currentModel.transform : leftHandHook.currentModel.transform;

                    // 정상적인 할당이 이뤄지지 않았다면 return
                    if (!_targetTransform) return;

                    // 모델의 부모 트랜스폼을 Sheath로 지정
                    _targetTransform.parent = sheathBack;
                }
            }
            
            _targetTransform.localPosition = Vector3.zero;
            _targetTransform.localRotation = Quaternion.identity;
            _targetTransform.localScale = Vector3.one;
        }

        public Weapon GetUseWeapon(bool isLeftAttack)
        {
            // 주 무기 공격 실행
            if (isLeftAttack && primaryEquipment)
                currentUseEquipment = primaryEquipment;
            // 보조 장비(무기) 공격 실행
            else if (!isLeftAttack && secondaryEquipment)
                currentUseEquipment = secondaryEquipment;

            return (Weapon)currentUseEquipment;
        }

        // 모든 장비 해제
        private void UnloadEquipment()
        {
            if (rightHandHook.assignedEquipment) rightHandHook.UnloadWeapon();
            if (shieldHook.assignedEquipment) shieldHook.UnloadWeapon();
            if (leftHandHook.assignedEquipment) leftHandHook.UnloadWeapon();
        }
    }
}