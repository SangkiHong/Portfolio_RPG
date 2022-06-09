using UnityEngine;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace SK.Data
{
    [System.Serializable]
    public class SerializationQuestInfo<T>
    {
        [SerializeField] List<T> QuestInfo;

        public List<T> ToList() { return QuestInfo; }
        public SerializationQuestInfo(List<T> t) { QuestInfo = t; }
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

#if UNITY_EDITOR
        // 오브젝트 정보 CSV파일로 저장(에디터 전용)_220329
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
#endif

        // 플레이어 정보 CSV파일로 저장_220504
        public static void ExportPlayerData(PlayerData playerData, PlayerItemData itemData, Transform playerTransform)
        {
            string path = Path.Combine(Application.dataPath, Path.Combine("Resources", "Data", "Player", "PlayerData")) + ".csv";
            using (StreamWriter sr = new StreamWriter(path))
            {
                sr.Write("Index,Name,Level,Exp,MaxHp,MaxMp,MaxSp,Str,Dex,Int,AttackSpeed,CriticalChance,CriticalMultiplier,Def,Speed,Avoidance,RecoverHp,RecoverMp,RecoverSp,xPos,yPos,zPos");
                sr.WriteLine();

                // 플레이어 정보 저장
                stringBuilder.Clear();
                stringBuilder.Append(_comma);
                stringBuilder.Append(playerData.name);
                sr.Write(stringBuilder.ToString());

                stringBuilder.Clear();
                stringBuilder.Append(_comma);
                stringBuilder.Append(playerData.Level);
                sr.Write(stringBuilder.ToString());

                stringBuilder.Clear();
                stringBuilder.Append(_comma);
                stringBuilder.Append(playerData.Exp);
                sr.Write(stringBuilder.ToString());

                stringBuilder.Clear();
                stringBuilder.Append(_comma);
                stringBuilder.Append(playerData.MaxHp);
                sr.Write(stringBuilder.ToString());

                stringBuilder.Clear();
                stringBuilder.Append(_comma);
                stringBuilder.Append(playerData.MaxMp);
                sr.Write(stringBuilder.ToString());

                stringBuilder.Clear();
                stringBuilder.Append(_comma);
                stringBuilder.Append(playerData.MaxSp);
                sr.Write(stringBuilder.ToString());

                stringBuilder.Clear();
                stringBuilder.Append(_comma);
                stringBuilder.Append(playerData.Str);
                sr.Write(stringBuilder.ToString());

                stringBuilder.Clear();
                stringBuilder.Append(_comma);
                stringBuilder.Append(playerData.Dex);
                sr.Write(stringBuilder.ToString());

                stringBuilder.Clear();
                stringBuilder.Append(_comma);
                stringBuilder.Append(playerData.Int);
                sr.Write(stringBuilder.ToString());

                stringBuilder.Clear();
                stringBuilder.Append(_comma);
                stringBuilder.Append(playerData.AttackSpeed);
                sr.Write(stringBuilder.ToString());

                stringBuilder.Clear();
                stringBuilder.Append(_comma);
                stringBuilder.Append(playerData.CriticalChance);
                sr.Write(stringBuilder.ToString());

                stringBuilder.Clear();
                stringBuilder.Append(_comma);
                stringBuilder.Append(playerData.CriticalMultiplier);
                sr.Write(stringBuilder.ToString());

                stringBuilder.Clear();
                stringBuilder.Append(_comma);
                stringBuilder.Append(playerData.Def);
                sr.Write(stringBuilder.ToString());

                stringBuilder.Clear();
                stringBuilder.Append(_comma);
                stringBuilder.Append(playerData.Speed);
                sr.Write(stringBuilder.ToString());

                stringBuilder.Clear();
                stringBuilder.Append(_comma);
                stringBuilder.Append(playerData.Avoidance);
                sr.Write(stringBuilder.ToString());

                stringBuilder.Clear();
                stringBuilder.Append(_comma);
                stringBuilder.Append(playerData.RecoverHp);
                sr.Write(stringBuilder.ToString());

                stringBuilder.Clear();
                stringBuilder.Append(_comma);
                stringBuilder.Append(playerData.RecoverMp);
                sr.Write(stringBuilder.ToString());

                stringBuilder.Clear();
                stringBuilder.Append(_comma);
                stringBuilder.Append(playerData.RecoverSp);
                sr.Write(stringBuilder.ToString());

                stringBuilder.Clear();
                stringBuilder.Append(_comma);
                stringBuilder.Append(string.Format("{0:0.00}", playerTransform.position.x));
                sr.Write(stringBuilder.ToString());

                stringBuilder.Clear();
                stringBuilder.Append(_comma);
                stringBuilder.Append(string.Format("{0:0.00}", playerTransform.position.y));
                sr.Write(stringBuilder.ToString());

                stringBuilder.Clear();
                stringBuilder.Append(_comma);
                stringBuilder.Append(string.Format("{0:0.00}", playerTransform.position.z));
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
                        stringBuilder.Append(item.item.id);
                        sr.Write(stringBuilder.ToString());

                        stringBuilder.Clear();
                        stringBuilder.Append(_comma);
                        stringBuilder.Append((int)item.item.itemType);
                        sr.Write(stringBuilder.ToString());

                        stringBuilder.Clear();
                        stringBuilder.Append(_comma);
                        stringBuilder.Append((int)item.item.equipmentType);
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

        // 퀘스트 정보 Json파일로 저장_220524
        public static void ExportQuestsData(ref List<Quests.Quest> quests)
        {
            // 저장할 퀘스트 데이터가 없는 경우
            if (quests.Count == 0) return;

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

            SerializationQuestInfo<QuestInfo> tempInst = new SerializationQuestInfo<QuestInfo>(questDatas);
            string questJsonData = JsonUtility.ToJson(tempInst);

            File.WriteAllText(Path.Combine(Application.dataPath, Path.Combine("Resources", "Data", "QuestJsonData")) + ".json", questJsonData);
        }

#if UNITY_EDITOR
        // 아이템을 플레이어 CSV 데이터에 추가(데이터 전용)_220508
        public static void AddItem(Item addItem)
        {
            string path = Path.Combine(Application.dataPath, Path.Combine("Resources", "Data", "Player", "PlayerData")) + ".csv";
            using (StreamWriter sr = new StreamWriter(path, true))
            {
                // 인벤토리 아이템 정보 추가
                sr.WriteLine();
                sr.Write(_comma + addItem.id);
                sr.Write(_comma + (int)addItem.itemType);
                sr.Write(_comma + (int)addItem.equipmentType);
                sr.Write(_comma + 1);
                sr.Write(_comma + -1);
                sr.Write(_comma + 0);
                sr.Close();
            }
        }
#endif
    }
}