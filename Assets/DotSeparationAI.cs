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

    [SerializeField] float[] wantedChanges;
    [SerializeField] int numberOfExamples = 100;
    [SerializeField] float learnRate = 0.01f;

    void Start()
    {
        dots = FormatFromFile(FindObjectOfType<FileManager>().LoadFile(fileLocation));
        transform.position = (Position00.position + Position22.position) / 2;
        transform.localScale = new Vector3(Mathf.Abs(Position00.position.x - Position22.position.x), Mathf.Abs(Position00.position.y - Position22.position.y), 1);
        AI = new Network(new int[] { 2, 3, 3 }, numberOfExamples, learnRate);
        for (int i = 0; i < 3000; i++)
        {
            AI.BackPropagation(AI.WantedChanges(dots[i].Index), AI.layers.Length);
        }
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
            float[] guess = AI.Guess(mousePosition.x, mousePosition.y);
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
}

internal class Dot
{
    public float x, y;
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
    public Layer[,] wantedChangesForNumberOfExamples;
    public int numberOfExamples;
    float learningRate;

    public Network(int[] l_numberOfNodesPerLayer, int l_numberOfExamples, float l_learningRate)
    {
        numberOfNodesPerLayer = l_numberOfNodesPerLayer;
        layers = new Layer[numberOfNodesPerLayer.Length - 1];
        wantedChangesForNumberOfExamples = new Layer[l_numberOfExamples, numberOfNodesPerLayer.Length - 1];
        for (int i = 0; i < numberOfNodesPerLayer.Length - 1; i++)
        {
            layers[i] = new Layer(numberOfNodesPerLayer[i], numberOfNodesPerLayer[i + 1], true);
        }
        numberOfExamples = l_numberOfExamples;
        learningRate = l_learningRate;
    }

    public float[] Guess(float x, float y)
    {
        layers[0].inputNodes[0] = x;
        layers[0].inputNodes[1] = y;
        for (int i = 0; i < numberOfNodesPerLayer.Length - 1; i++)
        {
            layers[i].outputNodes = MatrixOperations.Sigmoid(MatrixOperations.Add(MatrixOperations.Multiply(layers[i].inputNodes, layers[i].conections), layers[i].biases));
            if (i != numberOfNodesPerLayer.Length - 2) layers[i + 1].inputNodes = layers[i].outputNodes;
        }
        return layers[numberOfNodesPerLayer.Length - 2].outputNodes;
    }

    public float[] Guess(Dot dot)
    {
        return Guess(dot.x, dot.y);
    }

    public float Cost(Dot dot)
    {
        float[] guess = Guess(dot);
        float cost = 0;
        for (int i = 0; i < guess.Length; i++)
        {
            if (i == dot.Index)
            {
                cost += (float)Math.Pow((1 - guess[i]), 2);
                continue;
            }
            cost += (float)Math.Pow(guess[i], 2);
        }
        return cost;
    }

    public void BackPropagation(float[] wantedChanges, int layerIndex)
    {
        layerIndex--;
        Layer wantedChangesToPreviousLayer = new Layer(layers[layerIndex].inputNodes.Length, layers[layerIndex].outputNodes.Length, false);
        wantedChangesToPreviousLayer.outputNodes = wantedChanges;
        for (int i = 0; i < layers[layerIndex].outputNodes.Length; i++)
        {
            float dfds = (float)Mathf.Exp(layers[layerIndex].outputNodes[i]) / (float)Math.Pow(1 + Mathf.Exp(layers[layerIndex].outputNodes[i]), 2);

            //wantedChangesToPreviousLayer.biases[i] = wantedChanges[i] * dfds;
            layers[layerIndex].biases[i] -= wantedChanges[i] * dfds * learningRate;
            for (int j = 0; j < layers[layerIndex].inputNodes.Length; j++)
            {
                //wantedChangesToPreviousLayer.conections[i, j] = wantedChanges[i] * dfds / layers[layerIndex].inputNodes[j];
                wantedChangesToPreviousLayer.inputNodes[j] += wantedChanges[i] * dfds * layers[layerIndex].conections[i, j];

                layers[layerIndex].conections[i, j] -= wantedChanges[i] * dfds * layers[layerIndex].inputNodes[j] * learningRate;
            }
        }
        if(layerIndex != 0) BackPropagation(wantedChangesToPreviousLayer.inputNodes, layerIndex);
        //wantedChangesForNumberOfExamples[numberOfSavedWantedChanges, layerIndex] = wantedChangesToPreviousLayer;
        //if(layerIndex == layers.Length - 1) numberOfSavedWantedChanges = (numberOfSavedWantedChanges + 1) % numberOfExamples;
        //if(numberOfSavedWantedChanges == 0 && layerIndex == layers.Length - 1)
        //{
        //    UpdateAI();
        //}
    }

