using UnityEngine;
using System.Collections.Generic;
using System.IO;
using SK.Quests;
using SimpleJSON;

namespace SK.Data
{
    public class DataManager : MonoBehaviour
    {
        [SerializeField] private PlayerData playerData;
        [SerializeField] private PlayerItemData playerItemData;

        public PlayerData PlayerData => playerData;
        public PlayerItemData PlayerItemData => playerItemData;

        private readonly string _prefabs = "Prefabs";
        private readonly string _enemyTag = "Enemy";
        private readonly string _grassTag = "Grass";
        private readonly char _comma = ',';

        private bool isCompleteLoad;

        // 플레이어 데이터 초기화_220503
        public void Initialize()
        {
            isCompleteLoad = LoadPlayerData();

            if (!isCompleteLoad)
            {
                // 로드할 플레이어 데이터 파일이 없는 경우
                playerData.Initialize();
            }

            Debug.Log("DataManager 초기화 완료");
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
            string path = Path.Combine(Application.dataPath, Path.Combine("Resources", "Data", dataName)) + ".csv";

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
                        GameManager.Instance.GrassManager.AddGrass(obj.GetComponent<ProceduralGrassRenderer>());
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
                DataUtility.ExportPlayerData(playerData, GameManager.Instance.Player.playerItemData, GameManager.Instance.Player.transform);
        }

        // 플레이어 정보를 CSV 데이터 파일로 읽은 후에 Scriptable Obejct(PlayerData, PlayerItemData)로 데이터 옮기기_220503
        private bool LoadPlayerData()
        {
            string path = Path.Combine(Application.dataPath, Path.Combine("Resources", "Data", "Player", "PlayerData")) + ".csv";

            if (!File.Exists(path))
                return false;

            using (StreamReader sr = new StreamReader(path))
            {
                bool loadPlayerData = false;
                string line;
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
                        loadPlayerData = true;
                        playerData.Name = row[1];
                        playerData.Level = uint.Parse(row[2]);
                        playerData.Exp = uint.Parse(row[3]);
                        playerData.MaxHp = uint.Parse(row[4]);
                        playerData.MaxMp = uint.Parse(row[5]);
                        playerData.MaxSp = uint.Parse(row[6]);
                        playerData.Str = uint.Parse(row[7]);
                        playerData.Dex = uint.Parse(row[8]);
                        playerData.Int = uint.Parse(row[9]);
                        playerData.AttackSpeed = uint.Parse(row[10]);
                        playerData.CriticalChance = float.Parse(row[11]);
                        playerData.CriticalMultiplier = float.Parse(row[12]);
                        playerData.Def = uint.Parse(row[13]);
                        playerData.Speed = float.Parse(row[14]);
                        playerData.Avoidance = uint.Parse(row[15]);
                        playerData.RecoverHp = float.Parse(row[16]);
                        playerData.RecoverMp = float.Parse(row[17]);
                        playerData.RecoverSp = float.Parse(row[18]);
                        playerData.RecentLocation
                            = new Vector3(float.Parse(row[19]), float.Parse(row[20]), float.Parse(row[21]));
                    }
                    else
                    {
                        // 아이템 정보 로드
                        itemData = new ItemData();
                        itemData.item = GameManager.Instance.ItemListManager
                                .GetItem(int.Parse(row[1]), (ItemType)int.Parse(row[2]), (EquipmentType)int.Parse(row[3]));
                        itemData.amount = uint.Parse(row[4]);
                        itemData.slotID = int.Parse(row[5]);

                        // 아이템 착용 여부
                        if (row[6] != null)
                            itemData.isEquiped = int.Parse(row[6]) == 1;

                        playerItemData.items.Add(itemData);
                    }

                }
                sr.Close();
            }

