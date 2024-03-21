using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Unity.Properties;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.UI;

public class DrawingCanvas : MonoBehaviour
{
    Vector3 center;
    Vector3 mousePos;
    float[,] pixels;
    int dimension = 32;

    [Header("References")]

    [SerializeField] Sprite pixel;
    [SerializeField] Transform pivot;
    [SerializeField] Camera camera;
    [SerializeField] Slider radiusSlider;
    [SerializeField] Slider strengthSlider;
    [SerializeField] Dropdown digit;
    [SerializeField] Text text;
    [SerializeField] Text loadedFromRandom;

    [Header("Crtanje")]

    [SerializeField] float innerRadius;
    [SerializeField] float scale;
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
        center = pivot.position;
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
        GameObject current;
        float distance;
        float gray;
        Color color;
        for (int i = 0; i < transform.childCount; i++)
        {
            for (int j = 0; j < transform.GetChild(i).childCount; j++)
            {
                current = transform.GetChild(i).GetChild(j).gameObject;
                distance = Vector2.Distance(current.transform.position, mousePosition);
                gray = Strength(distance);
                color = new Color(gray, gray, gray, 0);
                current.GetComponent<SpriteRenderer>().color -= new Color(gray, gray, gray, 0);
                pixels[i, j] = current.GetComponent<SpriteRenderer>().color.r;
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
        loadedFromRandom.text = "";
    }

    public void Save()
    {
        //Creating digits folders
        for (int i = 0; i < digit.options.Count; i++)
        {
            if (!Directory.Exists($"{Application.persistentDataPath}/{digit.options[i]}"))
                Directory.CreateDirectory(Path.Combine(Application.persistentDataPath, digit.options[i].text));
        }

        //Creating file
        int fileName = 0;
        while (true)
        {
            if (!File.Exists($"{Application.persistentDataPath}/{digit.options[digit.value].text}/{fileName}.txt")) break;
            fileName++;
        }

        // Creating data
        int dimension = pixels.GetLength(0);
        string path = Path.Combine($"{digit.options[digit.value].text}/{fileName}.txt");
        string data = "";

        //Storing data
        for (int i = 0; i < dimension; i++)
        {
            for (int j = 0; j < dimension; j++)
            {
                data = data + pixels[i, j].ToString("F3", CultureInfo.InvariantCulture) + " ";
            }
            data += "\n";
        }

        FindObjectOfType<FileManager>().Save(data, digit, path);
    }

    public void ButtonLoad()
    {
        Load(text.text);
    }

    void Load(string path)
    {
        //HideChildren();
        pixels = ReadFile(FindObjectOfType<FileManager>().LoadFile(path));
        if(pixels == null)
        {
            Clear();
            return;
        }
        LoadPicture();
        loadedFromRandom.text = "";
        //ShowChildren();
    }

    float[,] ReadFile(string data)
    {
        if (data == null) return new float[dimension, dimension];
        string[] columns = data.Substring(0, data.Length - 1).Split("\n");
        float[,] pixels = new float[columns.Length, columns.Length];
        for (int i = 0; i < columns.Length; i++)
        {
            string[] currentColumn = columns[i].Substring(0, columns[i].Length - 1).Split(" ");
            for (int j = 0; j < currentColumn.Length; j++)
            {
                pixels[i, j] = float.Parse(currentColumn[j]);
            }
        }
        return pixels;
    }

    void LoadPicture()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            for (int j = 0; j < transform.GetChild(i).childCount; j++)
            {
                transform.GetChild(i).GetChild(j).gameObject.GetComponent<SpriteRenderer>().color = new Color(pixels[i, j], pixels[i, j], pixels[i, j]);
            }
        }
    }

    public void LoadRandomPicture()
    {
        int numberOfDigits = digit.options.Count;
        bool[] hasFile = new bool[numberOfDigits];
        int randomNumber;
        string lookingAtDigit;
        do
        {
            randomNumber = Random.Range(0, numberOfDigits);
            lookingAtDigit = digit.options[randomNumber].text;
            hasFile[randomNumber] = true;

        } while (Directory.GetFiles(Path.Combine(Application.persistentDataPath, lookingAtDigit)).Length == 0 && hasFile.Any(c => c == false));

        int numberOfExamples = Directory.GetFiles(Path.Combine(Application.persistentDataPath, lookingAtDigit)).Length;
        randomNumber = Random.Range(0, numberOfExamples);
        //Debug.Log($"{lookingAtDigit}/{randomNumber}.txt");
        Load($"{lookingAtDigit}/{randomNumber}.txt");
        loadedFromRandom.text = lookingAtDigit;
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