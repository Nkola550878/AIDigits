using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Unity.Properties;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.UI;

public class DrawingCanvas : MonoBehaviour
{
    Vector3 center;
    Vector3 mousePos;
    [SerializeField] float[,] pixels;

    [SerializeField] int dimension;
    [SerializeField] Sprite pixel;
    [SerializeField] float scale;
    [SerializeField] Transform pivot;
    [SerializeField] Camera camera;
    [SerializeField] Slider radiusSlider;
    [SerializeField] Slider strengthSlider;
    [SerializeField] float innerRadius;
    [SerializeField] Dropdown cifra;
    [SerializeField] Text text;

    [Header("Crtanje")]

    [SerializeField] float radius = 2;
    [SerializeField] float strength = 1f;

    void Start()
    {
        center = pivot.position;
        CreateCanvas();
        ReadRadius();
        ReadStrength();
        pixels = new float[dimension, dimension];
    }

    [ContextMenu("Create canvas")]
    void CreateCanvas()
    {
        int numberOfChildren = transform.childCount;
        while (numberOfChildren > 0)
        {
            Destroy(transform.GetChild(numberOfChildren - 1).gameObject);
            numberOfChildren--;
        }

        Vector2 start = new Vector2(center.x - (scale * dimension - 1) / 2, center.y - (scale * dimension - 1) / 2);
        for (int i = 0; i < dimension; i++)
        {
            GameObject coloumn = new GameObject();
            coloumn.transform.localPosition = new Vector3(start.x + i * scale, 0);
            coloumn.transform.parent = transform;
            coloumn.name = $"coloumn{i}";

            for (int j = 0; j < dimension; j++)
            {
                GameObject temp = new GameObject();
                temp.transform.localScale = Vector3.one * scale;
                temp.AddComponent<SpriteRenderer>();
                temp.GetComponent<SpriteRenderer>().sprite = pixel;
                temp.transform.localPosition = new Vector3(start.x + i * scale, start.y + j * scale);
                temp.transform.parent = coloumn.transform;
                temp.name = $"pixel{j}";
            }
        }
    }

    float Strength(float d)
    {
        return Mathf.Max(strength * (1 - 1 * d / radius), 0);
    }

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            if(Vector3.Distance(mousePos, camera.ScreenToWorldPoint(Input.mousePosition)) > innerRadius)
            {
                Paint();
                mousePos = camera.ScreenToWorldPoint(Input.mousePosition);
            }
        }
        if (Input.GetMouseButtonUp(0))
        {
            mousePos = new Vector3(0, 0, 1);
        }
        center = pivot.position;
    }

    void Paint()
    {
        Vector2 mousePosition = new Vector2(camera.ScreenToWorldPoint(Input.mousePosition).x, camera.ScreenToWorldPoint(Input.mousePosition).y);
        for (int i = 0; i < transform.childCount; i++)
        {
            for (int j = 0; j < transform.GetChild(i).childCount; j++)
            {
                GameObject current = transform.GetChild(i).GetChild(j).gameObject;
                float distance = Vector2.Distance(current.transform.position, mousePosition);
                float gray = Strength(distance);
                Color color = new Color(gray, gray, gray, 0);
                current.GetComponent<SpriteRenderer>().color -= new Color(gray, gray, gray, 0);
            }
        }
    }

    public void Clear()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            for (int j = 0; j < transform.GetChild(i).childCount; j++)
            {
                transform.GetChild(i).GetChild(j).gameObject.GetComponent<SpriteRenderer>().color = Color.white;
            }
        }
    }

    public void Save()
    {
        //Creating digits folders
        for (int i = 0; i < cifra.options.Count; i++)
        {
            if (!Directory.Exists($"{Application.persistentDataPath}/{cifra.options[i]}")) Directory.CreateDirectory(Path.Combine(Application.persistentDataPath, cifra.options[i].text));
        }

        //Creating file
        int fileName = 0;
        while (true)
        {
            if (!File.Exists($"{Application.persistentDataPath}/{cifra.options[cifra.value].text}/{fileName}.txt")) break;
            fileName++;
        }

        // Creating data
        //string path = $"{Application.persistentDataPath}/{cifra.options[cifra.value]}/{fileName}.txt";
        string path = Path.Combine(Application.persistentDataPath, cifra.options[cifra.value].text, $"{fileName}.txt");
        Debug.Log(path);
        StreamWriter streamWriter = new StreamWriter(path);
        string data = "";

        //Storing data
        for (int i = 0; i < dimension; i++)
        {
            for (int j = 0; j < dimension; j++)
            {
                data = data + transform.GetChild(i).GetChild(j).gameObject.GetComponent<SpriteRenderer>().color.r.ToString("F3", CultureInfo.InvariantCulture) + " ";
            }
            data += "\n";
        }
        streamWriter.Write(data);
        streamWriter.Close();
    }

    public void Load()
    {
        //HideChildren();
        pixels = FindObjectOfType<FileManager>().LoadFile($"{text.text}");
        if(pixels == null)
        {
            Clear();
            return;
        }
        LoadPicture();
        //ShowChildren();
    }

    public void LoadPicture()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            for (int j = 0; j < transform.GetChild(i).childCount; j++)
            {
                transform.GetChild(i).GetChild(j).gameObject.GetComponent<SpriteRenderer>().color = new Color(pixels[i, j], pixels[i, j], pixels[i, j]);
            }
        }
    }

    public void ReadRadius()
    {
        radius = radiusSlider.value;
    }

    public void ReadStrength()
    {
        strength = strengthSlider.value;
    }

    void HideChildren()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(false);
        }
    }
    void ShowChildren()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(true);
        }
    }
}