            return true;
        }
        #endregion

        #region ITEM DATA
        public void AddNewItem(Item newItem, uint amount = 1)
        {
            if (newItem == null)
                return;

            if (!GameManager.Instance.UIManager.inventoryManager.AddNewItem(newItem, amount))
            {
                // 아이템 추가에 실패했다면(인벤토리가 꽉 찼거나 아이템 정보가 없다면)
                Debug.Log("아이템 추가에 실패하였습니다");
            }
        }

        // 새로 할당된 슬롯의 아이템 정보와 슬롯 정보를 데이터에 추가_220507
        public void AddNewItemData(UI.InventorySlot inventorySlot)
            => playerItemData.AddItem(inventorySlot.AssignedItem, inventorySlot.slotID, inventorySlot.GetItemAmount());

        // 슬롯 아이템 데이터 삭제_220507
        public void DeleteItemData(UI.InventorySlot inventorySlot)
            => playerItemData.RemoveItem(inventorySlot.AssignedItem, inventorySlot.slotID);

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

        #region QUEST DATA
        // 퀘스트 리스트를 퀘스트 데이터로 저장_220524
        public void SaveQuestData()
        {
            if (GameManager.Instance.Player)
                DataUtility.ExportQuestsData(ref GameManager.Instance.UIManager.questManager.activeQuestsList);
        }

        // 퀘스트 Json 데이터를 퀘스트 리스트로 파싱_220524
        public bool LoadQuestData(ref List<Quest> ActivedQuestList, ref List<Quest> completedQuestList)
        {
            TextAsset textAsset = Resources.Load<TextAsset>(Path.Combine("Data", "QuestJsonData"));
            JSONNode root = JSON.Parse(textAsset.text)["QuestInfo"];
            Quest tempQuest;

            for (int i = 0; i < root.Count; i++)
            {
                // 퀘스트 이름을 통해 퀘스트 파일 로드
                tempQuest = Resources.Load<Quest>(Path.Combine("Data", "Quests", root[i]["QuestName"]));

                // 퀘스트 이름을 통해 퀘스트 파일 로드
                tempQuest.SetState((QuestState)root[i]["QuestState"].AsInt);

                // 완료된 퀘스트인 경우 완료된 퀘스트 리스트에 추가
                if (tempQuest.QuestState == QuestState.Complete)
                {
                    completedQuestList.Add(tempQuest);
                    continue;
                }

                // 현재 퀘스트 업무 인덱스 값 로드
                tempQuest.SetTaskGroupIndex(root[i]["CurrentTaskGroupIndex"].AsInt);
                // 현재 퀘스트 업무 상태 로드
                tempQuest.SetTaskGroupState((TaskGroupState)root[i]["TaskGroupState"].AsInt);

                // 현재 퀘스트 업무가 완료된 상태가 아닌 경우 현재 업무의 완수 횟수 로드
                if (!tempQuest.CurrentTaskGroup.IsComplete)
                {
                    for (int j = 0; j < tempQuest.CurrentTaskGroup.Tasks.Count; j++)
                    {
                        tempQuest.CurrentTaskGroup.Tasks[j].CurrentSuccess = root[i]["TasksSuccessValue"][j];
                    }
                }
                ActivedQuestList.Add(tempQuest);
            }

            return true;
        }

        // 리소스 폴더에서 퀘스트 에셋을 로드하여 반환하는 함수_220524
        public Quest GetQuest(string questName) { return Resources.Load<Quest>(Path.Combine("Data", "Quests", questName)); }
        #endregion

        #region DIALOG DATA
        public void LoadDialogData(ref Dialog.SerializableDicDialog dialogsDic)
        {
            string path = Path.Combine(Application.dataPath, Path.Combine("Resources", "Data", "DialogData")) + ".csv";

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

        // 종료 시 플레이어 데이터 저장_220503
        private void OnApplicationQuit()
        {
            // 데이터 로드가 완료되었을 때만 저장하도록 변경_220513
            if (isCompleteLoad) 
            {
                SavePlayerData(); 
                SaveQuestData(); 
            }
        }
    }
}