using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.UIElements;
using UnityEngine.UI;
using System.IO;
using System.Linq;
using System.Globalization;

public class FileManager : MonoBehaviour
{
    [SerializeField] Transform panel;

    //public void CreateFileExplorer(string startFolder)
    //{
    //    for (int i = 0; i < panel.childCount; i++)
    //    {
    //        Destroy(panel.GetChild(i));
    //    }

    //    string[] directories = Directory.GetDirectories(startFolder);
    //    Debug.Log(directories.Length);
    //    for (int i = 0; i < directories.Length; i++)
    //    {
    //        GameObject temp = Instantiate(button, panel.localPosition + new Vector3(0, i * button.GetComponent<RectTransform>().localScale.y), Quaternion.identity, panel);
    //        temp.GetComponentInChildren<Text>().text = Path.GetFileName(directories[i]);
    //        temp.GetComponent<Button>().onClick.AddListener(() => CreateFileExplorer($"{startFolder}/{directories[i]}"));
    //    }

    //    string[] files = Directory.GetFiles(startFolder);
    //    Debug.Log(directories.Length);
    //    for (int i = 0; i < files.Length; i++)
    //    {
    //        GameObject temp = Instantiate(button, panel.position + new Vector3(0, i * button.GetComponent<RectTransform>().localScale.y), Quaternion.identity, panel);
    //        temp.GetComponentInChildren<Text>().text = Path.GetFileName(files[i]);
    //        temp.GetComponent<Button>().onClick.AddListener(() => LoadFile($"{startFolder}/{files[i]}"));
    //    }


    //}

    public string LoadFile(string s)
    {
        string[] path = s.Split("/");
        string fullPath = Application.persistentDataPath;

        for (int i = 0; i < path.Length; i++)
        {
            fullPath = Path.Combine(fullPath, path[i]);
        }

        if (!File.Exists(fullPath))
            return null;

        StreamReader streamReader = new StreamReader(fullPath);
        string data = streamReader.ReadToEnd();
        return data;
    }

    public void Save(string data, Dropdown digit, string fileName)
    {
        string path = Path.Combine(Application.persistentDataPath, fileName);
        StreamWriter streamWriter = new StreamWriter(path);
        streamWriter.Write(data);
        streamWriter.Close();
    }
}
