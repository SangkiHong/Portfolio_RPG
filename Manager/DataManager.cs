using UnityEngine;
using System.Collections.Generic;
using System.IO;
using SK.Quests;
using SimpleJSON;

namespace SK.Data
{
    public class DataManager : MonoBehaviour
    {
        // 싱글톤
        public static DataManager Instance { get; private set; }

        public delegate void CurrencyChangedHandler();
        public delegate void ExpChangedHandler();

        [SerializeField] private PlayerData playerData;
        [SerializeField] private PlayerItemData playerItemData;
        [SerializeField] private UI.QuickSlotData quickSlotData;

        // 프로퍼티
        public PlayerData PlayerData => playerData;
        public PlayerItemData PlayerItemData => playerItemData;

        private ItemListManager _itemListManager;
        private UI.InventoryManager _inventoryManager;

        public event CurrencyChangedHandler OnChangedCurrency;
        public event ExpChangedHandler OnChangedExp;

        private readonly string _string_Resources = "Resources";
        private readonly string _string_Prefabs = "Prefabs";
        private readonly string _string_Data = "Data";
        private readonly string _string_CSV = ".csv";
        private readonly string _tag_Enemy = "Enemy";
        private readonly char _comma = ',';

        private bool _initializedPlayerData;

        private bool _isCompletedLoadplayerData,
                     _isCompletedLoadQuestData,
                     _isCompletedLoadSkillData;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(this);
            }
            else
                Destroy(gameObject);
        }

        // 플레이어 데이터 초기화_220503
        public void Initialize()
        {
            _itemListManager = GameManager.Instance.ItemListManager;

            _isCompletedLoadplayerData = LoadPlayerData();

            Debug.Log("DataManager 초기화 완료");
        }

        public void InitializeScene()
        {
            if (_inventoryManager == null)
                _inventoryManager = UI.UIManager.Instance.inventoryManager;
        }

        /// <summary>
        /// CSV 데이터 파일을 토대로 오브젝트 생성_220406
        /// </summary>
        /// <param name="dataName">데이터 이름</param>
        /// <param name="prefabFolderName">프리펩을 포함하는 폴더 이름</param>
        /// <param name="parentTransform">부모 트렌스폼</param>
        /// <returns></returns>
        public GameObject LoadResource(string dataName, string prefabFolderName, Transform parentTransform = null)
        {
            string path = Path.Combine(Application.dataPath, Path.Combine(_string_Resources, _string_Data, dataName)) + _string_CSV;

            using (StreamReader sr = new StreamReader(path))
            {
                string line = string.Empty;
                GameObject obj = null;
                Enemy tempEnemy;
                Vector3 objPos; // 위치 값을 파싱 받을 Vector3

                while ((line = sr.ReadLine()) != null)
                {
                    string[] row = line.Split(new char[] { _comma }); // ,단위로 Split
                    if (row[0] == "Index") continue;

                    obj = Resources.Load(Path.Combine(_string_Prefabs, prefabFolderName, row[1])) as GameObject;
                    obj = parentTransform != null ? Instantiate(obj, parentTransform) : Instantiate(obj);
                    // 소수점 2자리까지 위치, 회전 값 받기
                    objPos.x = float.Parse(string.Format("{0:0.#}", row[2]));
                    objPos.y = float.Parse(string.Format("{0:0.#}", row[3]));
                    objPos.z = float.Parse(string.Format("{0:0.#}", row[4]));
                    obj.transform.SetPositionAndRotation(objPos, 
                        Quaternion.Euler(float.Parse(string.Format("{0:0.#}", row[5])), 
                                         float.Parse(string.Format("{0:0.#}", row[6])), 
                                         float.Parse(string.Format("{0:0.#}", row[7]))));
                    obj.transform.localScale = new Vector3(float.Parse(row[8]), float.Parse(row[9]), float.Parse(row[10]));

                    // Enemy Information
                    if (obj.CompareTag(_tag_Enemy))
                    {
                        tempEnemy = obj.GetComponent<Enemy>();
                        // 리스폰 위치 저장
                        tempEnemy.RespawnPoint = objPos;
                        // 순찰 가능 여부
                        tempEnemy.isPatrol = bool.Parse(row[11]);
                    }

                    // Grass Add List
                    /*if (obj.CompareTag(_tag_Grass))
                        SceneManager.Instance.grassManager.AddGrass(obj.GetComponent<ProceduralGrassRenderer>());*/
                }
                sr.Close();
                return obj;
            }
        }

        #region PLAYER DATA
        // PlayerData(Scriptable Obejct)를 토대로 플레이어 맵에 생성_220503
        public GameObject InstantiatePlayer()
        {
            string s_player = "Player";

            GameObject player = Resources.Load(Path.Combine(_string_Prefabs, s_player, s_player)) as GameObject;
            player = Instantiate(player);
            player.name = s_player;
            return player;
        }

        // 플레이어 정보를 CSV 파일로 저장_220503
        public void SavePlayerData()
        {
            if (GameManager.Instance.Player)
                DataUtility.ExportPlayerData(playerData, playerItemData, GameManager.Instance.Player.transform);
        }

        // 플레이어 정보를 CSV 데이터 파일로 읽은 후에 Scriptable Obejct(PlayerData, PlayerItemData)로 데이터 옮기기_220503
        private bool LoadPlayerData()
        {
            string path = Path.Combine(Application.dataPath, Path.Combine(_string_Resources, _string_Data, "Player", "PlayerData")) + _string_CSV;

            // 로드할 플레이어 데이터 파일이 없는 경우 새로 생성
            if (!File.Exists(path))
            {
                using (File.Create(path))
                {
                    _initializedPlayerData = true;

                    playerData.Initialize();
                    quickSlotData.slotInfoList.Clear();
                    playerItemData.items.Clear();

                    Debug.Log("플레이어 데이터 생성 완료");
                }
            }

            using (StreamReader sr = new StreamReader(path))
            {
                bool loadPlayerData = false;
                string line;
                int rowIndex;
                ItemData itemData;

                // 소유 아이템 리스트(Scriptable Object) 초기화   
                if (playerItemData.items == null)
                    playerItemData.items = new List<ItemData>();
                else
                    playerItemData.items.Clear();

                while ((line = sr.ReadLine()) != null)
                {
                    string[] row = line.Split(new char[] { _comma }); // ,단위로 Split
                    if (row[0] == "Index") continue;

                    // 플레이어 정보 로드
                    if (!loadPlayerData)
                    {
                        rowIndex = 1;
                        loadPlayerData = true;
                        playerData.Name = row[rowIndex++];
                        playerData.Level = uint.Parse(row[rowIndex++]);
                        playerData.Exp = uint.Parse(row[rowIndex++]);
                        playerData.Gold = uint.Parse(row[rowIndex++]);
                        playerData.Gem = uint.Parse(row[rowIndex++]);
                        playerData.Hp = uint.Parse(row[rowIndex++]);
                        playerData.Mp = uint.Parse(row[rowIndex++]);
                        playerData.Sp = uint.Parse(row[rowIndex++]);
                        playerData.Str = uint.Parse(row[rowIndex++]);
                        playerData.Dex = uint.Parse(row[rowIndex++]);
                        playerData.Int = uint.Parse(row[rowIndex++]);
                        playerData.SkillPoint = uint.Parse(row[rowIndex++]);
                        playerData.StatPoint = uint.Parse(row[rowIndex++]);
                        playerData.AttackSpeed = uint.Parse(row[rowIndex++]);
                        playerData.CriticalChance = float.Parse(row[rowIndex++]);
                        playerData.CriticalMultiplier = float.Parse(row[rowIndex++]);
                        playerData.Def = uint.Parse(row[rowIndex++]);
                        playerData.Speed = float.Parse(row[rowIndex++]);
                        playerData.Avoidance = uint.Parse(row[rowIndex++]);
                        playerData.RecoverHp = float.Parse(row[rowIndex++]);
                        playerData.RecoverMp = float.Parse(row[rowIndex++]);
                        playerData.RecoverSp = float.Parse(row[rowIndex++]);
                        playerData.RecentPosition
                            = new Vector3(float.Parse(row[rowIndex++]), float.Parse(row[rowIndex++]), float.Parse(row[rowIndex++]));
                        playerData.RecentLocation = int.Parse(row[rowIndex++]);
                    }
                    else
                    {
                        rowIndex = 1;
                        // 아이템 정보 로드
                        itemData = new ItemData();
                        itemData.item = GameManager.Instance.ItemListManager
                                .GetItem(int.Parse(row[rowIndex++]), (ItemType)int.Parse(row[rowIndex++]), (EquipmentType)int.Parse(row[rowIndex++]));
                        itemData.amount = uint.Parse(row[rowIndex++]);
                        itemData.slotID = int.Parse(row[rowIndex++]);

                        // 아이템 착용 여부
                        if (row[rowIndex] != null)
                            itemData.isEquiped = int.Parse(row[rowIndex]) == 1;

                        if (itemData.item != null) 
                            playerItemData.items.Add(itemData);
                    }

                }
                sr.Close();
            }

            return true;
        }

        // 플레이어 소지 화폐 수정
        public void AddGold(uint amount)
        { 
            PlayerData.Gold += amount;
            OnChangedCurrency?.Invoke();
        }
        public void SubtractGold(uint amount)
        {
            PlayerData.Gold -= amount;
            OnChangedCurrency?.Invoke();
        }
        public void AddGem(uint amount)
        {
            PlayerData.Gem += amount;
            OnChangedCurrency?.Invoke();
        }
        public void SubtractGem(uint amount)
        { 
            PlayerData.Gem -= amount;
            OnChangedCurrency?.Invoke();
        }
        // 플레이어 경험치 수정
        public void AddExp(uint amount)
        {
            Debug.Log($"경험치 상승 {amount}");
            PlayerData.Exp += amount;
            OnChangedExp?.Invoke();
        }
        #endregion

        #region ITEM DATA
        // 인벤토리와 데이터 정보에 아이템을 추가_220610
        public bool AddItem(Item item, uint amount, bool applyData)
        { 
            return _inventoryManager.AddNewItem(item, amount, applyData); 
        }

        // 퀘스트 보상에 대한 아이템 추가_220610
        public void GrantReward(Reward reward, List<Item> rewardItemList)
        {
            if (reward.exp > 0) playerData.Exp += reward.exp;
            if (reward.gold > 0) playerData.Gold += reward.gold;
            if (reward.gem > 0) playerData.Gem += reward.gem;

            // 보상 아이템 지급
            for (int i = 0; i < rewardItemList.Count; i++)
                AddItem(rewardItemList[i], reward.rewardItems[i].itemAmount, true);
        }

        // 빈 슬롯 인덱스를 반환_220719
        public int GetEmptySlotNum()
        {
            return playerItemData.GetEmptySlotIndex();
        }

        // 새로 할당된 슬롯의 아이템 정보와 슬롯 정보를 데이터에 추가_220507
        public void AddNewItemData(UI.InventorySlot inventorySlot)
            => playerItemData.AddItem(inventorySlot.AssignedItem, inventorySlot.slotID, inventorySlot.GetItemAmount());

        // 슬롯 아이템 데이터 삭제_220507
        public void DeleteItemData(UI.InventorySlot inventorySlot, int slotID, uint amount)
            => playerItemData.RemoveItem(inventorySlot.AssignedItem, slotID, amount);

        // 슬롯 아이템 데이터 삭제_220507
        public void DeleteItemData(Item item, int slotID, uint amount)
            => playerItemData.RemoveItem(item, slotID, amount);

        // 슬롯 아이템 정보 변경(A슬롯 ID, B슬롯 ID)_220506
        public void SwapSlot(int aSlotID, int bSlotID)
            => playerItemData.SwapSlotData(aSlotID, bSlotID);

        // 슬롯 아이템 데이터에서 아이템 수량 변경(아이템 정보, 변경할 수량)_220507
        public void UpdateItemData(Item item, uint currentAmount, uint changeAmount)
            => playerItemData.ChangeSlotInfo(item, currentAmount, changeAmount);

        // 아이템의 착용 여부 변경(아이템 정보, 변경할 상태)_220512
        public void UpdateItemData(Item item, bool equip)
            => playerItemData.ChangeSlotInfo(item, equip);
        #endregion

        #region QUEST DATA
        // 퀘스트 리스트를 퀘스트 데이터로 저장_220524
        public void SaveQuestData()
        {
            if (GameManager.Instance.Player)
                DataUtility.ExportQuestsData(UI.UIManager.Instance.questManager.activeQuestsList, 
                    UI.UIManager.Instance.questManager.completedQuestsList);
        }

        // 퀘스트 Json 데이터를 퀘스트 리스트로 파싱_220524
        public bool LoadQuestData(List<Quest> ActivedQuestList, List<Quest> completedQuestList)
        {
            TextAsset textAsset = Resources.Load<TextAsset>(Path.Combine(_string_Data, "QuestJsonData"));

            // 파일이 없는 경우
            if (_initializedPlayerData || textAsset == null)
            {
                _isCompletedLoadQuestData = true;
                return false; 
            }

            JSONNode root = JSON.Parse(textAsset.text)["List"];
            Quest tempQuest;

            string string_Quests = "Quests",
                   string_QuestName = "QuestName",
                   string_QuestState = "QuestState",
                   string_CurrentTaskGroupIndex = "CurrentTaskGroupIndex",
                   string_TaskGroupState = "TaskGroupState",
                   string_TasksSuccessValue = "TasksSuccessValue";

            for (int i = 0; i < root.Count; i++)
            {
                // 퀘스트 이름을 통해 퀘스트 파일 로드
                tempQuest = Resources.Load<Quest>(Path.Combine(_string_Data, string_Quests, root[i][string_QuestName]));

                // 퀘스트 이름을 통해 퀘스트 파일 로드
                tempQuest.SetState((QuestState)root[i][string_QuestState].AsInt);

                // 완료된 퀘스트인 경우 완료된 퀘스트 리스트에 추가
                if (tempQuest.QuestState == QuestState.Complete)
                {
                    completedQuestList.Add(tempQuest);
                    continue;
                }

                // 현재 퀘스트 업무 인덱스 값 로드
                tempQuest.SetTaskGroupIndex(root[i][string_CurrentTaskGroupIndex].AsInt);
                // 현재 퀘스트 업무 상태 로드
                tempQuest.SetTaskGroupState((TaskGroupState)root[i][string_TaskGroupState].AsInt);

                // 현재 퀘스트 업무가 완료된 상태가 아닌 경우 현재 업무의 완수 횟수 로드
                if (!tempQuest.CurrentTaskGroup.IsComplete)
                {
                    for (int j = 0; j < tempQuest.CurrentTaskGroup.Tasks.Count; j++)
                    {
                        tempQuest.CurrentTaskGroup.Tasks[j].SetCurrentSuccess(root[i][string_TasksSuccessValue][j]);
                    }
                }
                if (!ActivedQuestList.Contains(tempQuest))
                    ActivedQuestList.Add(tempQuest);
            }

            return _isCompletedLoadQuestData = true;
        }

        // 리소스 폴더에서 퀘스트 에셋을 로드하여 반환하는 함수_220524
        public Quest GetQuest(string questName) { return Resources.Load<Quest>(Path.Combine(_string_Data, "Quests", questName)); }
        #endregion

        #region DIALOG DATA
        public void LoadDialogData(ref Dialog.SerializableDicDialog dialogsDic)
        {
            string path = Path.Combine(Application.dataPath, Path.Combine(_string_Resources, _string_Data, "DialogData")) + _string_CSV;

            if (!File.Exists(path))
                return;

            using (StreamReader sr = new StreamReader(path))
            {
                string line;
                Dialog.Dialog tempDialog;
                tempDialog.scripts = null;

                while ((line = sr.ReadLine()) != null)
                {
                    if (line == string.Empty) continue;

                    string[] row = line.Split(new char[] { _comma }); // ,단위로 Split

                    if (row[0] == "DialogKey") continue;

                    if (row[0] != string.Empty)
                    {
                        tempDialog = new Dialog.Dialog();
                        tempDialog.scripts = new List<string>();
                        tempDialog.scripts.Add(row[1]);
                        dialogsDic.Add(row[0], tempDialog);
                    }
                    else
                        tempDialog.scripts.Add(row[1]);
                }

                sr.Close();
            }
        }
        #endregion

        #region SHOP DATA
        /// <summary>
        /// 상점 판매 아이템 정보 파일(CSV)로부터 데이터를 파싱하여 리스트에 추가_220615
        /// </summary>
        /// <param name="shopType">상점 타입(0: 잡화상점, 1: 장비상점)</param>
        /// <param name="itemList">아이템 목록을 추가할 아이템 리스트</param>
        public void GetShopList(int shopType, ref List<Item> itemList)
        {
            string dataPath = string.Empty;

            // 잡화 상점 데이터 불러오기
            if (shopType == 0)            
                dataPath = Path.Combine(Application.dataPath, Path.Combine(_string_Resources, _string_Data, "ShopData_Props")) + _string_CSV;            
            // 장비 상점 데이터 불러오기
            else if (shopType == 1)
                dataPath = Path.Combine(Application.dataPath, Path.Combine(_string_Resources, _string_Data, "ShopData_Equipments")) + _string_CSV;

            using (StreamReader sr = new StreamReader(dataPath))
            {
                string line = string.Empty;

                while ((line = sr.ReadLine()) != null)
                {
                    string[] row = line.Split(new char[] { _comma }); // ,단위로 Split
                    if (row[0] == "Index") continue;

                    // 파싱한 데이터를 토대로 아이템 정보를 가져와 리스트에 추가
                    itemList.Add(_itemListManager.GetItembyID(int.Parse(row[1]), int.Parse(row[2])));
                }
                sr.Close();
            }
        }
        #endregion

        #region SKILL DATA
        // 활성화된 스킬을 Json파일로 저장_220620
        public void SaveSkillData()
        {
            if (GameManager.Instance.Player)
                DataUtility.ExportSkillData();
        }

        // 스킬 Json 데이터를 스킬 관련 딕셔너리로 파싱_220620
        public void LoadSKill(ref Dictionary<int, UI.SkillSlot> skillSlotDic)
        {
            TextAsset textAsset = Resources.Load<TextAsset>(Path.Combine(_string_Data, "SkillJsonData"));

            // 파일이 없는 경우
            if (textAsset == null)
            {
                _isCompletedLoadSkillData = true;
                return;
            }

            JSONNode root = JSON.Parse(textAsset.text)["List"];

            // 해당 스킬 슬롯을 활성화
            for (int i = 0; i < root.Count; i++)
                skillSlotDic[root[i].AsInt]?.Active();

            _isCompletedLoadSkillData = true;
        }

        // 스킬 포인트 차감
        public bool UseSkillPoint(uint requireSkillPoint)
        {
            if (playerData.SkillPoint >= requireSkillPoint)
            {
                playerData.SkillPoint -= requireSkillPoint;
                return true;
            }

            return false;
        }
        #endregion

        #region LOOT DATA
        // CSV 데이터 파일에서 전리품 데이터 로드_220720
        public void LoadLootData(Dictionary<int, List<Loot.LootData>> lootDataDic)
        {
            string path = Path.Combine(Application.dataPath, Path.Combine(_string_Resources, _string_Data, "LootData")) + _string_CSV;

            // 데이터 파일이 없는 경우 에러 로그
            if (!File.Exists(path))
            {
                Debug.LogError("No exist LootData File!");
                return;
            }

            using (StreamReader sr = new StreamReader(path))
            {
                string line = string.Empty;
                bool skipFirstLine = false;
                Loot.LootData tempData;
                int enemyId;

                while ((line = sr.ReadLine()) != null)
                {
                    string[] row = line.Split(new char[] { _comma }); // ,단위로 Split
                    if (!skipFirstLine)
                    {
                        skipFirstLine = true;
                        continue;
                    }

                    // 적 고유 ID 변수에 저장
                    enemyId = int.Parse(row[0]);
                    // 전리품 데이터 생성 및 데이터 로드
                    tempData = new Loot.LootData();
                    tempData.lootItem = _itemListManager.GetItembyID((ItemListType)int.Parse(row[1]), int.Parse(row[2]));
                    tempData.dropChance = float.Parse(row[3]);
                    tempData.minAmount = int.Parse(row[4]);
                    tempData.maxAmount = int.Parse(row[5]);

                    // 딕셔너리에 새로 추가 시 리스트 생성
                    if (!lootDataDic.ContainsKey(enemyId))
                    {
                        // 딕셔너리에 추가(키: 적 ID, 값: 전리품 데이터 리스트)
                        lootDataDic.Add(enemyId, new List<Loot.LootData>());
                    }

                    // 리스트에 전리품 데이터 추가
                    lootDataDic[enemyId].Add(tempData);
                }
                sr.Close();
            }
        }
        #endregion

        // 종료 시 플레이어 데이터 저장_220503
        private void OnApplicationQuit()
        {
            // 데이터 로드가 완료되었을 때만 저장_220513
            if (_isCompletedLoadplayerData && GameManager.Instance.Player) GameManager.Instance.Player.SavePlayState();
            if (_isCompletedLoadplayerData) SavePlayerData();
            if (_isCompletedLoadQuestData) SaveQuestData();
            if (_isCompletedLoadSkillData) SaveSkillData();
        }
    }
}