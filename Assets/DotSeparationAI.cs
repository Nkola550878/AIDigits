using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class DotSeparationAI : MonoBehaviour
{
    List<Dot> dots = new List<Dot>();
    string fileLocation = "dots.csv";
    Network AI;

    [Header("References")]

    [SerializeField] Camera camera;
    [SerializeField] SpriteRenderer output;
    [SerializeField] float radius;
    [SerializeField] Transform Position00;
    [SerializeField] Transform Position22;
    [SerializeField] InputField inputX;
    [SerializeField] InputField inputY;
    [SerializeField] InputField inputValue;
    [SerializeField] GameObject circleSprite;

    [Header("AI")]

    //[SerializeField] float[] wantedChanges;
    //[SerializeField] int numberOfExamples = 100;
    [SerializeField] double learnRate = 0.01f;
    [SerializeField] int numberOfTrainigExamples;

    void Start()
    {
        dots = FormatFromFile(FindObjectOfType<FileManager>().LoadFile(fileLocation));
        transform.position = (Position00.position + Position22.position) / 2;
        transform.localScale = new Vector3(Mathf.Abs(Position00.position.x - Position22.position.x), Mathf.Abs(Position00.position.y - Position22.position.y), 1);
        AI = new Network(new int[] { 2, 3 }, learnRate);

        double[] wantedChanges = AI.WantedChanges(dots[0]);
        double[] guess = AI.Guess(dots[0]);

        Debug.Log($"W: {MatrixOperations.ToString(wantedChanges)}");
        Debug.Log($"G: {MatrixOperations.ToString(AI.Guess(dots[0]))}");

        for (int temp = 0; temp < 1000; temp++)
        {
            for (int i = 0; i < numberOfTrainigExamples; i++)
            {
                DrawDot(dots[i]);
                AI.BackPropagation(AI.WantedChanges(dots[i]), AI.layers.Length - 1);
                //wantedChanges = AI.WantedChanges(dots[i]);
            }
        }

        wantedChanges = AI.WantedChanges(dots[0]);
        Debug.Log($"W: {MatrixOperations.ToString(wantedChanges)}");
        Debug.Log($"G: {MatrixOperations.ToString(AI.Guess(dots[0]))}");
    }

    private void Update()
    {
        Vector3 mousePosition = camera.ScreenToWorldPoint(Input.mousePosition);
        if (Input.GetMouseButtonDown(0))
        {
            if (mousePosition.x > Position22.position.x) return;
            if (mousePosition.x < Position00.position.x) return;
            if (mousePosition.y > Position22.position.y) return;
            if (mousePosition.y < Position00.position.y) return;
            Vector2 mouseScreenPosition;
            mouseScreenPosition = (mousePosition - Position00.position) / (Position22.position.x - Position00.position.x) * 2;
            double[] guess = AI.Guess(mouseScreenPosition.x, mouseScreenPosition.y);
            int index = Array.IndexOf(guess, guess.Max());
            output.GetComponent<SpriteRenderer>().color = new Color(index == 0 ? 1 : 0, index == 1 ? 1 : 0, index == 2 ? 1 : 0);
        }
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
            //DrawDot(x, y, value);
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

    void DrawDot(Dot dot)
    {
        DrawDot((float)dot.x, (float)dot.y, dot.Index);
    }
}

internal class Dot
{
    public double x, y;
    int index;
    public int Index
    {
        get
        {
            return index;
        }
    }

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

internal class Network
{
    public int numberOfSavedWantedChanges = 0;
    int[] numberOfNodesPerLayer;
    public Layer[] layers;
    //public Layer[,] wantedChangesForNumberOfExamples;
    public int numberOfExamples;
    double learningRate;

    public Network(int[] l_numberOfNodesPerLayer, double l_learningRate)
    {
        numberOfNodesPerLayer = l_numberOfNodesPerLayer;
        layers = new Layer[numberOfNodesPerLayer.Length - 1];
        //wantedChangesForNumberOfExamples = new Layer[l_numberOfExamples, numberOfNodesPerLayer.Length - 1];
        for (int i = 0; i < numberOfNodesPerLayer.Length - 1; i++)
        {
            layers[i] = new Layer(numberOfNodesPerLayer[i], numberOfNodesPerLayer[i + 1], true);
        }
        //numberOfExamples = l_numberOfExamples;
        learningRate = l_learningRate;
    }

    public double[] Guess(double x, double y)
    {
        layers[0].inputNodes[0] = x;
        layers[0].inputNodes[1] = y;
        for (int i = 0; i < layers.Length; i++)
        {
            layers[i].outputNodes = MatrixOperations.Sigmoid(MatrixOperations.Add(MatrixOperations.Multiply(layers[i].inputNodes, layers[i].conections), layers[i].biases));
            if (i != layers.Length - 1) layers[i + 1].inputNodes = layers[i].outputNodes;
        }
        return layers[layers.Length - 1].outputNodes;
    }

    public double[] Guess(Dot dot)
    {
        return Guess(dot.x, dot.y);
    }

    public float Cost(Dot dot)
    {
        double[] wantedChanges = WantedChanges(dot);
        float cost = 0;
        for (int i = 0; i < wantedChanges.Length; i++)
        {
            cost += (float)Math.Pow(wantedChanges[i], 2);
        }
        return cost;
    }

    public void BackPropagation(double[] wantedChanges, int layerIndex)
    {
        Layer currentLayer = layers[layerIndex];
        double[] wantedChangesToPreviousLayer = new double[currentLayer.inputNodes.Length];

        //wantedChangesToPreviousLayer = wantedChanges;
        for (int i = 0; i < layers[layerIndex].outputNodes.Length; i++)
        {
            double sigmoid = MatrixOperations.Sigmoid(layers[layerIndex].outputNodes[i]);
            double dfds = sigmoid * (1 - sigmoid);

            layers[layerIndex].biases[i] -= wantedChanges[i] * dfds * learningRate;
            for (int j = 0; j < layers[layerIndex].inputNodes.Length; j++)
            {
                layers[layerIndex].conections[i, j] -= wantedChanges[i] * dfds * layers[layerIndex].inputNodes[j] * learningRate;

                wantedChangesToPreviousLayer[j] += wantedChanges[i] * dfds * layers[layerIndex].conections[i, j];
            }
        }
        if(layerIndex > 0) BackPropagation(wantedChangesToPreviousLayer, layerIndex - 1);
    }

    public double[] WantedChanges(Dot dot)
    {
        double[] wantedChanges = Guess(dot);
        wantedChanges[dot.Index] = wantedChanges[dot.Index] - 1;

        return wantedChanges;
    }
}

internal class Layer
{
    public double[] inputNodes;
    public double[] outputNodes;
    public double[,] conections;
    public double[] biases;

    public Layer(int numberOfInputNodes, int numberOfOutputNodes, bool randomize)
    {
        inputNodes = new double[numberOfInputNodes];
        conections = new double[numberOfOutputNodes, numberOfInputNodes];
        outputNodes = new double[numberOfOutputNodes];
        biases = new double[numberOfOutputNodes];
        if (randomize) FillConnections();
    }

    void FillConnections()
    {
        for (int i = 0; i < conections.GetLength(0); i++)
        {
            for (int j = 0; j < conections.GetLength(1); j++)
            {
                conections[i, j] = UnityEngine.Random.value;
            }
        }
    }
}

public class MatrixOperations
{
    public static double[] Multiply(double[] vector, double[,] matrix)
    {
        if (matrix.GetLength(1) != vector.Length) return null;
        double[] result = new double[matrix.GetLength(0)];
        for (int i = 0; i < matrix.GetLength(0); i++)
        {
            double currentResult = 0;
            for (int j = 0; j < matrix.GetLength(1); j++)
            {
                currentResult += matrix[i, j] * vector[j];
            }
            result[i] = currentResult;
        }
        return result;
    }

    public static double[] Add(double[] v1, double[] v2)
    {
        if(v1.Length != v2.Length)
        {
            return null;
        }
        double[] result = new double[v1.Length];

        for (int i = 0; i < v1.Length; i++)
        {
            result[i] = v1[i] + v2[i];
        }
        return result;
    }

    public static double[] Sigmoid(double[] v)
    {
        double[] result = new double[v.Length];

        for (int i = 0; i < v.Length; i++)
        {
            result[i] = Sigmoid(v[i]);
        }
        return result;
    }

    public static double Sigmoid(double input)
    {
        return 1 / (1 + Math.Pow(Math.E, -input));
    }

    public static string ToString(double[] d)
    {
        string dAsString = "";
        for (int i = 0; i < d.Length; i++)
        {
            dAsString = $"{dAsString}, {d[i]}";
        }
        dAsString = dAsString.Substring(2);
        return dAsString;
    }
}