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
    float lastSum = float.PositiveInfinity;

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
    [SerializeField] string numberOfNodesPerLayer;

    void Start()
    {
        int[] nodesPerLayer = Network.FormatNumberOfNodes(numberOfNodesPerLayer);
        dots = FormatFromFile(FindObjectOfType<FileManager>().LoadFile(fileLocation));
        transform.position = (Position00.position + Position22.position) / 2;
        transform.localScale = new Vector3(Mathf.Abs(Position00.position.x - Position22.position.x), Mathf.Abs(Position00.position.y - Position22.position.y), 1);
        AI = new Network(nodesPerLayer, learnRate);

        if (dots.Count < numberOfTrainigExamples)
        {
            throw new Exception("Not enough dots in file");
        }

        for (int i = 0; i < numberOfTrainigExamples; i++)
        {
            DrawDot(dots[i]);
        }

        double[] wantedChanges = WantedChanges(dots[0]);
        double[] guess = Guess(dots[0]);

        for (int temp = 0; temp < 1000; temp++)
        {
            for (int i = 0; i < numberOfTrainigExamples; i++)
            {
                AI.BackPropagation(WantedChanges(dots[i]), AI.layers.Length - 1);
            }
        }

        wantedChanges = Guess(dots[0]);
        Debug.Log(MatrixOperations.ToString(wantedChanges));
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
            double[] guess = Guess(mouseScreenPosition.x, mouseScreenPosition.y);
            int index = Array.IndexOf(guess, guess.Max());
            output.GetComponent<SpriteRenderer>().color = new Color(index == 0 ? 1 : 0, index == 1 ? 1 : 0, index == 2 ? 1 : 0);
        }

        if (Input.GetMouseButtonDown(1))
        {
            for (int temp = 0; temp < 1000; temp++)
            {
                for (int i = 0; i < numberOfTrainigExamples; i++)
                {
                    AI.BackPropagation(WantedChanges(dots[i]), AI.layers.Length - 1);
                }
            }
            double[] wantedChanges = WantedChanges(dots[0]);
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
            if (y < (2.0 / 3.0) * x)
            {
                index = 0;
                AddDot(x, y, index);
                continue;
            }
            if (y < (3.0 / 2.0) * x)
            {
                index = 1;
                AddDot(x, y, index);
                continue;
            }
            index = 2;
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
        //FindObjectOfType<FileManager>().Append(addedDot.ToString(), fileLocation);
        DrawDot(x, y, index);
        FindObjectOfType<FileManager>().Append(addedDot.ToString(), fileLocation);
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

    double[] Guess(Dot dot)
    {
        double[] coordinates = new double[2];
        coordinates[0] = dot.x;
        coordinates[1] = dot.y;
        return AI.Guess(coordinates);
    }

    double[] Guess(double x, double y)
    {
        double[] coordinates = new double[2];
        coordinates[0] = x;
        coordinates[1] = y;
        return AI.Guess(coordinates);
    }

    double[] WantedChanges(Dot dot)
    {
        double[] coordinates = new double[2];
        coordinates[0] = dot.x;
        coordinates[1] = dot.y;
        return AI.WantedChanges(AI.Guess(coordinates), dot.Index);
    }

    double Cost(Dot dot)
    {
        return AI.Cost(WantedChanges(dot));
    }
}

public class Dot
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