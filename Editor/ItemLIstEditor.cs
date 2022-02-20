using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace Sangki
{
    [CustomEditor(typeof(ItemList))]
    public class ItemLIstEditor : EditorWindow
    {
        public static ItemLIstEditor Instance;
        private PopupContent m_PopupContent;

        public ItemList selectedItemList;
        private ItemList[] itemLists;

        public string listName, findItemText;
        private string rootPath = "Data/ItemList/";
        private int seletectedListIndex = 999;
        public int popupIndex, viewIndex = 1;

        [MenuItem("Item List/Open Editor %#e")]
        static void Init()
        {
            ItemLIstEditor window = (ItemLIstEditor)EditorWindow.GetWindowWithRect(typeof(ItemLIstEditor), new Rect(Screen.width * 0.7f, Screen.height * 0.6f, 500f, 800f));
        }

        public void OnEnable()
        {
            Instance = this;

            m_PopupContent = new PopupContent();

            LoadLists();
        }

        void OnGUI()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Item List Editor", EditorStyles.boldLabel);
            if (selectedItemList != null)
            {
                if (GUILayout.Button("Show List"))
                {
                    EditorUtility.FocusProjectWindow();
                    Selection.activeObject = selectedItemList;
                }
                if (GUILayout.Button("Close List"))
                {
                    Selection.activeObject = null;
                    selectedItemList = null;
                }
            }
            else
            {
                if (seletectedListIndex != 999)
                {
                    if (GUILayout.Button("Open"))
                    {
                        EditorUtility.FocusProjectWindow();
                        selectedItemList = itemLists[seletectedListIndex];
                        seletectedListIndex = 999;
                    }
                    if (GUILayout.Button("Delete"))
                    {
                        EditorUtility.FocusProjectWindow();
                        FileUtil.DeleteFileOrDirectory("Assets/Resources/Data/ItemList/" + itemLists[seletectedListIndex].name + ".asset");
                        AssetDatabase.Refresh();
                        LoadLists();
                        seletectedListIndex = 999;
                    }
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (selectedItemList != null)
            {
                Rect buttonRect = new Rect(EditorGUIUtility.fieldWidth - 100,  EditorGUIUtility.singleLineHeight, 100, 30);

                if (GUILayout.Button("Find Item", GUILayout.ExpandWidth(false)))
                {
                    EditorUtility.FocusProjectWindow();
                    popupIndex = 1;
                    PopupWindow.Show(GUILayoutUtility.GetLastRect(), m_PopupContent);
                }
            }
            GUILayout.EndHorizontal();

            if (selectedItemList == null)
            {
                GUILayout.BeginHorizontal();
                if (itemLists != null)
                {
                    GUILayout.BeginHorizontal("Box");
                    for (int i = 0; i < itemLists.Length; i++)
                    {
                        if (GUILayout.Button(itemLists[i].name, EditorStyles.toolbarButton))
                        {
                            seletectedListIndex = i;
                        }
                    }
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Space(10);
                if (GUILayout.Button("Create New Item List", GUILayout.ExpandWidth(false)))
                {
                    popupIndex = 0;
                    PopupWindow.Show(GUILayoutUtility.GetLastRect(), m_PopupContent);
                }
                if (GUILayout.Button("Open Existing Item List", GUILayout.ExpandWidth(false)))
                {
                    OpenItemList();
                }
                GUILayout.EndHorizontal();
            }

            GUILayout.Space(5);
            GuiLine();
            GUILayout.Space(5);
            if (selectedItemList != null)
            {
                GUILayout.BeginHorizontal();

                GUILayout.Space(10);

                if (GUILayout.Button(" << ", GUILayout.ExpandWidth(false)))
                {
                    viewIndex = 1;
                }
                GUILayout.Space(5);
                if (GUILayout.Button(" < ", GUILayout.ExpandWidth(false)))
                {
                    if (viewIndex > 1)
                        viewIndex--;
                }
                GUILayout.Space(5);
                if (GUILayout.Button(" > ", GUILayout.ExpandWidth(false)))
                {
                    if (viewIndex < selectedItemList.itemList.Count)
                    {
                        viewIndex++;
                    }
                }
                GUILayout.Space(5);
                if (GUILayout.Button(" >> ", GUILayout.ExpandWidth(false)))
                {
                    viewIndex = selectedItemList.itemList.Count;
                }

                GUILayout.Space(200);

                if (GUILayout.Button("Add Item", GUILayout.ExpandWidth(false)))
                {
                    AddItem();
                }
                if (GUILayout.Button("Delete Item", GUILayout.ExpandWidth(false)))
                {
                    DeleteItem(viewIndex - 1);
                }

                GUILayout.EndHorizontal();

                GUILayout.Space(5);
                GuiLine();
                GUILayout.Space(5);

                if (selectedItemList.itemList == null) Debug.Log("List is empty");

                if (selectedItemList.itemList.Count > 0)
                {
                    GUILayout.BeginHorizontal();
                    viewIndex = Mathf.Clamp(EditorGUILayout.IntField("Current Item", viewIndex, GUILayout.ExpandWidth(false)), 1, selectedItemList.itemList.Count);
                    //Mathf.Clamp (viewIndex, 1, itemList.itemList.Count);
                    EditorGUILayout.LabelField("of   " + selectedItemList.itemList.Count.ToString() + "  items", "", GUILayout.ExpandWidth(false));
                    GUILayout.EndHorizontal();

                    GUILayout.BeginVertical("Box");
                    selectedItemList.itemList[viewIndex - 1].id = EditorGUILayout.IntField("ID", selectedItemList.itemList[viewIndex - 1].id, GUILayout.ExpandWidth(false));
                    selectedItemList.itemList[viewIndex - 1].itemName = EditorGUILayout.TextField("Item Name", selectedItemList.itemList[viewIndex - 1].itemName as string);
                    selectedItemList.itemList[viewIndex - 1].itemType = (ItemType)EditorGUILayout.EnumPopup("Item Type ", selectedItemList.itemList[viewIndex - 1].itemType);
                    if (selectedItemList.itemList[viewIndex - 1].itemType == ItemType.Equipment)
                        selectedItemList.itemList[viewIndex - 1].equipmentType = (EquipmentType)EditorGUILayout.EnumPopup("Equipment Type ", selectedItemList.itemList[viewIndex - 1].equipmentType);
                    selectedItemList.itemList[viewIndex - 1].itemGrade = (ItemGrade)EditorGUILayout.EnumPopup("Item Grade ", selectedItemList.itemList[viewIndex - 1].itemGrade);
                    selectedItemList.itemList[viewIndex - 1].itemIcon = EditorGUILayout.ObjectField("Item Icon", selectedItemList.itemList[viewIndex - 1].itemIcon, typeof(Texture2D), false) as Texture2D;
                    GUILayout.EndVertical();
                    GUILayout.Space(10);

                    GUILayout.BeginVertical("Box");

                    GUILayout.BeginHorizontal();
                    if (selectedItemList.itemList[viewIndex - 1].itemType != ItemType.Equipment)
                    {
                        GUILayout.BeginVertical();
                        selectedItemList.itemList[viewIndex - 1].isConsumable = (bool)EditorGUILayout.Toggle("Consumable", selectedItemList.itemList[viewIndex - 1].isConsumable, GUILayout.ExpandWidth(false));
                        GUILayout.EndVertical();
                    }
                    if (selectedItemList.itemList[viewIndex - 1].itemType != ItemType.Equipment)
                    {
                        GUILayout.BeginVertical();
                        selectedItemList.itemList[viewIndex - 1].isStackable = (bool)EditorGUILayout.Toggle("Stackable", selectedItemList.itemList[viewIndex - 1].isStackable, GUILayout.ExpandWidth(false));
                        GUILayout.EndVertical();
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.BeginVertical();
                    selectedItemList.itemList[viewIndex - 1].requiredLevel = EditorGUILayout.IntField("Required Level ", selectedItemList.itemList[viewIndex - 1].requiredLevel, GUILayout.ExpandWidth(false));
                    GUILayout.EndVertical();
                    GUILayout.EndHorizontal();

                    GUILayout.EndVertical();

                    GUILayout.Space(10);

                    GUILayout.Label("Item Using Effect");
                    GUILayout.BeginVertical("Box");

                    if (selectedItemList.itemList[viewIndex - 1].itemType == ItemType.Equipment)
                    {
                        GUILayout.BeginHorizontal();
                        selectedItemList.itemList[viewIndex - 1].stat_Str = EditorGUILayout.IntField("Stat Str ", selectedItemList.itemList[viewIndex - 1].stat_Str, GUILayout.ExpandWidth(false));
                        selectedItemList.itemList[viewIndex - 1].stat_Dex = EditorGUILayout.IntField("Stat Dex ", selectedItemList.itemList[viewIndex - 1].stat_Dex, GUILayout.ExpandWidth(false));
                        GUILayout.EndHorizontal();
                        GUILayout.BeginHorizontal();
                        selectedItemList.itemList[viewIndex - 1].stat_Int = EditorGUILayout.IntField("Stat Int ", selectedItemList.itemList[viewIndex - 1].stat_Int, GUILayout.ExpandWidth(false));
                        GUILayout.EndHorizontal();
                    }

                    if (selectedItemList.itemList[viewIndex - 1].itemType == ItemType.Food)
                    {
                        GUILayout.BeginHorizontal();
                        selectedItemList.itemList[viewIndex - 1].recoverHP = EditorGUILayout.IntField("recover HP ", selectedItemList.itemList[viewIndex - 1].recoverHP, GUILayout.ExpandWidth(false));
                        GUILayout.EndHorizontal();
                    }

                    if (selectedItemList.itemList[viewIndex - 1].itemType == ItemType.Posion)
                    {
                        GUILayout.BeginHorizontal();
                        selectedItemList.itemList[viewIndex - 1].buff_Str = EditorGUILayout.IntField("Buff Str ", selectedItemList.itemList[viewIndex - 1].buff_Str, GUILayout.ExpandWidth(false));
                        selectedItemList.itemList[viewIndex - 1].buff_Dex = EditorGUILayout.IntField("Buff Dex ", selectedItemList.itemList[viewIndex - 1].buff_Dex, GUILayout.ExpandWidth(false));
                        GUILayout.EndHorizontal();
                        GUILayout.BeginHorizontal();
                        selectedItemList.itemList[viewIndex - 1].buff_Int = EditorGUILayout.IntField("Buff Int ", selectedItemList.itemList[viewIndex - 1].buff_Int, GUILayout.ExpandWidth(false));
                        GUILayout.EndHorizontal();
                    }

                    GUILayout.EndVertical();
                }
                else
                {
                    GUILayout.Label("This Item List is Empty.");
                }
            }
            if (GUI.changed && selectedItemList)
            {
                EditorUtility.SetDirty(selectedItemList);
            }
        }

        void LoadLists()
        {
            var assets = Resources.LoadAll(rootPath, typeof(ItemList));

            if (assets != null)
                itemLists = Array.ConvertAll(assets, item => item as ItemList);
        }

        public void CreateNewItemList()
        {
            // There is no overwrite protection here!
            // There is No "Are you sure you want to overwrite your existing object?" if it exists.
            // This should probably get a string from the user to create a new name and pass it ...
            viewIndex = 1;
            
            selectedItemList = CreateItemList.Create(listName);
            if (selectedItemList)
            {
                selectedItemList.itemList = new List<Item>();
                selectedItemList.itemList.Add(new Item());
                string relPath = AssetDatabase.GetAssetPath(selectedItemList);
                EditorPrefs.SetString("ObjectPath", relPath);
            }
            LoadLists();
        }

        void OpenItemList()
        {
            string absPath = EditorUtility.OpenFilePanel("Select Item List", "", "");
            if (absPath.StartsWith(Application.dataPath))
            {
                string relPath = absPath.Substring(Application.dataPath.Length - "Assets".Length);
                selectedItemList = AssetDatabase.LoadAssetAtPath(relPath, typeof(ItemList)) as ItemList;
                if (selectedItemList.itemList == null)
                    selectedItemList.itemList = new List<Item>();
                if (selectedItemList)
                {
                    EditorPrefs.SetString("ObjectPath", relPath);
                }
            }
        }

        void AddItem()
        {
            Item newItem = new Item();
            newItem.itemName = "New Item";
            newItem.id = selectedItemList.itemList.Count > 0 ? selectedItemList.itemList.Count : 0;
            selectedItemList.itemList.Add(newItem);
            viewIndex = selectedItemList.itemList.Count;
        }

        void DeleteItem(int index)
        {
            selectedItemList.itemList.RemoveAt(index);
        }
        void GuiLine(int i_height = 1)
        {
            Rect rect = EditorGUILayout.GetControlRect(false, i_height);
            rect.height = i_height;
            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1));
        }
    }

    public class PopupContent : PopupWindowContent
    {
        public override Vector2 GetWindowSize()
        {
            return new Vector2(350, 100);
        }

        public override void OnGUI(Rect rect)
        {
            GUILayout.Space(10);

            if (ItemLIstEditor.Instance.popupIndex == 0)
            {
                ItemLIstEditor.Instance.listName = EditorGUILayout.TextField("리스트 이름을 입력하세요: ", ItemLIstEditor.Instance.listName);
                
                if (GUILayout.Button("새로운 리스트 생성", GUILayout.ExpandWidth(false)))
                {
                    if (ItemLIstEditor.Instance.listName.Length > 0)
                    {
                        ItemLIstEditor.Instance.CreateNewItemList();
                        ItemLIstEditor.Instance.listName = string.Empty;
                        editorWindow.Close();
                    }
                    else
                        Debug.Log("리스트의 이름을 입력하세요.");
                }
            }
            else
            {
                ItemLIstEditor.Instance.findItemText = EditorGUILayout.TextField("검색할 아이템 : ", ItemLIstEditor.Instance.findItemText);

                if (GUILayout.Button("검색", GUILayout.ExpandWidth(false)))
                {
                    if (ItemLIstEditor.Instance.findItemText.Length > 0)
                    {
                        if (!FindItem(ItemLIstEditor.Instance.findItemText))
                            Debug.Log("리스트에 해당 아이템이 검색되지 않습니다.");
                    }
                    else
                        Debug.Log("검색할 아이템 이름을 입력하세요.");
                }
            }
            if (GUI.changed)
            {
                EditorUtility.SetDirty(ItemLIstEditor.Instance);
            }
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                editorWindow.Close();
            }
        }

        public override void OnClose()
        {
            base.OnClose();
            ItemLIstEditor.Instance.findItemText = string.Empty;
            ItemLIstEditor.Instance.listName = string.Empty;
        }

        private bool FindItem(string itemName)
        {
            for (int i = 0; i < ItemLIstEditor.Instance.selectedItemList.itemList.Count; i++)
            {
                // 값 비교를 위해 양쪽 모두 Lower로 변환하여 비교 연산
                if (ItemLIstEditor.Instance.selectedItemList.itemList[i].itemName.ToLower().Contains(itemName.ToLower()))
                {
                    editorWindow.Close();
                    ItemLIstEditor.Instance.viewIndex = i + 1;
                    return true;
                }
            }
            return false;
        }
    }
}
