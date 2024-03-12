using System.Collections;
using System.Collections.Generic;
using Unity.Properties;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.UI;

public class DrawingCanvas : MonoBehaviour
{
    Vector3 center;
    Vector3 mousePos;

    [SerializeField] int dimension;
    [SerializeField] Sprite pixel;
    [SerializeField] float scale;
    [SerializeField] Transform pivot;
    [SerializeField] Camera camera;
    [SerializeField] Slider radiusSlider;
    [SerializeField] Slider strengthSlider;

    [Header("Crtanje")]

    [SerializeField] float radius = 2;
    [SerializeField] float strength = 1f;

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

    float Strength(float d)
    {
        return Mathf.Max(strength * (1 - 1 * d / radius), 0);
    }

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            if(mousePos != Input.mousePosition)
            {
                Paint();
                mousePos = Input.mousePosition;
                Debug.Log("a");
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
                //Debug.Log($"{distance}, {gray}");
                //Debug.Log($"{current.GetComponent<SpriteRenderer>().color} - {color}");
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

    public void ReadRadius()
    {
        radius = radiusSlider.value;
    }

    public void ReadStrength()
    {
        strength = strengthSlider.value;
    }
}