    public float[] WantedChanges(int indexOfCorrect)
    {
        float[] wantedChanges = layers[numberOfNodesPerLayer.Length - 2].outputNodes;
        wantedChanges[indexOfCorrect] = 1 - layers[numberOfNodesPerLayer.Length - 2].outputNodes[indexOfCorrect];
        return wantedChanges;
    }

    //private void UpdateAI(float cost)
    //{
    //    Vector2Int locationOfMax = new Vector2Int(-2, -2);
    //    float valueOfMaximum = float.NegativeInfinity;
    //    Vector2Int temp = new Vector2Int(-2, -2);
    //    int indexOfCurrentLayer = -1;
    //    for (int i = 0; i < numberOfExamples; i++)
    //    {
    //        valueOfMaximum = float.NegativeInfinity;
    //        for (int j = 0; j < layers.Length; j++)
    //        {
    //            //Debug.Log($"i: {i}, j: {j}");
    //            temp = LocationOfMaximumInLayer(wantedChangesForNumberOfExamples[i, j]);
    //            if(valueOfMaximum < Math.Abs(temp.y == -1 ? wantedChangesForNumberOfExamples[i, j].biases[temp.x] : wantedChangesForNumberOfExamples[i, j].conections[temp.x, temp.y]))
    //            {
    //                locationOfMax = temp;
    //                valueOfMaximum = Math.Abs(temp.y == -1 ? wantedChangesForNumberOfExamples[i, j].biases[temp.x] : wantedChangesForNumberOfExamples[i, j].conections[temp.x, temp.y]);
    //                indexOfCurrentLayer = j;
    //            }
    //        }

    //        if (locationOfMax.y == -1)
    //        {
    //            layers[indexOfCurrentLayer].biases[locationOfMax.x] -= cost / layers[indexOfCurrentLayer].biases[locationOfMax.x];
    //            continue;
    //        }
    //        //Debug.Log($"locationOfMax: {locationOfMax}, indexOfCorrectLayer: {indexOfCurrentLayer}");
    //        layers[indexOfCurrentLayer].conections[locationOfMax.x, locationOfMax.y] -= cost / layers[indexOfCurrentLayer].conections[locationOfMax.x, locationOfMax.y];
    //    }
    //}

    //private Vector2Int LocationOfMaximumInLayer(Layer layer)
    //{
    //    Vector2Int locationOfMax = new Vector2Int(0, 0);
    //    float max = layer.conections[0, 0];
    //    for (int i = 0; i < layer.outputNodes.Length; i++)
    //    {
    //        if (max < Math.Abs(layer.biases[i]))
    //        {
    //            locationOfMax = new Vector2Int(i, -1);
    //            max = Math.Abs(layer.biases[i]);
    //        }
    //        for (int j = 0; j < layer.inputNodes.Length; j++)
    //        {
    //            if (max < Math.Abs(layer.conections[i, j]))
    //            {
    //                locationOfMax = new Vector2Int(i, j);
    //                max = Math.Abs(layer.conections[i, j]);
    //            }
    //        }
    //    }
    //    return locationOfMax;
    //}
}

internal class Layer
{
    public float[] inputNodes;
    public float[] outputNodes;
    public float[,] conections;
    public float[] biases;

    public Layer(int numberOfInputNodes, int numberOfOutputNodes, bool randomize)
    {
        inputNodes = new float[numberOfInputNodes];
        conections = new float[numberOfOutputNodes, numberOfInputNodes];
        outputNodes = new float[numberOfOutputNodes];
        biases = new float[numberOfOutputNodes];
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
    public static float[] Multiply(float[] vector, float[,] matrix)
    {
        if (matrix.GetLength(1) != vector.Length) return null;
        float[] result = new float[matrix.GetLength(0)];
        for (int i = 0; i < matrix.GetLength(0); i++)
        {
            float currentResult = 0;
            for (int j = 0; j < matrix.GetLength(1); j++)
            {
                currentResult += matrix[i, j] * vector[j];
            }
            result[i] = currentResult;
        }
        return result;
    }

    public static float[] Add(float[] v1, float[] v2)
    {
        if(v1.Length != v2.Length)
        {
            return null;
        }
        float[] result = new float[v1.Length];

        for (int i = 0; i < v1.Length; i++)
        {
            result[i] = v1[i] + v2[i];
        }
        return result;
    }

    public static float[] Sigmoid(float[] v)
    {
        float[] result = new float[v.Length];

        for (int i = 0; i < v.Length; i++)
        {
            result[i] = Sigmoid(v[i]);
        }
        return result;
    }

    public static float Sigmoid(float input)
    {
        return (float)(1 / (1 + Math.Pow(Math.E, -input)));
    }
}