using UnityEngine;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace SK.Data
{
    [System.Serializable]
    public class Serialization<T>
    {
        [SerializeField] List<T> List;

        public List<T> ToList() { return List; }
        public Serialization(List<T> t) { List = t; }
    }

    [System.Serializable]
    public struct QuestInfo
    {
        public string QuestName;
        public int QuestState;
        public int CurrentTaskGroupIndex;
        public int TaskGroupState;
        public int[] TasksSuccessValue;
    }

    public class DataUtility
    {
        private static StringBuilder stringBuilder = new StringBuilder();
        private static readonly char _comma = ',';
        
        private static List<QuestInfo> questDatas;
        private static string questDataPath;
        private static string skillDataPath;

#if UNITY_EDITOR
        // 오브젝트 정보를 CSV파일로 저장(에디터 전용)_220329
        public static void ExportCSVData(string fileName, int tagIndex)
        {
            string tagName = UnityEditorInternal.InternalEditorUtility.tags[tagIndex];
            GameObject[] objects = GameObject.FindGameObjectsWithTag(tagName);

            // txt 파일 생성
            using (StreamWriter sr = new StreamWriter(fileName)) 
            { 
                sr.Write("Index,Name,xPos,yPos,zPos,xRot,yRot,zRot,xScale,yScale,zScale");
                if (tagName == "Enemy") sr.Write(",isPatrol");
                sr.WriteLine();
                int index = 0;
                foreach (var obj in objects)
                {
                    sr.Write(index++);

                    stringBuilder.Clear();
                    stringBuilder.Append(_comma);
                    stringBuilder.Append(obj.name);
                    sr.Write(stringBuilder.ToString());

                    stringBuilder.Clear();
                    stringBuilder.Append(_comma);
                    stringBuilder.Append(string.Format("{0:0.00}", obj.transform.position.x));
                    sr.Write(stringBuilder.ToString());

                    stringBuilder.Clear();
                    stringBuilder.Append(_comma);
                    stringBuilder.Append(string.Format("{0:0.00}", obj.transform.position.y));
                    sr.Write(stringBuilder.ToString());

                    stringBuilder.Clear();
                    stringBuilder.Append(_comma);
                    stringBuilder.Append(string.Format("{0:0.00}", obj.transform.position.z));
                    sr.Write(stringBuilder.ToString());

                    stringBuilder.Clear();
                    stringBuilder.Append(_comma);
                    stringBuilder.Append(string.Format("{0:0.00}", obj.transform.rotation.eulerAngles.x));
                    sr.Write(stringBuilder.ToString());

                    stringBuilder.Clear();
                    stringBuilder.Append(_comma);
                    stringBuilder.Append(string.Format("{0:0.00}", obj.transform.rotation.eulerAngles.y));
                    sr.Write(stringBuilder.ToString());

                    stringBuilder.Clear();
                    stringBuilder.Append(_comma);
                    stringBuilder.Append(string.Format("{0:0.00}", obj.transform.rotation.eulerAngles.z));
                    sr.Write(stringBuilder.ToString());

                    stringBuilder.Clear();
                    stringBuilder.Append(_comma);
                    stringBuilder.Append(string.Format("{0:0.00}", obj.transform.localScale.x));
                    sr.Write(stringBuilder.ToString());

                    stringBuilder.Clear();
                    stringBuilder.Append(_comma);
                    stringBuilder.Append(string.Format("{0:0.00}", obj.transform.localScale.y));
                    sr.Write(stringBuilder.ToString());

                    stringBuilder.Clear();
                    stringBuilder.Append(_comma);
                    stringBuilder.Append(string.Format("{0:0.00}", obj.transform.localScale.z));
                    sr.Write(stringBuilder.ToString());

                    // Enemy Export
                    if (tagName == "Enemy")
                    {
                        sr.Write("," + obj.GetComponent<Enemy>().isPatrol);
                    }
                    sr.WriteLine();
                }
                sr.Close(); 
            }
            string name = fileName.Substring(fileName.LastIndexOf("/") + 1);
            Debug.Log("Export Complete " + name + " file.");
        }

        // 아이템을 플레이어 CSV 데이터에 추가(데이터 전용)_220508
        public static void AddItem(Item addItem, string amount)
        {
            Debug.Log($"플레이어 아이템 추가: {addItem.ItemName}");

            string path = Path.Combine(Application.dataPath, Path.Combine("Resources", "Data", "Player", "PlayerData")) + ".csv";
            int lastIndex = 0;
            using (StreamReader sr = new StreamReader(path))
            {
                string line = string.Empty;

                while ((line = sr.ReadLine()) != null)
                {
                    string[] row = line.Split(new char[] { _comma });
                    if (row[0] == "Index")
                        lastIndex = 0;
                    else
                        lastIndex++;
                }
                sr.Close();
            }
            Debug.Log($"마지막 인덱스: {lastIndex}");

            using (StreamWriter sr = new StreamWriter(path, true))
            {
                // 인벤토리 아이템 정보 추가
                sr.WriteLine();
                sr.Write(lastIndex);
                sr.Write(_comma + addItem.Id.ToString());
                int itemtype = (int)addItem.ItemType;
                Debug.Log("itemtype: " + itemtype);
                sr.Write(_comma + itemtype.ToString());
                int equiptype = (int)addItem.EquipmentType;
                Debug.Log("equiptype: " + equiptype);
                sr.Write(_comma + equiptype.ToString());
                sr.Write(_comma + amount);
                sr.Write(_comma + "-1");
                sr.Write(_comma + "0");
                sr.Close();
            }
        }
#endif

        // 플레이어 정보를 CSV파일로 저장_220504
        public static void ExportPlayerData(PlayerData playerData, PlayerItemData itemData, Transform playerTransform)
        {
            string path = Path.Combine(Application.dataPath, Path.Combine("Resources", "Data", "Player", "PlayerData")) + ".csv";

            using (StreamWriter sr = new StreamWriter(path))
            {
                sr.Write("Index,Name,Level,Exp,Gold,Gem,Hp,Mp,Sp,Str,Dex,Int,SkillPoint,StatPoint,AttackSpeed,CriticalChance,CriticalMultiplier,Def,Speed,Avoidance,RecoverHp,RecoverMp,RecoverSp,xPos,yPos,zPos,RecentLocation");
                sr.WriteLine();

                // 플레이어 정보 저장
                stringBuilder.Clear();
                stringBuilder.Append(_comma);
                stringBuilder.Append(playerData.name);
                stringBuilder.Append(_comma);
                stringBuilder.Append(playerData.Level);
                stringBuilder.Append(_comma);
                stringBuilder.Append(playerData.Exp);
                stringBuilder.Append(_comma);
                stringBuilder.Append(playerData.Gold);
                stringBuilder.Append(_comma);
                stringBuilder.Append(playerData.Gem);
                stringBuilder.Append(_comma);
                stringBuilder.Append(playerData.Hp);
                stringBuilder.Append(_comma);
                stringBuilder.Append(playerData.Mp);
                stringBuilder.Append(_comma);
                stringBuilder.Append(playerData.Sp);
                stringBuilder.Append(_comma);
                stringBuilder.Append(playerData.Str);
                stringBuilder.Append(_comma);
                stringBuilder.Append(playerData.Dex);
                stringBuilder.Append(_comma);
                stringBuilder.Append(playerData.Int);
                stringBuilder.Append(_comma);
                stringBuilder.Append(playerData.SkillPoint);
                stringBuilder.Append(_comma);
                stringBuilder.Append(playerData.StatPoint);
                stringBuilder.Append(_comma);
                stringBuilder.Append(playerData.AttackSpeed);
                stringBuilder.Append(_comma);
                stringBuilder.Append(playerData.CriticalChance);
                stringBuilder.Append(_comma);
                stringBuilder.Append(playerData.CriticalMultiplier);
                stringBuilder.Append(_comma);
                stringBuilder.Append(playerData.Def);
                stringBuilder.Append(_comma);
                stringBuilder.Append(playerData.Speed);
                stringBuilder.Append(_comma);
                stringBuilder.Append(playerData.Avoidance);
                stringBuilder.Append(_comma);
                stringBuilder.Append(playerData.RecoverHp);
                stringBuilder.Append(_comma);
                stringBuilder.Append(playerData.RecoverMp);
                stringBuilder.Append(_comma);
                stringBuilder.Append(playerData.RecoverSp);
                stringBuilder.Append(_comma);
                stringBuilder.Append(string.Format("{0:0.00}", playerTransform.position.x));
                stringBuilder.Append(_comma);
                stringBuilder.Append(string.Format("{0:0.00}", playerTransform.position.y));
                stringBuilder.Append(_comma);
                stringBuilder.Append(string.Format("{0:0.00}", playerTransform.position.z));
                stringBuilder.Append(_comma);
                stringBuilder.Append((int)SceneManager.Instance.locationManager.CurrentLocation);
                sr.Write(stringBuilder.ToString());
                sr.WriteLine();

                // 인벤토리 아이템 정보 저장
                sr.Write("Index,ItemID,ItemType,EquipType,Amount,SlotID,IsEquiped");
                if (itemData.items.Count > 0)
                {
                    int index = 0;

                    foreach (var item in itemData.items)
                    {
                        sr.WriteLine();
                        sr.Write(index++);

                        stringBuilder.Clear();
                        stringBuilder.Append(_comma);
                        stringBuilder.Append(item.item.Id);
                        sr.Write(stringBuilder.ToString());

                        stringBuilder.Clear();
                        stringBuilder.Append(_comma);
                        stringBuilder.Append((int)item.item.ItemType);
                        sr.Write(stringBuilder.ToString());

                        stringBuilder.Clear();
                        stringBuilder.Append(_comma);
                        stringBuilder.Append((int)item.item.EquipmentType);
                        sr.Write(stringBuilder.ToString());

                        stringBuilder.Clear();
                        stringBuilder.Append(_comma);
                        stringBuilder.Append(item.amount);
                        sr.Write(stringBuilder.ToString());

                        stringBuilder.Clear();
                        stringBuilder.Append(_comma);
                        stringBuilder.Append(item.slotID);
                        sr.Write(stringBuilder.ToString());

                        stringBuilder.Clear();
                        stringBuilder.Append(_comma);
                        stringBuilder.Append((item.isEquiped ? 1 : 0));
                        sr.Write(stringBuilder.ToString());
                    }
                }
                sr.Close();
            }
        }

        // 퀘스트 정보를 Json파일로 저장_220524
        public static void ExportQuestsData(List<Quests.Quest> activatedQuests, List<Quests.Quest> completedQuests)
        {
            void AddQuestData(List<Quests.Quest> quests)
            {
                if (questDatas == null)
                    questDatas = new List<QuestInfo>();

                QuestInfo tempData;

                foreach (var quest in quests)
                {
                    tempData = new QuestInfo();
                    tempData.QuestName = quest.name;
                    tempData.QuestState = (int)quest.QuestState;
                    tempData.CurrentTaskGroupIndex = quest.CurrentTaskGroupIndex;
                    tempData.TaskGroupState = (int)quest.CurrentTaskGroup.State;

                    // 퀘스트의 업무가 완료된 상태가 아닌 경우 각 업무의 완수 수량을 저장
                    if (quest.CurrentTaskGroup.State == Quests.TaskGroupState.Running)
                    {
                        // 업무 수량에 따라 배열 초기화
                        tempData.TasksSuccessValue = new int[quest.CurrentTaskGroup.Tasks.Count];
                        for (int i = 0; i < quest.CurrentTaskGroup.Tasks.Count; i++)
                            tempData.TasksSuccessValue[i] = quest.CurrentTaskGroup.Tasks[i].CurrentSuccess;
                    }

                    questDatas.Add(tempData);
                }
            }

            // 현재 활성화된 퀘스트 정보 저장
            if (activatedQuests.Count > 0) 
                AddQuestData(activatedQuests);

            // 완료된 퀘스트 정보 저장
            if (completedQuests.Count > 0) 
                AddQuestData(completedQuests);

            Serialization<QuestInfo> tempInst = new Serialization<QuestInfo>(questDatas);
            string questJsonData = JsonUtility.ToJson(tempInst);

            // 데이터 파일 경로 초기화
            if (string.IsNullOrEmpty(questDataPath))
                questDataPath = Path.Combine(Application.dataPath, Path.Combine("Resources", "Data", "QuestJsonData")) + ".json";

            // 파일이 있는지 확인 후 없으면 생성
            if (!File.Exists(questDataPath))
            {                
                using (File.Create(questDataPath))
                {
                    Debug.Log("퀘스트 데이터 파일 생성 완료");
                }
            }

            // Json 확장자 파일에 Json 데이터 쓰기
            File.WriteAllText(questDataPath, questJsonData);
        }

        // 스킬 정보를 Json파일로 저장_220620
        public static void ExportSkillData()
        {
            // 활성화된 스킬데이터의 인스턴스ID를 저장할 리스트
            List<int> skillIDList = new List<int>();
            // 스킬 데이터 딕셔너리를 통해 활성화된 스킬의 인스턴스ID를 리스트에 저장
            foreach (KeyValuePair<int, UI.SkillSlot> SkillSlotDic in UI.UIManager.Instance.skillManager.SkillSlotDic)
            {
                if (SkillSlotDic.Value.IsActivated)
                    skillIDList.Add(SkillSlotDic.Value.SkillData.skillID);
            }
            Serialization<int> tempInst = new Serialization<int>(skillIDList);
            string skillJsonData = JsonUtility.ToJson(tempInst);

            // 데이터 파일 경로 초기화
            if (string.IsNullOrEmpty(skillDataPath))
                skillDataPath = Path.Combine(Application.dataPath, Path.Combine("Resources", "Data", "SkillJsonData")) + ".json";

            // 파일이 있는지 확인 후 없으면 생성
            if (!File.Exists(skillDataPath))
            {
                using (File.Create(skillDataPath))
                {
                    Debug.Log("스킬 데이터 파일 생성 완료");
                }
            }

            // Json 확장자 파일에 Json 데이터 쓰기
            File.WriteAllText(skillDataPath, skillJsonData);
        }
    }
}