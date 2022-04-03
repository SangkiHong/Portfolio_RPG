using UnityEngine;
using System.IO;
using SK;

namespace SK.Data
{
    public class ExportData
    {
        public static void ExportCSVData(string fileName, int tagIndex)
        {
            string tagName = UnityEditorInternal.InternalEditorUtility.tags[tagIndex];
            GameObject[] environments = GameObject.FindGameObjectsWithTag(tagName);

            // txt 파일 생성
            using (StreamWriter sr = new StreamWriter(fileName)) 
            { 
                sr.Write("Index,Name,xPos,yPos,zPos,xRot,yRot,zRot,xScale,yScale,zScale");
                if (tagName == "Enemy") sr.Write(",isPatrol");
                sr.WriteLine();
                int index = 0;
                foreach (var obj in environments)
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
    }
}