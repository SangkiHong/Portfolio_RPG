using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using SK.Data;
using Random = UnityEngine.Random;

namespace SK.Loot
{
    /* 작성자: 홍상기
     * 내용: 전리품(드랍) 아이템에 관련된 기능을 가진 매니저 클래스
     * 작성일: 22년 7월 19일
     */

    public struct LootData
    {
        public Item lootItem;
        public float dropChance;
        public int maxAmount;
        public int minAmount;
    }

    public class LootManager : MonoBehaviour
    {
        // 싱글톤
        private static LootManager _instance;
        public static LootManager Instance => _instance;

        [SerializeField] private int lootItemSeedSize;
        [SerializeField] private GameObject lootItemPrefab;
        
        [Header("UI")]
        [SerializeField] private Transform lootIcon;
        [SerializeField] private LootItemListPanel lootItemListPanel;

        // 전리품 데이터를 저장한 딕셔너리(키: 적 ID, 값: 전리품 데이터)
        private Dictionary<int, List<LootData>> _lootDataDic = new Dictionary<int, List<LootData>>();

        // 사용 대기 중인 전리품 아이템 컴포넌트 큐
        private Queue<LootItems> _waitingUseLootItems = new Queue<LootItems>();

        // 사용 중인 전리품 아이템 컴포넌트 딕셔너리(키: 인스턴스 ID)
        private Dictionary<int, LootItems> _usingLootItems = new Dictionary<int, LootItems>();

        // 전리품 획득을 위한 인풋 액션
        private InputActionMap _lootingInputActionMap;
        private InputAction _inputAction_OpenLootItemList;
        private InputAction _inputAction_LootAllItems;
        private InputAction _inputAction_CloseList;
        private LootItems _selectedLoot;
        private LootItems _OnCloseLoot;
        private Camera _mainCamera;
        private Transform _thisTransform;

        private int _selectedLootIndex;

        private void Awake()
            => _instance = this;

        private void Start()
            => Initialize();

        public void Initialize()
        {
            // CSV 데이터 파일을 로드 및 파싱하여 전리품 데이터 딕셔너리에 추가
            DataManager.Instance.LoadLootData(_lootDataDic);

            _mainCamera = Camera.main;
            _thisTransform = transform;

            // 인풋 초기화
            _inputAction_OpenLootItemList = GameManager.Instance.InputManager.playerInput.actions["OpenLootItemList"];
            _inputAction_LootAllItems = GameManager.Instance.InputManager.playerInput.actions["LootAllItems"];
            _inputAction_CloseList = GameManager.Instance.InputManager.playerInput.actions["CloseList"];
            _lootingInputActionMap = GameManager.Instance.InputManager.playerInput.actions.actionMaps[4];
            _inputAction_OpenLootItemList.started += OpenLootItemList;
            _inputAction_LootAllItems.started += LootAllItems;
            _inputAction_CloseList.started += ClosePanel;
            _lootingInputActionMap.Disable();

            // 컴포넌트 오브젝트 생성
            AddNewLootItem(lootItemSeedSize);
        }

        // 컴포넌트 오브젝트 생성
        private void AddNewLootItem(int addSize = 2)
        {
            GameObject lootItemObject;

            // 컴포넌트 오브젝트 생성
            for (int i = 0; i < addSize; i++)
            {
                // 전리품 오브젝트 생성
                lootItemObject = Instantiate(lootItemPrefab, _thisTransform);

                // 컴포넌트 정보 가져오기
                _selectedLoot = lootItemObject.GetComponent<LootItems>();

                // 할당 해제 시 호출될 이벤트에 함수 추가
                _selectedLoot.OnUnassign += UnassignLootItem;
                _selectedLoot.OnLootDistance += OnLootDistance;
                _selectedLoot.OnFarAway += OnFarAway;

                // 사용 대기 딕셔너리에 컴포넌트 추가
                _waitingUseLootItems.Enqueue(_selectedLoot);

                // 오브젝트 끔
                _selectedLoot.gameObject.SetActive(false);
            }
        }

        // 지정된 장소에 적 ID에 따라 아이템 드랍
        public void DropLoot(int enemyId, Vector3 dropPosition)
        {
            // 드랍 정보가 없으면 즉시 반환
            if (!_lootDataDic.ContainsKey(enemyId)) return;

            // 사용 가능한 전리품 컴포넌트가 없는 경우 새로 생성
            if (_waitingUseLootItems.Count == 0)
                AddNewLootItem();

            // 컴포넌트를 큐에서 꺼냄
            _selectedLoot = _waitingUseLootItems.Dequeue();

            foreach (var lootData in _lootDataDic[enemyId])
            {
                // 랜덤 확률로 드랍 여부
                if (lootData.dropChance > Random.value)
                {
                    // 드랍 수량의 최소 값과 최대 값 범위에 따라 드랍 수량 결정
                    int amount = lootData.minAmount != lootData.maxAmount ?
                        Random.Range(lootData.minAmount, lootData.maxAmount + 1) : lootData.minAmount;

                    // 드랍 아이템 리스트에 추가
                    _selectedLoot.AddItem(lootData.lootItem, amount);
                }
            }

            // 드랍 아이템이 할당 되었다면
            if (_selectedLoot.IsAssigned)
            {
                // 사용 중인 딕셔너리에 추가
                _usingLootItems.Add(_selectedLoot.GetInstanceID(), _selectedLoot);
                // 위치 이동
                _selectedLoot.transform.position = dropPosition;
                // 오브젝트 보이기
                _selectedLoot.gameObject.SetActive(true);
            }
            // 아이템이 1개도 할당되지 않았다면 다시 큐에 넣음
            else
                _waitingUseLootItems.Enqueue(_selectedLoot);
        }

