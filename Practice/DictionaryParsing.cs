using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace SK.Practice
{
    public struct Item
    {
        public int id;
        public string name;
        public int data1;
        public int data2;
        public int data3;
        public int data4;
        public int data5;
        public int data6;
        public int data7;
        public int data8;
        public int data9;
        public int data10;
        public int data11;
    }

    public class DictionaryParsing : MonoBehaviour
    {
        [SerializeField] private string dataName;

        Dictionary<int, Item> items;

        private void Start()
        {
            items = new Dictionary<int, Item>();

            //LoadItems();
            LoadItemsByTextAsset();
            PrintItems();
        }

        private void LoadItems()
        {
            string path = Path.Combine(Application.dataPath, Path.Combine("Resources", "Table", dataName)) + ".csv";

            using (StreamReader sr = new StreamReader(path))
            {
                string line = string.Empty;

                while ((line = sr.ReadLine()) != null)
                {
                    string[] row = line.Split(new char[] { ',' }); // ,단위로 Split
                    if (row[0] == "Index") continue;

                    Item tmpItem = new Item
                    {
                        id = int.Parse(row[0]),
                        name = row[1],
                        data1 = int.Parse(row[2]),
                        data2 = int.Parse(row[3]),
                        data3 = int.Parse(row[4]),
                        data4 = int.Parse(row[5]),
                        data5 = int.Parse(row[6]),
                        data6 = int.Parse(row[7]),
                        data7 = int.Parse(row[8]),
                        data8 = int.Parse(row[9]),
                        data9 = int.Parse(row[10]),
                        data10 = int.Parse(row[11]),
                        data11 = int.Parse(row[12])
                    };

                    items.Add(int.Parse(row[0]), tmpItem);
                }
                sr.Close();
            }
        }

        private void LoadItemsByTextAsset()
        {
            TextAsset csvData = Resources.Load<TextAsset>("Table/DictionaryExample");

            char[] option = { '\r', '\n' };
            string[] lines = csvData.text.Split(option);

            for (int i = 1; i < lines.Length; i++)
            {
                if (string.IsNullOrEmpty(lines[i]))
                    continue;

                string[] datas = lines[i].Split(',');

                Item tmp;
                tmp.id = int.Parse(datas[0]);
                tmp.name = datas[1];
                tmp.data1 = int.Parse(datas[2]);
                tmp.data2 = int.Parse(datas[3]);
                tmp.data3 = int.Parse(datas[4]);
                tmp.data4 = int.Parse(datas[5]);
                tmp.data5 = int.Parse(datas[6]);
                tmp.data6 = int.Parse(datas[7]);
                tmp.data7 = int.Parse(datas[8]);
                tmp.data8 = int.Parse(datas[9]);
                tmp.data9 = int.Parse(datas[10]);
                tmp.data10 = int.Parse(datas[11]);
                tmp.data11 = int.Parse(datas[12]);

                items.Add(tmp.id, tmp);
            }
        }

        private void PrintItems()
        {
            foreach (KeyValuePair<int, Item> one in items)
            {
                Debug.Log($"Item Key: {one.Key}, Name: {one.Value.name}");
            }
        }
        
        private Item? GetDictionaryItem(int _key)
        {
            Item outItem;
            if (items.TryGetValue(_key, out outItem))
            {
                return outItem;
            }
            return null;
        }
    }
}