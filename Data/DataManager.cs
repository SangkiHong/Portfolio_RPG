using UnityEngine;
using System.IO;

namespace SK.Data
{
    public class DataManager : MonoBehaviour
    {
        [SerializeField] private PlayerData playerData;
        [SerializeField] private PlayerItemData playerItemData;

        [Header("Reference")]
        public ItemListManager itemListManager;
        public GrassManager grassManager;

        public PlayerData PlayerData => playerData;
        public PlayerItemData PlayerItemData => playerItemData;

        private readonly string _prefabs = "Prefabs";
        private readonly string _resources = "Resources";
        private readonly string _data = "Data";
        private readonly char _comma = ',';
        private readonly string _enemyTag = "Enemy";
        private readonly string _grassTag = "Grass";

        private bool isCompleteLoad;

        // ��Ʈ�� ������ �÷��̾� ������ �ҷ�����_220503
        private void Awake()
        {
            isCompleteLoad = LoadPlayerData();

            if (!isCompleteLoad)
            {
                // �ε��� �÷��̾� ������ ������ ���� ���
                playerData.Initialize();
            }
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
            string path = Path.Combine(Application.dataPath, Path.Combine(_resources, _data, dataName)) + ".csv";

            using (StreamReader sr = new StreamReader(path))
            {
                string line = string.Empty;
                GameObject obj = null;
                Vector3 objPos; // ��ġ ���� �Ľ� ���� Vector3

                while ((line = sr.ReadLine()) != null)
                {
                    string[] row = line.Split(new char[] { _comma }); // ,������ Split
                    if (row[0] == "Index") continue;

                    obj = Resources.Load(Path.Combine(_prefabs, prefabFolderName, row[1])) as GameObject;
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
                    if (obj.CompareTag(_enemyTag))
                        obj.GetComponent<Enemy>().isPatrol = bool.Parse(row[11]);

                    // Grass Add List
                    if (obj.CompareTag(_grassTag))
                        grassManager.AddGrass(obj.GetComponent<ProceduralGrassRenderer>());
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

            GameObject player = Resources.Load(Path.Combine(_prefabs, s_player, s_player)) as GameObject;
            player = Instantiate(player);
            player.transform.position = playerData.RecentLocation;
            player.name = s_player;
            return player;
        }

        // �÷��̾� ������ CSV ���Ϸ� ����_220503
        public void SavePlayerData()
        {
            if (GameManager.Instance.Player)
                ExportData.ExportPlayerData(playerData, GameManager.Instance.Player.playerItemData, GameManager.Instance.Player.transform);
        }

        // �÷��̾� ������ CSV ������ ���Ϸ� ���� �Ŀ� Scriptable Obejct(PlayerData, PlayerItemData)�� ������ �ű��_220503
        private bool LoadPlayerData()
        {
            string path = Path.Combine(Application.dataPath, Path.Combine(_resources, _data, "PlayerData")) + ".csv";

            if (!File.Exists(path))
                return false;                

            using (StreamReader sr = new StreamReader(path))
            {
                bool loadPlayerData = false;
                string line;
                ItemData itemData;

                // ���� ������ ����Ʈ(Scriptable Object) �ʱ�ȭ   
                if (playerItemData.items == null)
                    playerItemData.items = new System.Collections.Generic.List<ItemData>();
                else
                    playerItemData.items.Clear();

                while ((line = sr.ReadLine()) != null)
                {
                    string[] row = line.Split(new char[] { _comma }); // ,������ Split
                    if (row[0] == "Name" || row[0] == "ItemID") continue;

                    // �÷��̾� ���� �ε�
                    if (!loadPlayerData)
                    {
                        loadPlayerData = true;
                        playerData.Name = row[0];
                        playerData.Level = uint.Parse(row[1]);
                        playerData.Exp = uint.Parse(row[2]);
                        playerData.MaxHp = uint.Parse(row[3]);
                        playerData.MaxMp = uint.Parse(row[4]);
                        playerData.MaxSp = uint.Parse(row[5]);
                        playerData.Str = uint.Parse(row[6]);
                        playerData.Dex = uint.Parse(row[7]);
                        playerData.Int = uint.Parse(row[8]);
                        playerData.AttackSpeed = uint.Parse(row[9]);
                        playerData.CriticalChance = float.Parse(row[10]);
                        playerData.CriticalMultiplier = float.Parse(row[11]);
                        playerData.Armor = uint.Parse(row[12]);
                        playerData.Speed = float.Parse(row[13]);
                        playerData.Avoidance = uint.Parse(row[14]);
                        playerData.RecoverHp = float.Parse(row[15]);
                        playerData.RecoverMp = float.Parse(row[16]);
                        playerData.RecoverSp = float.Parse(row[17]);
                        playerData.RecentLocation
                            = new Vector3(float.Parse(row[18]), float.Parse(row[19]), float.Parse(row[20]));
                    }
                    else
                    {
                        // ������ ���� �ε�
                        itemData = new ItemData
                        {
                            item = itemListManager
                            .GetItem(int.Parse(row[0]), (ItemType)int.Parse(row[1]), (EquipmentType)int.Parse(row[2])),
                            amount = uint.Parse(row[3]),
                            slotID = int.Parse(row[4])
                        };
                        if (row.Length > 5)
                            itemData.isEquiped = int.Parse(row[5]) == 1;

                        playerItemData.items.Add(itemData);
                    }

                }
                sr.Close();
            }

            return true;
        }
        #endregion

        #region ITEM DATA
        // ���ο� ���� & ������ �߰�_220507
        public void AddNewItemData(UI.InventorySlot inventorySlot)
            => playerItemData.AddItem(inventorySlot.assignedItem, inventorySlot.slotID, inventorySlot.GetItemAmount());

        // ���� ������ ������ ����_220507
        public void DeleteItemData(UI.InventorySlot inventorySlot)
            => playerItemData.RemoveItem(inventorySlot.assignedItem, inventorySlot.slotID);

        // ���� ������ ������ ����_220507
        public void DeleteItemData(Item item, uint amount)
            => playerItemData.RemoveItem(item, amount);

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

        // ���� �� �÷��̾� ������ ����_220503
        private void OnApplicationQuit()
        {
            // ������ �ε尡 �Ϸ�Ǿ��� ���� �����ϵ��� ����_220513
            if (isCompleteLoad) SavePlayerData();
        }
    }
}