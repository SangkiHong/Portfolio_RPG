using UnityEngine;
using System.Collections.Generic;
using System.IO;
using SK.Quests;
using SimpleJSON;

namespace SK.Data
{
    public class DataManager : MonoBehaviour
    {
        // �̱���
        public static DataManager Instance { get; private set; }

        public delegate void CurrencyChangedHandler();
        public delegate void ExpChangedHandler();

        [SerializeField] private PlayerData playerData;
        [SerializeField] private PlayerItemData playerItemData;
        [SerializeField] private UI.QuickSlotData quickSlotData;

        // ������Ƽ
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

        // �÷��̾� ������ �ʱ�ȭ_220503
        public void Initialize()
        {
            _itemListManager = GameManager.Instance.ItemListManager;

            _isCompletedLoadplayerData = LoadPlayerData();

            Debug.Log("DataManager �ʱ�ȭ �Ϸ�");
        }

        public void InitializeScene()
        {
            if (_inventoryManager == null)
                _inventoryManager = UI.UIManager.Instance.inventoryManager;
        }

        /// <summary>
        /// CSV ������ ������ ���� ������Ʈ ����_220406
        /// </summary>
        /// <param name="dataName">������ �̸�</param>
        /// <param name="prefabFolderName">�������� �����ϴ� ���� �̸�</param>
        /// <param name="parentTransform">�θ� Ʈ������</param>
        /// <returns></returns>
        public GameObject LoadResource(string dataName, string prefabFolderName, Transform parentTransform = null)
        {
            string path = Path.Combine(Application.dataPath, Path.Combine(_string_Resources, _string_Data, dataName)) + _string_CSV;

            using (StreamReader sr = new StreamReader(path))
            {
                string line = string.Empty;
                GameObject obj = null;
                Enemy tempEnemy;
                Vector3 objPos; // ��ġ ���� �Ľ� ���� Vector3

                while ((line = sr.ReadLine()) != null)
                {
                    string[] row = line.Split(new char[] { _comma }); // ,������ Split
                    if (row[0] == "Index") continue;

                    obj = Resources.Load(Path.Combine(_string_Prefabs, prefabFolderName, row[1])) as GameObject;
                    obj = parentTransform != null ? Instantiate(obj, parentTransform) : Instantiate(obj);
                    // �Ҽ��� 2�ڸ����� ��ġ, ȸ�� �� �ޱ�
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
                        // ������ ��ġ ����
                        tempEnemy.RespawnPoint = objPos;
                        // ���� ���� ����
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
        // PlayerData(Scriptable Obejct)�� ���� �÷��̾� �ʿ� ����_220503
        public GameObject InstantiatePlayer()
        {
            string s_player = "Player";

            GameObject player = Resources.Load(Path.Combine(_string_Prefabs, s_player, s_player)) as GameObject;
            player = Instantiate(player);
            player.name = s_player;
            return player;
        }

        // �÷��̾� ������ CSV ���Ϸ� ����_220503
        public void SavePlayerData()
        {
            if (GameManager.Instance.Player)
                DataUtility.ExportPlayerData(playerData, playerItemData, GameManager.Instance.Player.transform);
        }

        // �÷��̾� ������ CSV ������ ���Ϸ� ���� �Ŀ� Scriptable Obejct(PlayerData, PlayerItemData)�� ������ �ű��_220503
        private bool LoadPlayerData()
        {
            string path = Path.Combine(Application.dataPath, Path.Combine(_string_Resources, _string_Data, "Player", "PlayerData")) + _string_CSV;

            // �ε��� �÷��̾� ������ ������ ���� ��� ���� ����
            if (!File.Exists(path))
            {
                using (File.Create(path))
                {
                    _initializedPlayerData = true;

                    playerData.Initialize();
                    quickSlotData.slotInfoList.Clear();
                    playerItemData.items.Clear();

                    Debug.Log("�÷��̾� ������ ���� �Ϸ�");
                }
            }

            using (StreamReader sr = new StreamReader(path))
            {
                bool loadPlayerData = false;
                string line;
                int rowIndex;
                ItemData itemData;

                // ���� ������ ����Ʈ(Scriptable Object) �ʱ�ȭ   
                if (playerItemData.items == null)
                    playerItemData.items = new List<ItemData>();
                else
                    playerItemData.items.Clear();

                while ((line = sr.ReadLine()) != null)
                {
                    string[] row = line.Split(new char[] { _comma }); // ,������ Split
                    if (row[0] == "Index") continue;

                    // �÷��̾� ���� �ε�
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
                        // ������ ���� �ε�
                        itemData = new ItemData();
                        itemData.item = GameManager.Instance.ItemListManager
                                .GetItem(int.Parse(row[rowIndex++]), (ItemType)int.Parse(row[rowIndex++]), (EquipmentType)int.Parse(row[rowIndex++]));
                        itemData.amount = uint.Parse(row[rowIndex++]);
                        itemData.slotID = int.Parse(row[rowIndex++]);

                        // ������ ���� ����
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

        // �÷��̾� ���� ȭ�� ����
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
        // �÷��̾� ����ġ ����
        public void AddExp(uint amount)
        {
            Debug.Log($"����ġ ��� {amount}");
            PlayerData.Exp += amount;
            OnChangedExp?.Invoke();
        }
        #endregion

        #region ITEM DATA
        // �κ��丮�� ������ ������ �������� �߰�_220610
        public bool AddItem(Item item, uint amount, bool applyData)
        { 
            return _inventoryManager.AddNewItem(item, amount, applyData); 
        }

        // ����Ʈ ���� ���� ������ �߰�_220610
        public void GrantReward(Reward reward, List<Item> rewardItemList)
        {
            if (reward.exp > 0) playerData.Exp += reward.exp;
            if (reward.gold > 0) playerData.Gold += reward.gold;
            if (reward.gem > 0) playerData.Gem += reward.gem;

            // ���� ������ ����
            for (int i = 0; i < rewardItemList.Count; i++)
                AddItem(rewardItemList[i], reward.rewardItems[i].itemAmount, true);
        }

        // �� ���� �ε����� ��ȯ_220719
        public int GetEmptySlotNum()
        {
            return playerItemData.GetEmptySlotIndex();
        }

        // ���� �Ҵ�� ������ ������ ������ ���� ������ �����Ϳ� �߰�_220507
        public void AddNewItemData(UI.InventorySlot inventorySlot)
            => playerItemData.AddItem(inventorySlot.AssignedItem, inventorySlot.slotID, inventorySlot.GetItemAmount());

        // ���� ������ ������ ����_220507
        public void DeleteItemData(UI.InventorySlot inventorySlot, int slotID, uint amount)
            => playerItemData.RemoveItem(inventorySlot.AssignedItem, slotID, amount);

        // ���� ������ ������ ����_220507
        public void DeleteItemData(Item item, int slotID, uint amount)
            => playerItemData.RemoveItem(item, slotID, amount);

        // ���� ������ ���� ����(A���� ID, B���� ID)_220506
        public void SwapSlot(int aSlotID, int bSlotID)
            => playerItemData.SwapSlotData(aSlotID, bSlotID);

        // ���� ������ �����Ϳ��� ������ ���� ����(������ ����, ������ ����)_220507
        public void UpdateItemData(Item item, uint currentAmount, uint changeAmount)
            => playerItemData.ChangeSlotInfo(item, currentAmount, changeAmount);

        // �������� ���� ���� ����(������ ����, ������ ����)_220512
        public void UpdateItemData(Item item, bool equip)
            => playerItemData.ChangeSlotInfo(item, equip);
        #endregion

        #region QUEST DATA
        // ����Ʈ ����Ʈ�� ����Ʈ �����ͷ� ����_220524
        public void SaveQuestData()
        {
            if (GameManager.Instance.Player)
                DataUtility.ExportQuestsData(UI.UIManager.Instance.questManager.activeQuestsList, 
                    UI.UIManager.Instance.questManager.completedQuestsList);
        }

        // ����Ʈ Json �����͸� ����Ʈ ����Ʈ�� �Ľ�_220524
        public bool LoadQuestData(List<Quest> ActivedQuestList, List<Quest> completedQuestList)
        {
            TextAsset textAsset = Resources.Load<TextAsset>(Path.Combine(_string_Data, "QuestJsonData"));

            // ������ ���� ���
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
                // ����Ʈ �̸��� ���� ����Ʈ ���� �ε�
                tempQuest = Resources.Load<Quest>(Path.Combine(_string_Data, string_Quests, root[i][string_QuestName]));

                // ����Ʈ �̸��� ���� ����Ʈ ���� �ε�
                tempQuest.SetState((QuestState)root[i][string_QuestState].AsInt);

                // �Ϸ�� ����Ʈ�� ��� �Ϸ�� ����Ʈ ����Ʈ�� �߰�
                if (tempQuest.QuestState == QuestState.Complete)
                {
                    completedQuestList.Add(tempQuest);
                    continue;
                }

                // ���� ����Ʈ ���� �ε��� �� �ε�
                tempQuest.SetTaskGroupIndex(root[i][string_CurrentTaskGroupIndex].AsInt);
                // ���� ����Ʈ ���� ���� �ε�
                tempQuest.SetTaskGroupState((TaskGroupState)root[i][string_TaskGroupState].AsInt);

                // ���� ����Ʈ ������ �Ϸ�� ���°� �ƴ� ��� ���� ������ �ϼ� Ƚ�� �ε�
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

        // ���ҽ� �������� ����Ʈ ������ �ε��Ͽ� ��ȯ�ϴ� �Լ�_220524
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

                    string[] row = line.Split(new char[] { _comma }); // ,������ Split

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
        /// ���� �Ǹ� ������ ���� ����(CSV)�κ��� �����͸� �Ľ��Ͽ� ����Ʈ�� �߰�_220615
        /// </summary>
        /// <param name="shopType">���� Ÿ��(0: ��ȭ����, 1: ������)</param>
        /// <param name="itemList">������ ����� �߰��� ������ ����Ʈ</param>
        public void GetShopList(int shopType, ref List<Item> itemList)
        {
            string dataPath = string.Empty;

            // ��ȭ ���� ������ �ҷ�����
            if (shopType == 0)            
                dataPath = Path.Combine(Application.dataPath, Path.Combine(_string_Resources, _string_Data, "ShopData_Props")) + _string_CSV;            
            // ��� ���� ������ �ҷ�����
            else if (shopType == 1)
                dataPath = Path.Combine(Application.dataPath, Path.Combine(_string_Resources, _string_Data, "ShopData_Equipments")) + _string_CSV;

            using (StreamReader sr = new StreamReader(dataPath))
            {
                string line = string.Empty;

                while ((line = sr.ReadLine()) != null)
                {
                    string[] row = line.Split(new char[] { _comma }); // ,������ Split
                    if (row[0] == "Index") continue;

                    // �Ľ��� �����͸� ���� ������ ������ ������ ����Ʈ�� �߰�
                    itemList.Add(_itemListManager.GetItembyID(int.Parse(row[1]), int.Parse(row[2])));
                }
                sr.Close();
            }
        }
        #endregion

        #region SKILL DATA
        // Ȱ��ȭ�� ��ų�� Json���Ϸ� ����_220620
        public void SaveSkillData()
        {
            if (GameManager.Instance.Player)
                DataUtility.ExportSkillData();
        }

        // ��ų Json �����͸� ��ų ���� ��ųʸ��� �Ľ�_220620
        public void LoadSKill(ref Dictionary<int, UI.SkillSlot> skillSlotDic)
        {
            TextAsset textAsset = Resources.Load<TextAsset>(Path.Combine(_string_Data, "SkillJsonData"));

            // ������ ���� ���
            if (textAsset == null)
            {
                _isCompletedLoadSkillData = true;
                return;
            }

            JSONNode root = JSON.Parse(textAsset.text)["List"];

            // �ش� ��ų ������ Ȱ��ȭ
            for (int i = 0; i < root.Count; i++)
                skillSlotDic[root[i].AsInt]?.Active();

            _isCompletedLoadSkillData = true;
        }

        // ��ų ����Ʈ ����
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
        // CSV ������ ���Ͽ��� ����ǰ ������ �ε�_220720
        public void LoadLootData(Dictionary<int, List<Loot.LootData>> lootDataDic)
        {
            string path = Path.Combine(Application.dataPath, Path.Combine(_string_Resources, _string_Data, "LootData")) + _string_CSV;

            // ������ ������ ���� ��� ���� �α�
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
                    string[] row = line.Split(new char[] { _comma }); // ,������ Split
                    if (!skipFirstLine)
                    {
                        skipFirstLine = true;
                        continue;
                    }

                    // �� ���� ID ������ ����
                    enemyId = int.Parse(row[0]);
                    // ����ǰ ������ ���� �� ������ �ε�
                    tempData = new Loot.LootData();
                    tempData.lootItem = _itemListManager.GetItembyID((ItemListType)int.Parse(row[1]), int.Parse(row[2]));
                    tempData.dropChance = float.Parse(row[3]);
                    tempData.minAmount = int.Parse(row[4]);
                    tempData.maxAmount = int.Parse(row[5]);

                    // ��ųʸ��� ���� �߰� �� ����Ʈ ����
                    if (!lootDataDic.ContainsKey(enemyId))
                    {
                        // ��ųʸ��� �߰�(Ű: �� ID, ��: ����ǰ ������ ����Ʈ)
                        lootDataDic.Add(enemyId, new List<Loot.LootData>());
                    }

                    // ����Ʈ�� ����ǰ ������ �߰�
                    lootDataDic[enemyId].Add(tempData);
                }
                sr.Close();
            }
        }
        #endregion

        // ���� �� �÷��̾� ������ ����_220503
        private void OnApplicationQuit()
        {
            // ������ �ε尡 �Ϸ�Ǿ��� ���� ����_220513
            if (_isCompletedLoadplayerData && GameManager.Instance.Player) GameManager.Instance.Player.SavePlayState();
            if (_isCompletedLoadplayerData) SavePlayerData();
            if (_isCompletedLoadQuestData) SaveQuestData();
            if (_isCompletedLoadSkillData) SaveSkillData();
        }
    }
}