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

    private void Start()
    {
        //DontDestroyOnLoad(this);
    }

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
        streamReader.Close();
        return data;
    }

    public void Save(string data, string fileName)
    {
        string path = Path.Combine(Application.persistentDataPath, fileName);
        StreamWriter streamWriter = new StreamWriter(path);
        streamWriter.Write(data);
        streamWriter.Close();
    }

    public void Append(string data, string fileName)
    {
        string path = Path.Combine(Application.persistentDataPath, fileName);
        StreamWriter streamWriter = new StreamWriter(path, true);
        data = $"\n{data}";
        streamWriter.Write(data);
        streamWriter.Close();
    }
}
