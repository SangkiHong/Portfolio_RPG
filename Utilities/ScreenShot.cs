using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class ScreenShot : MonoBehaviour
{
    public Camera _camera;       //보여지는 카메라.

    public bool size2ScreenSize;
    public int resWidth;
    public int resHeight;
    string path;

    void Start()
    {
        if (size2ScreenSize)
        {
            resWidth = Screen.width;
            resHeight = Screen.height;
        }
        path = Application.dataPath + "/ScreenShot/";
        Debug.Log(path);
    }

    private void LateUpdate()
    {
        if (Input.GetKeyUp(KeyCode.Space))
        {
            ClickScreenShot();
        }
    }

    public void ClickScreenShot()
    {
        DirectoryInfo dir = new DirectoryInfo(path);
        if (!dir.Exists)
        {
            Directory.CreateDirectory(path);
        }
        string name;
        name = path + System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".png";
        RenderTexture rt = new RenderTexture(resWidth, resHeight, 24);
        _camera.targetTexture = rt;
        Texture2D screenShot = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false);
        Rect rec = new Rect(0, 0, screenShot.width, screenShot.height);
        _camera.Render();
        RenderTexture.active = rt;
        screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
        screenShot.Apply();

        byte[] bytes = screenShot.EncodeToPNG();
        File.WriteAllBytes(name, bytes);
    }
}