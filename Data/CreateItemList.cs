using UnityEngine;
using UnityEditor;
using System.IO;

namespace SK
{
    public class CreateItemList
    {
        #if UNITY_EDITOR
        [MenuItem("Assets/Create/Item List")]
        public static ItemList Create(string listName)
        { 
            ItemList asset = ScriptableObject.CreateInstance<ItemList>();

            // 디렉토리 확인 후 없을 시 생성
            DirectoryInfo di = new DirectoryInfo("Assets/Resources/Data/ItemList");
            if (!di.Exists) di.Create();
            
            AssetDatabase.CreateAsset(asset, "Assets/Resources/Data/ItemList/" + listName + ".asset");
            AssetDatabase.SaveAssets();
            return asset;
        }
        #endif
    }
}