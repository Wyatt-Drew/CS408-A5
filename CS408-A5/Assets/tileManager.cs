using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class tileManager : MonoBehaviour
{
    [SerializeField]
    private int rows;
    [SerializeField]
    private int columns;
    [SerializeField]
    private float tileSize;

    public GameObject[,] tileArray;
    public float[,] densityMap1;
    public float[,] densityMap2;
    public Vector2[,] velocityMap;
    System.Random rnd;

    Camera m_MainCamera;
    // Start is called before the first frame update
    void Start()
    {
        // height = 2x size
        // width = 3X size
        // height/2 or width/3
        // 16*9

        m_MainCamera = Camera.main;
        float screenRatio = (float)Screen.width / (float)Screen.height;
        float targetRatio = columns / rows;
        if (screenRatio >= targetRatio)
        {
            m_MainCamera.orthographicSize = rows / 2;
        }
        else
        {
            float diff = targetRatio / screenRatio;
            m_MainCamera.orthographicSize = rows / 2*diff;
        }
        //m_MainCamera.orthographicSize = Math.Max(rows/2, columns/3);
        generateGrid();
    }
    void generateGrid()
    {
        //assign to empty arrays
        tileArray = new GameObject[rows, columns];
        densityMap1 = new float[rows, columns];
        densityMap2 = new float[rows, columns];
        velocityMap = new Vector2[rows, columns];
        float x;
        float y;
        GameObject template = (GameObject)Instantiate(Resources.Load("tile"));
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                tileArray[i, j] = (GameObject)Instantiate(template, transform);
                x = j * tileSize;
                y = i * -tileSize;
                tileArray[i, j].transform.position = new Vector2(x, y);
                
            }
        }
        Destroy(template);
        float gridWidth = columns * tileSize;
        float gridHeight = rows * tileSize;
        // Center camera
        m_MainCamera.transform.position = new Vector3(gridWidth / 2 - tileSize / 2, -gridHeight / 2 + tileSize / 2, -10);
        //Generate gas
        rnd = new System.Random();
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                //Debug.Log(rnd.Next(0, 3) % 3 + 1);
                if ((rnd.Next(0,20) + 1)%20 == 0)
                {
                    velocityMap[i, j] = randomVector(-3,3);
                    densityMap1[i, j] = randomFloat(0, 100);
                }
                else
                {
                    velocityMap[i, j] = new Vector2(0f, 0f);
                    densityMap1[i, j] = 0f;
                }
            }
        }

    }
    Vector2 randomVector(float minValue, float maxValue)
    {
        return new Vector2(randomFloat(minValue, maxValue), randomFloat(minValue, maxValue));
    }
    float randomFloat(float minValue, float maxValue)
    {
        return (float)(rnd.NextDouble() * (maxValue - minValue) + minValue);
    }
    Color generateColor(float density)
    {
        float R = density;
        float G = density * density * 0.05f;
        float B = density * density * density * 0.0001f;
        return new Color(R, G, B, 1f);
    }

    void moveGas(int i, int j)
    {
        //densityMap1[i, j] = 
    }
    void updateGas()
    {
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                moveGas(i, j);
            }
        }
    }
    void displayGas()
    {
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                tileArray[i, j].GetComponent<SpriteRenderer>().color = generateColor(densityMap1[i, j]);
            }
        }
    }
    // Update is called once per frame
    void Update()
    {
        updateGas();
        displayGas();
    }
}
