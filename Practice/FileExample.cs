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

            // 바이트 배열을 파일에 작성
            // 문자열을 바이트 배열로 변환
            string str = "d:/dir/temp/aaa";
            byte[] bytes = Encoding.Default.GetBytes(str);
            string dataPath = Application.dataPath + "/text.bin";

            // 바이트배열을 파일에 저장
            // 바이트 저장식은 플랫폼 호환에 용이함
            File.WriteAllBytes(dataPath, bytes);

            // 파일의 내용을 바이트 배열로 읽는다.
            byte[] arr2 = File.ReadAllBytes(dataPath);
            string datas = Encoding.Default.GetString(arr2);
            Debug.Log(datas);

            // Split 함수
            string data3 = "name,posx,posy,posz";
            string[] arrData3 = data3.Split(','); // 쉼표(,) 단위로 구분지어 보관
            foreach (string one in arrData3)
                Debug.Log(one);

            // Parse 함수 
            string data4 = "123";
            int iData4 = int.Parse(data4); // 문자열을 정수로 보관
            float fData4 = float.Parse(data4); // 문자열을 실수로 보관

            // IndexOf & Substring
            string data5 = "test.txt";
            int startIndex5 = data5.IndexOf('.'); // 앞에서부터 콤마(.)를 검색하여 index값을 반환
            string subdata5 = data5.Substring(startIndex5 + 1); // 콤마(.) 다음의 문자열 반환
            Debug.Log(subdata5);

            // LastIndexOf & Substring
            string data6 = "Test.123.txt";
            int startIndex6 = data6.LastIndexOf('.'); // 뒤에서부터 콤마(.)를 검색하여 index값을 반환
            string subdata6 = data6.Substring(startIndex6 + 1);
            Debug.Log(subdata6);

            // Replace
            string data7 = "East.West";
            data7.Replace('.', '_'); // 콤마(.)를 언더바(_)로 변경
            Debug.Log(data7);

            string data8 = "123.456.789";
            data8.Replace('.', ' '); // 콤마(.)를 공백문자로 변경
            Debug.Log(data8);

            string data9 = "홍길동은 아버지를 아버지라고 부르지 못했다.";
            data9.Replace("홍길동", " "); // 특정 문자열을 공백문자로 변경
            Debug.Log(data9);

            string data10 = "d:/dir/temp/aaa";
            data10.Replace("/", @"\"); // 슬래쉬(/)를 역슬래쉬(\)로 변경, @를 통해 문자 그대로를 사용
            Debug.Log(data10);

            // Trim
            string data11 = "가나다라 마바사아 ";
            data11.Trim(); // 공백문자를 제거
            Debug.Log(data11);

            using (StreamReader sr = new StreamReader(Application.dataPath + "/test.bin"))
            {
                string line = sr.ReadLine();
                Debug.Log(line); // line에서 읽어온 값은 /가 하나인데 실제 /는 두개가 있음.
                sr.Close();
            }
        }
    }
}
