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

        // 인트로 씬에서 플레이어 데이터 불러오기_220503
        private void Awake()
        {
            isCompleteLoad = LoadPlayerData();

            if (!isCompleteLoad)
            {
                // 로드할 플레이어 데이터 파일이 없는 경우
                playerData.Initialize();
            }
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
            string path = Path.Combine(Application.dataPath, Path.Combine(_resources, _data, dataName)) + ".csv";

            using (StreamReader sr = new StreamReader(path))
            {
                string line = string.Empty;
                GameObject obj = null;
                Vector3 objPos; // 위치 값을 파싱 받을 Vector3

                while ((line = sr.ReadLine()) != null)
                {
                    string[] row = line.Split(new char[] { _comma }); // ,단위로 Split
                    if (row[0] == "Index") continue;

                    obj = Resources.Load(Path.Combine(_prefabs, prefabFolderName, row[1])) as GameObject;
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
        // PlayerData(Scriptable Obejct)를 토대로 플레이어 맵에 생성_220503
        public GameObject InstantiatePlayer()
        {
            string s_player = "Player";

            GameObject player = Resources.Load(Path.Combine(_prefabs, s_player, s_player)) as GameObject;
            player = Instantiate(player);
            player.transform.position = playerData.RecentLocation;
            player.name = s_player;
            return player;
        }

        // 플레이어 정보를 CSV 파일로 저장_220503
        public void SavePlayerData()
        {
            if (GameManager.Instance.Player)
                ExportData.ExportPlayerData(playerData, GameManager.Instance.Player.playerItemData, GameManager.Instance.Player.transform);
        }

        // 플레이어 정보를 CSV 데이터 파일로 읽은 후에 Scriptable Obejct(PlayerData, PlayerItemData)로 데이터 옮기기_220503
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

                // 소유 아이템 리스트(Scriptable Object) 초기화   
                if (playerItemData.items == null)
                    playerItemData.items = new System.Collections.Generic.List<ItemData>();
                else
                    playerItemData.items.Clear();

                while ((line = sr.ReadLine()) != null)
                {
                    string[] row = line.Split(new char[] { _comma }); // ,단위로 Split
                    if (row[0] == "Name" || row[0] == "ItemID") continue;

                    // 플레이어 정보 로드
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
                        // 아이템 정보 로드
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
        // 새로운 슬롯 & 아이템 추가_220507
        public void AddNewItemData(UI.InventorySlot inventorySlot)
            => playerItemData.AddItem(inventorySlot.assignedItem, inventorySlot.slotID, inventorySlot.GetItemAmount());

        // 슬롯 아이템 데이터 삭제_220507
        public void DeleteItemData(UI.InventorySlot inventorySlot)
            => playerItemData.RemoveItem(inventorySlot.assignedItem, inventorySlot.slotID);

        // 슬롯 아이템 데이터 삭제_220507
        public void DeleteItemData(Item item, uint amount)
            => playerItemData.RemoveItem(item, amount);

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

        // 종료 시 플레이어 데이터 저장_220503
        private void OnApplicationQuit()
        {
            // 데이터 로드가 완료되었을 때만 저장하도록 변경_220513
            if (isCompleteLoad) SavePlayerData();
        }
    }
}