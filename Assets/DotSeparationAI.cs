using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class DotSeparationAI : MonoBehaviour
{
    List<Dot> dots = new List<Dot>();
    string fileLocation = "dots.csv";
    Network AI;

    [Header("References")]

    [SerializeField] float radius;
    [SerializeField] Transform Position00;
    [SerializeField] Transform Position22;
    [SerializeField] InputField inputX;
    [SerializeField] InputField inputY;
    [SerializeField] InputField inputValue;
    [SerializeField] GameObject circleSprite;

    void Start()
    {
        dots = FormatFromFile(FindObjectOfType<FileManager>().LoadFile(fileLocation));
        transform.position = (Position00.position + Position22.position) / 2;
        transform.localScale = new Vector3(Mathf.Abs(Position00.position.x - Position22.position.x), Mathf.Abs(Position00.position.y - Position22.position.y), 1);
        //AddDots(500);
        //Layer layer = new Layer();
        AI.Guess();
    }

    void AddDots(int number)
    {
        float x;
        float y;
        int index;

        for (int i = 0; i < number; i++)
        {
            x = UnityEngine.Random.value * 2;
            y = UnityEngine.Random.value * 2;
            if (2 * x + y - 1.5 < 0)
            {
                index = 0;
                AddDot(x, y, index);
                continue;
            }
            if (2.5 * x + y - 3 < 0)
            {
                index = 2;
                AddDot(x, y, index);
                continue;
            }
            index = 1;
            AddDot(x, y, index);
            continue;
        }
    }

    public void AddDotButton()
    {
        float x = float.Parse(inputX.text);
        float y = float.Parse(inputY.text);
        int index = int.Parse(inputValue.text);
        AddDot(x, y, index);
    }

    public void AddDot(float x, float y, int index)
    {
        Dot addedDot = new Dot(x, y, index);
        dots.Add(addedDot);
        FindObjectOfType<FileManager>().Append(addedDot.ToString(), fileLocation);
        DrawDot(x, y, index);
    }

    List<Dot> FormatFromFile(string s)
    {
        List<Dot> result = new List<Dot>();
        string[] lines = s.Split("\n");

        string currentLine;
        string[] brokenLine;

        float x, y;
        int value;

        for (int i = 1; i < lines.Length; i++)
        {
            currentLine = lines[i];
            if (currentLine == "" || currentLine == null) break;
            brokenLine = currentLine.Split(',');
            x = float.Parse(brokenLine[0]);
            y = float.Parse(brokenLine[1]);
            value = int.Parse(brokenLine[2]);
            result.Add(new Dot(x, y, value));
            DrawDot(x, y, value);
        }
        return result;
    }

    public void DrawDot(float x, float y, int index)
    {
        GameObject temp = Instantiate(circleSprite, Vector3.zero, Quaternion.identity);
        temp.SetActive(true);
        temp.transform.localScale = Vector2.one * radius;
        float newX = 0 + x / 2 * (Position22.position.x - Position00.position.x) + Position00.position.x;
        float newY = 0 + y / 2 * (Position22.position.y - Position00.position.y) + Position00.position.y;
        temp.transform.position = new Vector2(newX, newY);
        temp.GetComponent<SpriteRenderer>().color = new Color(index == 0 ? 1 : 0, index == 1 ? 1 : 0, index == 2 ? 1 : 0);
    }

    void Update()
    {
        
    }
}

internal class Dot
{
    float x, y;
    int index;

    public Dot(float l_x, float l_y, int l_index)
    {
        x = l_x;
        y = l_y;
        index = l_index;
    }

    public override string ToString()
    {
        return $"{x},{y},{index}";
    }
}

internal class Layer
{
    public float[] inputNodes;
    public float[] outputNodes;
    public float[,] conections;
    float[] biases;

    public Layer(int numberOfInputNodes, int numberOfOutputNodes)
    {
        conections = new float[numberOfOutputNodes, numberOfInputNodes];
        FillConnections();
    }

    void FillConnections()
    {
        for (int i = 0; i < conections.GetLength(0); i++)
        {
            for (int j = 0; j < conections.GetLength(1); j++)
            {
                conections[i, j] = UnityEngine.Random.value;
                Debug.Log(conections[i, j]);
            }
        }
    }
}

internal class Network
{
    int[] numberOfNodesPerLayer = new int[] { 2, 3 };
    Layer[] layers;

    public Network()
    {
        layers = new Layer[numberOfNodesPerLayer.Length - 1];
        for (int i = 0; i < numberOfNodesPerLayer.Length - 1; i++)
        {
            layers[i] = new Layer(numberOfNodesPerLayer[i], numberOfNodesPerLayer[i + 1]);
        }
    }

    public int Guess()
    {
        for (int i = 0; i < numberOfNodesPerLayer.Length - 1; i++)
        {
            layers[i + 1].inputNodes = Network.Multiply(layers[i].inputNodes, layers[i].conections);
        }
        return Array.IndexOf(layers[numberOfNodesPerLayer.Length - 1].outputNodes, layers[numberOfNodesPerLayer.Length - 1].outputNodes.Max());
    }

    public static float[] Multiply(float[] vector, float[,] matrix)
    {
        if (matrix.GetLength(1) != vector.Length) return null;
        float[] result = new float[matrix.GetLength(0)];
        for (int i = 0; i < matrix.GetLength(0); i++)
        {
            float currentResult = 0;
            for (int j = 0; j < matrix.GetLength(1); j++)
            {
                currentResult += matrix[j, i] * vector[j];
            }
            result[i] = currentResult;
        }
        return result;
    }
}
