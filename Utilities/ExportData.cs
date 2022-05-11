using UnityEngine;
using System.IO;

namespace SK.Data
{
    public class ExportData
    {
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
                    sr.Write("," + obj.name);
                    sr.Write("," + obj.transform.position.x);
                    sr.Write("," + obj.transform.position.y);
                    sr.Write("," + obj.transform.position.z);
                    sr.Write("," + obj.transform.rotation.eulerAngles.x);
                    sr.Write("," + obj.transform.rotation.eulerAngles.y);
                    sr.Write("," + obj.transform.rotation.eulerAngles.z);
                    sr.Write("," + obj.transform.localScale.x );
                    sr.Write("," + obj.transform.localScale.y);
                    sr.Write("," + obj.transform.localScale.z);

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

        // 플레이어 정보 CSV파일로 저장_220504
        public static void ExportPlayerData(PlayerData playerData, PlayerItemData itemData, Transform playerTransform)
        {
            string path = Path.Combine(Application.dataPath, Path.Combine("Resources", "Data", "PlayerData")) + ".csv";
            using (StreamWriter sr = new StreamWriter(path))
            {
                sr.Write("Name,Level,Exp,MaxHp,MaxMp,MaxSp,Str,Dex,Int,AttackSpeed,CriticalChance,CriticalMultiplier,Armor,Speed,Avoidance,RecoverHp,RecoverMp,RecoverSp,xPos,yPos,zPos");
                sr.WriteLine();

                // 플레이어 정보 저장
                sr.Write(playerData.Name);
                sr.Write("," + playerData.Level);
                sr.Write("," + playerData.Exp);
                sr.Write("," + playerData.MaxHp);
                sr.Write("," + playerData.MaxMp);
                sr.Write("," + playerData.MaxSp);
                sr.Write("," + playerData.Str);
                sr.Write("," + playerData.Dex);
                sr.Write("," + playerData.Int);
                sr.Write("," + playerData.AttackSpeed);
                sr.Write("," + playerData.CriticalChance);
                sr.Write("," + playerData.CriticalMultiplier);
                sr.Write("," + playerData.Armor);
                sr.Write("," + playerData.Speed);
                sr.Write("," + playerData.Avoidance);
                sr.Write("," + playerData.RecoverHp);
                sr.Write("," + playerData.RecoverMp);
                sr.Write("," + playerData.RecoverSp);
                sr.Write("," + playerTransform.position.x);
                sr.Write("," + playerTransform.position.y);
                sr.Write("," + playerTransform.position.z);
                sr.WriteLine();

                // 인벤토리 아이템 정보 저장
                sr.Write("ItemID,ItemType,EquipType,Amount,SlotID");
                if (itemData.items.Count > 0)
                {
                    foreach (var item in itemData.items)
                    {
                        sr.WriteLine();
                        sr.Write(item.item.id);
                        sr.Write("," + (int)item.item.itemType);
                        sr.Write("," + (int)item.item.equipmentType);
                        sr.Write("," + item.amount);
                        sr.Write("," + item.slotID);
                    }
                }
                sr.Close();
            }
        }

        public static void AddItem(Item addItem)
        {
            string path = Path.Combine(Application.dataPath, Path.Combine("Resources", "Data", "PlayerData")) + ".csv";
            using (StreamWriter sr = new StreamWriter(path, true))
            {
                // 인벤토리 아이템 정보 추가
                sr.WriteLine();
                sr.Write(addItem.id);
                sr.Write("," + (int)addItem.itemType);
                sr.Write("," + (int)addItem.equipmentType);
                sr.Write("," + 1);
                sr.Write("," + -1);
                sr.Close();
            }
        }
    }
}