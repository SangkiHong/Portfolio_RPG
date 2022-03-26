using System.IO;
using UnityEngine;

namespace SK
{
    public class DataParser : MonoBehaviour
    {
        [SerializeField] private string fileName;

        void Awake()
        {
            Parse(fileName);
        }

        private void Parse(string csvFileName)
        {
            //StreamReader csvData = new StreamReader(Path.Combine("Assets", "Resources", "Data", csvFileName + ".csv"));
            TextAsset csvData = Resources.Load<TextAsset>(Path.Combine("Data", csvFileName)); // csv���� ������

            GameObject parent = new GameObject(); // Instantiate Parent Object
            parent.name = fileName;

            if (csvData == null)
            {
                Debug.LogError("Failed Load CSV Data.");
                return;
            }

            string[] data = csvData.text.Split(new char[] { '\n' }); // ���� �������� Split

            for (int i = 1; i < data.Length - 1; i++) // �� ��° ��(Index 1)���� Data Read
            {
                string[] row = data[i].Split(new char[] { ',' }); // ,������ Split

                GameObject obj = Resources.Load(Path.Combine("Prefabs", row[1])) as GameObject;
                obj = Instantiate(obj, parent.transform);
                obj.transform.position = new Vector3(float.Parse(row[2]), float.Parse(row[3]), float.Parse(row[4]));
                obj.transform.localRotation = Quaternion.Euler(float.Parse(row[5]), float.Parse(row[6]), float.Parse(row[7]));
                obj.transform.localScale = new Vector3(float.Parse(row[8]), float.Parse(row[9]), float.Parse(row[10]));
            }
        }
    }
}
