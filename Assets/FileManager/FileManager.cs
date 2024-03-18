using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.UIElements;
using UnityEngine.UI;
using System.IO;
using System.Linq;

public class FileManager : MonoBehaviour
{
    [SerializeField] Transform panel;
    [SerializeField] GameObject button;

    public void CreateFileExplorer(string startFolder)
    {
        for (int i = 0; i < panel.childCount; i++)
        {
            Destroy(panel.GetChild(i));
        }

        string[] directories = Directory.GetDirectories(startFolder);
        Debug.Log(directories.Length);
        for (int i = 0; i < directories.Length; i++)
        {
            GameObject temp = Instantiate(button, panel.localPosition + new Vector3(0, i * button.GetComponent<RectTransform>().localScale.y), Quaternion.identity, panel);
            temp.GetComponentInChildren<Text>().text = Path.GetFileName(directories[i]);
            temp.GetComponent<Button>().onClick.AddListener(() => CreateFileExplorer($"{startFolder}/{directories[i]}"));
        }

        string[] files = Directory.GetFiles(startFolder);
        Debug.Log(directories.Length);
        for (int i = 0; i < files.Length; i++)
        {
            GameObject temp = Instantiate(button, panel.position + new Vector3(0, i * button.GetComponent<RectTransform>().localScale.y), Quaternion.identity, panel);
            temp.GetComponentInChildren<Text>().text = Path.GetFileName(files[i]);
            temp.GetComponent<Button>().onClick.AddListener(() => LoadFile($"{startFolder}/{files[i]}"));
        }


    }

    public float[,] LoadFile(string s)
    {
        //Debug.Log(Application.persistentDataPath);

        string[] path = s.Split("/");
        string fullPath = Application.persistentDataPath;

        for (int i = 0; i < path.Length; i++)
        {
            //Debug.Log(Path.Combine(fullPath, path[i]));
            fullPath = Path.Combine(fullPath, path[i]);
        }

        if(!File.Exists(fullPath))
            return null;
        StreamReader streamReader = new StreamReader(fullPath);

        string data = streamReader.ReadToEnd();
        string[] columns = data.Substring(0, data.Length - 1).Split("\n");
        float[,] pixels = new float[columns.Length,columns.Length];
        for (int i = 0; i < columns.Length; i++)
        {
            //Debug.Log($"i={i}");
            string[] currentColumn = columns[i].Substring(0, columns[i].Length - 1).Split(" ");
            for (int j = 0; j < currentColumn.Length; j++)
            {
                pixels[i, j] = float.Parse(currentColumn[j]);
                //Debug.Log(pixels[i,j]);
            }
        }
        return pixels;
    }
}
