using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class DrawingCanvas : MonoBehaviour
{
    [SerializeField] int dimension;
    [SerializeField] Sprite pixel;
    [SerializeField] float scale;
    [SerializeField] Transform pivot;
    Vector3 center;

    void Start()
    {
        center = pivot.position;
        CreateCanvas();
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

    void Update()
    {
        center = pivot.position;
    }
}