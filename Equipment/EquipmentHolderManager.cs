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

        public Equipments _currentUseEquipment;
        public Equipments CurrentUseEquipment 
        {
            get
            {
                if (_currentUseEquipment == null)
                    _currentUseEquipment = primaryEquipment;
                return _currentUseEquipment;
            }
            private set { _currentUseEquipment = value; }
        }

        internal Animator anim;
        private EquipmentHolderHook _currentUseEquipmentHook;
        private Item _tempItem;
        private Transform _targetTransform;

        private void Awake()
        {
            // Hook 초기화
            if (rightHandHook) rightHandHook.Initialize(this);
            if (leftHandHook) leftHandHook.Initialize(this);
            if (shieldHook) shieldHook.Initialize(this);

            // 컴포넌트 초기화
            anim = GetComponent<Animator>();
            // 죽을 경우 장비 해제 이벤트 등록
            //GetComponent<Health>().onDead += UnloadEquipment;

            // 몬스터인 경우 사전 장비 장착
            if (primaryEquipment) { 
                LoadEquipmentOnHook(primaryEquipment, true);
                _currentUseEquipment = primaryEquipment;
            }
            if (secondaryEquipment) LoadEquipmentOnHook(secondaryEquipment, false);
        }

        // 장비 착용
        public void AssignEquipment(Equipments equipment, bool isPrimary, bool isInitializing = false)
        {
            if (equipment == null) return;

            if (isPrimary) // 장비 착용(주 무기)
            {
                primaryEquipment = equipment as Weapon;

                // 장비가 투핸드라면 보조 장비 착용 해제
                if (primaryEquipment.isTwoHand && secondaryEquipment)
                {
                    if (leftHandHook.AssignedEquipment == secondaryEquipment) leftHandHook.UnloadWeapon();
                    if (shieldHook.AssignedEquipment == secondaryEquipment) shieldHook.UnloadWeapon();

                    // 인벤토리 착용 해제
                    _tempItem = GameManager.Instance.ItemListManager.GetItem(secondaryEquipment);
                    if (_tempItem != null)
                    {
                        // 장비 슬롯에서 해제
                        UI.UIManager.Instance.equipSlotManager.UnequipItem(_tempItem);
                        // 인벤토리 슬롯에서 해제
                        UI.UIManager.Instance.inventoryManager.UnqeuipItem(_tempItem);
                    }

                    secondaryEquipment = null;
                }
            }
            else // 장비 착용(보조 장비)
            { 
                secondaryEquipment = equipment;

                // 방패 착용 시 플레이어 애니메이터 파라미터 변경
                if (secondaryEquipment.isShield)
                    anim.SetBool(Strings.AnimPara_EquipShield, true);
            }

            LoadEquipmentOnHook(equipment, isPrimary, isInitializing);
        }

        // 장비 착용 해제
        public void UnassignEquipment(Equipments equipment)
        {
            if (rightHandHook.AssignedEquipment == equipment)
            {
                rightHandHook.UnloadWeapon();
                primaryEquipment = null;
            }
            if (leftHandHook.AssignedEquipment == equipment)
            {
                leftHandHook.UnloadWeapon();
                secondaryEquipment = null;
            }
            if (shieldHook.AssignedEquipment == equipment)
            {
                shieldHook.UnloadWeapon();
                secondaryEquipment = null;
            }
        }

        // 착용 위치 지정 및 모델 로딩
        public void LoadEquipmentOnHook(Equipments equipment, bool isPrimary, bool isInitializing = false)
        {
            if (isPrimary)
            {
                // 사용 중인 장비가 있다면 착용 해제
                if (rightHandHook.IsLoaded)
                {
                    Unequip();
                    rightHandHook.UnloadWeapon();
                }

                // 모델 로드 및 착용
                rightHandHook.LoadModel(equipment, 
                    equipment.sheathPosition == SheathPosition.SwordSheath ? sheathPrimary : sheathBack);
            }
            else
            {
                // 보조 장비가 방패인 경우
                if (equipment.isShield)
                {
                    // 사용 중인 장비가 있다면 착용 해제
                    if (shieldHook.IsLoaded)
                    {
                        Unequip(1);
                        shieldHook.UnloadWeapon();
                    }

                    // 모델 로드 및 착용
                    shieldHook.LoadModel(equipment, 
                        equipment.sheathPosition == SheathPosition.SwordSheath ? sheathPrimary : sheathBack);
                }
                else // 보조 장비가 무기인 경우
                {
                    // 사용 중인 장비가 있다면 착용 해제
                    if (leftHandHook.IsLoaded)
                    {
                        Unequip(1);
                        leftHandHook.UnloadWeapon();
                    }

                    // 모델 로드 및 착용
                    leftHandHook.LoadModel(equipment, 
                        equipment.sheathPosition == SheathPosition.SwordSheath ? sheathPrimary : sheathBack);
                }
            }

            // 장비 착용에 따른 애니메이션 재생 이벤트 호출
            if (!isInitializing) ChangeEquipment();
        }

        public bool IsEquipedWeapon(bool isPrimary)
        {
            if (isPrimary) return primaryEquipment != null;
            else return secondaryEquipment != null;
        }

        // 현재 착용 중인 무기 정보 반환
        public Weapon GetUseWeapon(bool isPrimary)
        {
            // 주 무기 공격 실행
            if (isPrimary && primaryEquipment)
                _currentUseEquipment = primaryEquipment;
            // 보조 장비(무기) 공격 실행
            else if (!isPrimary && secondaryEquipment)
                _currentUseEquipment = secondaryEquipment;

            return (Weapon)_currentUseEquipment;
        }

        #region Animation Event
        // 장비 착용(애니메이션 이벤트 함수로도 사용)
        public void Equip(int isPrimary = 0)
        {
            if (isPrimary == 0) // 주무기
            {
                if (rightHandHook.AssignedEquipment == null) return;

                // 현재 할당된 주 무기의 트랜스폼을 변수에 할당
                _targetTransform = rightHandHook.CurrentEquipmentModel.transform;

                // 모델의 부모 트랜스폼을 Hook으로 지정
                _targetTransform.parent = rightHandHook.transform;
            }
            else // 보조 무기
            {
                // 보조 무기 훅 할당
                if (leftHandHook.AssignedEquipment != null) _currentUseEquipmentHook = leftHandHook;
                if (shieldHook.AssignedEquipment != null) _currentUseEquipmentHook = shieldHook;

                // 착용 중인 보조 무기가 없는 경우 리턴
                if (_currentUseEquipmentHook == null) return;

                // 현재 할당된 보조 장비의 트랜스폼을 변수에 할당
                _targetTransform = _currentUseEquipmentHook.AssignedEquipment != null ?
                    _currentUseEquipmentHook.CurrentEquipmentModel.transform : leftHandHook.CurrentEquipmentModel.transform;

                // 정상적인 할당이 이뤄지지 않았다면 return
                if (!_targetTransform) return;

                // 모델의 부모 트랜스폼을 Hook으로 지정
                _targetTransform.parent = _currentUseEquipmentHook.AssignedEquipment != null ?
                    _currentUseEquipmentHook.transform : leftHandHook.transform;
            }
            
            // 타겟 트랜스폼의 로컬 위치, 로컬 회전, 스케일 값을 초기화
            _targetTransform.localPosition = Vector3.zero;
            _targetTransform.localRotation = Quaternion.identity;
            _targetTransform.localScale = Vector3.one;
        }

        // 장비 착용 해제(애니메이션 이벤트 함수로도 사용)
        public void Unequip(int isPrimary = 0)
        {
            _currentUseEquipmentHook = null;

            if (isPrimary == 0)
            {
                if (primaryEquipment == null) return;
                
                _currentUseEquipmentHook = rightHandHook;

                // 현재 할당된 주 무기의 트랜스폼을 변수에 할당
                _targetTransform = rightHandHook.CurrentEquipmentModel.transform;
            }
            else
            {
                if (secondaryEquipment == null) return;

                if (leftHandHook.IsLoaded) _currentUseEquipmentHook = leftHandHook;
                else if(shieldHook.IsLoaded) _currentUseEquipmentHook = shieldHook;

                // 현재 할당된 보조 장비의 트랜스폼을 변수에 할당
                _targetTransform = shieldHook.AssignedEquipment != null ?
                    shieldHook.CurrentEquipmentModel.transform : leftHandHook.CurrentEquipmentModel.transform;
            }

            // 정상적인 할당이 이뤄지지 않았다면 return
            if (!_targetTransform) return;

            if (_currentUseEquipmentHook != null)
            {
                // 모델의 부모 트랜스폼을 장착 위치로 지정
                _targetTransform.parent = _currentUseEquipmentHook.AssignedEquipment.sheathPosition
                    == SheathPosition.SwordSheath ? sheathPrimary : sheathBack;

                _targetTransform.localPosition = _currentUseEquipmentHook.AssignedEquipment.sheathModelPosition;
                _targetTransform.localRotation = Quaternion.Euler(_currentUseEquipmentHook.AssignedEquipment.sheathModelRotation);
                _targetTransform.localScale = Vector3.one;
            }
        }

        // 전투 모드 상태인 경우 착용 애니메이션 재생
        public void ChangeEquipment()
        {
            // 전투 모드 중에 장비를 착용하는 경우 착용 애니메이션 재생
            if (anim.GetBool(Strings.AnimPara_onCombat))
            {
                // 주무기
                if (primaryEquipment != null)
                {
                    if (primaryEquipment.sheathPosition == SheathPosition.SwordSheath)
                        GameManager.Instance.Player.PlayerTargetAnimation(Strings.AnimName_Equip_SwordSheath, false, 2);
                    else
                        GameManager.Instance.Player.PlayerTargetAnimation(Strings.AnimName_Equip_BackSheath, false, 4);
                }

                // 보조장비(방패)
                if (secondaryEquipment != null)
                {
                    if (secondaryEquipment.sheathPosition == SheathPosition.Back)
                        GameManager.Instance.Player.PlayerTargetAnimation(Strings.AnimName_Equip_Shield, false, 3);
                }
            }

            // 방패 착용 여부 확인하여 애니메이션 파라미터 변경
            if (secondaryEquipment && secondaryEquipment.isShield)
                anim.SetBool(Strings.AnimPara_EquipShield, true);
            else
                anim.SetBool(Strings.AnimPara_EquipShield, false);

            // 어떤 무기도 착용하지 않은 경우 전투모드 해제
            if (!primaryEquipment && !secondaryEquipment)
                anim.SetBool(Strings.AnimPara_onCombat, false);
        }
        #endregion

        // 모든 장비 해제
        private void UnloadEquipment()
        {
            if (rightHandHook && rightHandHook.AssignedEquipment) rightHandHook.UnloadWeapon();
            if (leftHandHook && leftHandHook.AssignedEquipment) leftHandHook.UnloadWeapon();
            if (shieldHook && shieldHook.AssignedEquipment) shieldHook.UnloadWeapon();
        }
    }
}