        #region Event Method
        // 전리품 컴포넌트를 사용 해제 및 대기 큐에 추가
        private void UnassignLootItem(int instanceID)
        {
            _OnCloseLoot = null;

            // 액션 맵 중지
            _lootingInputActionMap.Disable();

            // 사용 대기 컴포넌트 큐에 추가
            _waitingUseLootItems.Enqueue(_usingLootItems[instanceID]);
            
            // 사용 중인 컴포넌트 딕셔너리에서 제거
            _usingLootItems.Remove(instanceID);
        }

        // 플레이어가 획득 가능 거리에 도달한 경우 호출될 이벤트 함수
        private void OnLootDistance(int instanceID)
        {
            Debug.Log("루팅 거리 도달");
            _selectedLootIndex = instanceID;
            _OnCloseLoot = _usingLootItems[_selectedLootIndex];

            lootIcon.gameObject.SetActive(true);

            _lootingInputActionMap.Enable();
            _inputAction_LootAllItems.Disable();

            SceneManager.Instance.OnFixedUpdate += UpdateIconPosition;
        }
        
        // 획득 거리보다 멀어진 경우 호출될 이벤트 함수
        private void OnFarAway(int instanceID)
        {
            Debug.Log("루팅 거리에서 멀어짐");
            lootIcon.gameObject.SetActive(false);
            
            _OnCloseLoot = null;

            // 패널이 열려있다면 닫기
            if (lootItemListPanel.IsOpen) 
                lootItemListPanel.Hide();

            // 인풋 액션맵 중지
            _lootingInputActionMap.Disable();

            // 아이콘 위치 업데이트 중지
            SceneManager.Instance.OnFixedUpdate -= UpdateIconPosition;
        }

        // 획득 가능 전리품 리스트 UI 패널을 여는 함수
        private void OpenLootItemList(InputAction.CallbackContext context)
        {
            if (_usingLootItems.ContainsKey(_selectedLootIndex))
            {
                lootItemListPanel.Show(_usingLootItems[_selectedLootIndex]);

                // 인풋 컨트롤
                GameManager.Instance.InputManager.SwitchInputMode(InputMode.UI);
                _inputAction_OpenLootItemList.Disable();
                _inputAction_LootAllItems.Enable();

                // 마우스 모드 및 카메라 회전 중지
                GameManager.Instance.MouseVisibleAndFixCamera(true);

                // 아이콘 숨김
                lootIcon.gameObject.SetActive(false);
                SceneManager.Instance.OnFixedUpdate -= UpdateIconPosition;
            }
        }

        // 모든 전리품 아이템을 획득하는 함수
        private void LootAllItems(InputAction.CallbackContext context)
        {
            lootItemListPanel.LootAll();
            // 아이템 모두 획득하여 패널이 닫힌 경우
            if (!lootItemListPanel.IsOpen)
            {
                // 인풋 컨트롤
                GameManager.Instance.InputManager.SwitchInputMode(InputMode.GamePlay);
                _inputAction_LootAllItems.Disable();
                _lootingInputActionMap.Disable();
            }

            // 마우스 모드 해제 및 카메라 회전 재개
            GameManager.Instance.MouseVisibleAndFixCamera(false);
        }

        private void ClosePanel(InputAction.CallbackContext context)
        {
            lootItemListPanel.Hide();

            // 인풋 컨트롤
            GameManager.Instance.InputManager.SwitchInputMode(InputMode.GamePlay);
            _inputAction_OpenLootItemList.Enable();
            _inputAction_LootAllItems.Disable();

            // 마우스 모드 해제 및 카메라 회전 재개
            GameManager.Instance.MouseVisibleAndFixCamera(false);

            // 아이콘 업데이트 시작
            if (_OnCloseLoot != null)
            {
                lootIcon.gameObject.SetActive(true);
                SceneManager.Instance.OnFixedUpdate += UpdateIconPosition;
            }
        }

        // 루팅 아이콘이 전리품 부근에 머물도록하는 업데이트 함수
        private void UpdateIconPosition()
        {
            if (_OnCloseLoot)
                lootIcon.position = _mainCamera.WorldToScreenPoint(_OnCloseLoot.transform.position);
        }
        #endregion

        private void OnApplicationQuit()
        {
            _inputAction_OpenLootItemList.started -= OpenLootItemList;
            _inputAction_LootAllItems.started -= LootAllItems;
        }
    }
}