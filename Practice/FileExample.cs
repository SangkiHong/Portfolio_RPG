using UnityEngine;
using System.Text;
using System.IO;
using System.Collections.Generic;

namespace SK
{
    public class FileExample : MonoBehaviour
    {
        void Start()
        {
            
        }

        private void Example()
        {
            string path = @"c:\temp\MyTest.txt";
            if (!File.Exists(path))
                Debug.Log("File is not exist");

            if (!Directory.Exists(path))
                Debug.Log("File is not exist");

            // ����Ʈ �迭�� ���Ͽ� �ۼ�
            // ���ڿ��� ����Ʈ �迭�� ��ȯ
            string str = "d:/dir/temp/aaa";
            byte[] bytes = Encoding.Default.GetBytes(str);
            string dataPath = Application.dataPath + "/text.bin";

            // ����Ʈ�迭�� ���Ͽ� ����
            // ����Ʈ ������� �÷��� ȣȯ�� ������
            File.WriteAllBytes(dataPath, bytes);

            // ������ ������ ����Ʈ �迭�� �д´�.
            byte[] arr2 = File.ReadAllBytes(dataPath);
            string datas = Encoding.Default.GetString(arr2);
            Debug.Log(datas);

            // Split �Լ�
            string data3 = "name,posx,posy,posz";
            string[] arrData3 = data3.Split(','); // ��ǥ(,) ������ �������� ����
            foreach (string one in arrData3)
                Debug.Log(one);

            // Parse �Լ� 
            string data4 = "123";
            int iData4 = int.Parse(data4); // ���ڿ��� ������ ����
            float fData4 = float.Parse(data4); // ���ڿ��� �Ǽ��� ����

            // IndexOf & Substring
            string data5 = "test.txt";
            int startIndex5 = data5.IndexOf('.'); // �տ������� �޸�(.)�� �˻��Ͽ� index���� ��ȯ
            string subdata5 = data5.Substring(startIndex5 + 1); // �޸�(.) ������ ���ڿ� ��ȯ
            Debug.Log(subdata5);

            // LastIndexOf & Substring
            string data6 = "Test.123.txt";
            int startIndex6 = data6.LastIndexOf('.'); // �ڿ������� �޸�(.)�� �˻��Ͽ� index���� ��ȯ
            string subdata6 = data6.Substring(startIndex6 + 1);
            Debug.Log(subdata6);

            // Replace
            string data7 = "East.West";
            data7.Replace('.', '_'); // �޸�(.)�� �����(_)�� ����
            Debug.Log(data7);

            string data8 = "123.456.789";
            data8.Replace('.', ' '); // �޸�(.)�� ���鹮�ڷ� ����
            Debug.Log(data8);

            string data9 = "ȫ�浿�� �ƹ����� �ƹ������ �θ��� ���ߴ�.";
            data9.Replace("ȫ�浿", " "); // Ư�� ���ڿ��� ���鹮�ڷ� ����
            Debug.Log(data9);

            string data10 = "d:/dir/temp/aaa";
            data10.Replace("/", @"\"); // ������(/)�� ��������(\)�� ����, @�� ���� ���� �״�θ� ���
            Debug.Log(data10);

            // Trim
            string data11 = "�����ٶ� ���ٻ�� ";
            data11.Trim(); // ���鹮�ڸ� ����
            Debug.Log(data11);

            using (StreamReader sr = new StreamReader(Application.dataPath + "/test.bin"))
            {
                string line = sr.ReadLine();
                Debug.Log(line); // line���� �о�� ���� /�� �ϳ��ε� ���� /�� �ΰ��� ����.
                sr.Close();
            }
        }
    }
}
