using System.Collections;
using UnityEngine;
using UnityEditor;

namespace SK.Data.Eidotr
{
    public class ExportDataEditor : EditorWindow
    {
        private string fileName;
        private string[] tagStr;

        GUILayoutOption[] op_1;

        private int tagIndex;

        [MenuItem("CustomTool/Open Tool")]
        static void Init()
        {
            ExportDataEditor window = (ExportDataEditor)EditorWindow.GetWindow(typeof(ExportDataEditor));
            
            // Fix Window Size
            //ExportDataEditor window = (ExportDataEditor)EditorWindow.GetWindowWithRect(typeof(ExportDataEditor), new Rect(0, 0, 800, 600));

            window.Show();
        }

        private void OnGUI()
        {
            // Select Target Tag
            op_1 = new GUILayoutOption[2];
            op_1[0] = GUILayout.Height(20);
            op_1[1] = GUILayout.Width(180);
            GUILayout.Label("Select Target Object Tag");
            tagStr = UnityEditorInternal.InternalEditorUtility.tags;
            tagIndex = EditorGUILayout.Popup(tagIndex, tagStr, op_1);

            GUILayout.Space(10);

            float buttonSize = EditorGUIUtility.currentViewWidth * 0.2f;
            if (buttonSize < 150) buttonSize = 150; // 버튼 Min Width
            op_1[0] = GUILayout.Height(40);
            op_1[1] = GUILayout.Width(buttonSize);
            GUILayout.BeginHorizontal();
            GUILayout.Space(EditorGUIUtility.currentViewWidth * 0.5f - buttonSize * 0.5f); // 버튼이 중앙에 위치
            if (GUILayout.Button("Export CSV File", op_1))
            {
                fileName = EditorUtility.SaveFilePanel("Save", "Assets/Resources/Data", fileName, "csv");

                if (fileName == string.Empty)
                {
                    Debug.LogError("File name is empty. Fill file name.");
                    return; 
                }

                ExportData.ExportCSVData(fileName, tagIndex);
            }
            GUILayout.EndHorizontal();
        }
    }
}