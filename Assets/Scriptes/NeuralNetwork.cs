using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

internal class Network
{
    public int numberOfSavedWantedChanges = 0;
    int[] numberOfNodesPerLayer;
    public Layer[] layers;
    //public Layer[,] wantedChangesForNumberOfExamples;
    public int numberOfExamples;
    double learningRate;

    public static int[] FormatNumberOfNodes(string numberOfNodes)
    {
        string[] numberOfNodesOnLayer = numberOfNodes.Split(',');
        int[] temp = new int[numberOfNodesOnLayer.Length];
        for (int i = 0; i < temp.Length; i++)
        {
            temp[i] = int.Parse(numberOfNodesOnLayer[i]);
        }
        return temp;
    }

    public Network(int[] l_numberOfNodesPerLayer, double l_learningRate)
    {
        numberOfNodesPerLayer = l_numberOfNodesPerLayer;
        layers = new Layer[numberOfNodesPerLayer.Length - 1];
        for (int i = 0; i < numberOfNodesPerLayer.Length - 1; i++)
        {
            layers[i] = new Layer(numberOfNodesPerLayer[i], numberOfNodesPerLayer[i + 1], true);
        }
        learningRate = l_learningRate;
    }

    public double[] Guess(double[] input)
    {
        for (int i = 0; i < input.Length; i++)
        {
            layers[0].inputNodes[i] = input[i];
        }

        for (int i = 0; i < layers.Length; i++)
        {
            layers[i].outputNodes = layers[i].Pass();
            if (i != layers.Length - 1) layers[i + 1].inputNodes = layers[i].outputNodes;
        }
        return layers[layers.Length - 1].outputNodes;
    }

    public double Cost(double[] wantedChanges)
    {
        double cost = 0;
        for (int i = 0; i < wantedChanges.Length; i++)
        {
            cost += Math.Pow(wantedChanges[i], 2);
        }
        return cost;
    }

    public void BackPropagation(double[] wantedChanges, int layerIndex)
    {
        Layer currentLayer = layers[layerIndex];
        double[] wantedChangesToPreviousLayer = new double[currentLayer.inputNodes.Length];

        double[] derivativeOfActivationFunction = layers[layerIndex].Derivative(layers[layerIndex].outputNodes);
        for (int i = 0; i < layers[layerIndex].outputNodes.Length; i++)
        {
            /* Derivative of sigmoid function
            double sigmoid = ActivationFunctions.Sigmoid(layers[layerIndex].outputNodes[i]);
            double dfds = sigmoid * (1 - sigmoid);
            */

            layers[layerIndex].biases[i] -= wantedChanges[i] * derivativeOfActivationFunction[i] * learningRate;
            for (int j = 0; j < layers[layerIndex].inputNodes.Length; j++)
            {
                layers[layerIndex].conections[i, j] -= wantedChanges[i] * derivativeOfActivationFunction[i] * layers[layerIndex].inputNodes[j] * learningRate;

                wantedChangesToPreviousLayer[j] += wantedChanges[i] * derivativeOfActivationFunction[i] * layers[layerIndex].conections[i, j];
            }
        }
        if (layerIndex > 0) BackPropagation(wantedChangesToPreviousLayer, layerIndex - 1);
    }

    public double[] WantedChanges(double[] guess, int index)
    {
        double[] wantedChanges = new double[guess.Length];
        Array.Copy(guess, wantedChanges, guess.Length);
        wantedChanges[index] = wantedChanges[index] - 1;
        return wantedChanges;
    }

    public void Learn(double[] inputData, int correctOutput)
    {
        double[] guess = Guess(inputData);
        double[] wantedChanges = WantedChanges(guess, correctOutput);
        BackPropagation(wantedChanges, layers.Length - 1);
    }
}

enum PossibleAcitvationFunctions
{
    sigmoid,
    relu
}

internal class Layer
{
    public double[] inputNodes;
    public double[] outputNodes;
    public double[,] conections;
    public double[] biases;
    public PossibleAcitvationFunctions activationFunction;

    public Layer(int numberOfInputNodes, int numberOfOutputNodes, bool randomize)
    {
        inputNodes = new double[numberOfInputNodes];
        conections = new double[numberOfOutputNodes, numberOfInputNodes];
        outputNodes = new double[numberOfOutputNodes];
        biases = new double[numberOfOutputNodes];
        activationFunction = PossibleAcitvationFunctions.sigmoid;
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

    public double[] Pass()
    {
        double[] weightedSum = MatrixOperations.Add(MatrixOperations.Multiply(inputNodes, conections), biases);
        switch (activationFunction)
        {
            case PossibleAcitvationFunctions.sigmoid:
                return ActivationFunctions.Sigmoid(weightedSum);
            case PossibleAcitvationFunctions.relu:
                return ActivationFunctions.RELU(weightedSum);
            default:
                throw new Exception("Not selected activation function");
        }
    }

    public double[] Derivative(double[] input)
    {
        switch (activationFunction)
        {
            case PossibleAcitvationFunctions.sigmoid:
                return ActivationFunctions.DerivativeSigmoid(input);
            case PossibleAcitvationFunctions.relu:
                return ActivationFunctions.DerivativeRELU(input);
            default:
                throw new Exception("Not selected activation function");
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
        if (v1.Length != v2.Length)
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


    public static string ToString<T>(T[] d)
    {
        string dAsString = "";
        for (int i = 0; i < d.Length; i++)
        {
            dAsString = dAsString + ", " + d[i].ToString();
        }
        dAsString = dAsString.Substring(2);
        return dAsString;
    }

    public static double[] Array2DToArray1D(float[,] valuesMatrix)
    {
        double[] valuesArray = new double[valuesMatrix.GetLength(0) * valuesMatrix.GetLength(1)];
        for (int i = 0; i < valuesMatrix.GetLength(0); i++)
        {
            for (int j = 0; j < valuesMatrix.GetLength(1); j++)
            {
                valuesArray[i * valuesMatrix.GetLength(1) + j] = valuesMatrix[i, j];
            }
        }
        return valuesArray;
    }
}

public class ActivationFunctions
{
    public static double Sigmoid(double input)
    {
        return 1 / (1 + Math.Pow(Math.E, -input));
    }

    public static double[] Sigmoid(double[] input)
    {
        double[] result = new double[input.Length];

        for (int i = 0; i < input.Length; i++)
        {
            result[i] = Sigmoid(input[i]);
        }
        return result;
    }

    public static double[] DerivativeSigmoid(double[] input)
    {
        double[] output = new double[input.Length];
        for (int i = 0; i < input.Length; i++)
        {
            output[i] = Sigmoid(input[i]) * (1 - Sigmoid(input[i]));
        }
        return output;
    }

    public static double RELU(double input)
    {
        return Math.Max(0, input);
    }

    public static double[] RELU(double[] input)
    {
        double[] result = new double[input.Length];

        for (int i = 0; i < input.Length; i++)
        {
            result[i] = RELU(input[i]);
        }
        return result;
    }

    public static double[] DerivativeRELU(double[] input)
    {
        double[] output = new double[input.Length];
        for (int i = 0; i < input.Length; i++)
        {
            output[i] = input[i] > 0 ? input[i] : 0;
        }
        return output;
    }
}