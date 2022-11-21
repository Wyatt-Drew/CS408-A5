using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;
using UnityEngine.UI;

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
    public Vector2[,] velocityMap1;
    public Vector2[,] velocityMap2;
    System.Random rnd;

    Camera m_MainCamera;

    //Creative feature
    public float clickDensity = 1000f;
    public float clickSpeed = 10f;
    public TMP_InputField speedInput;
    public TMP_InputField densityInput;
    private bool isPaused = false;
    [SerializeField]
    private TextMeshProUGUI pauseTxt;

    void Start()
    {
        //Creative Feature (initializing text input fields)
        densityInput.text = clickDensity.ToString();
        speedInput.text = clickSpeed.ToString();

        Application.targetFrameRate = 60;
        m_MainCamera = Camera.main;
        //Correct formula for centering without sidebars
        //float screenRatio = (float)Screen.width / (float)Screen.height; 
        float screenRatio = 1f;
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
        generateGrid();
    }
    void generateGrid()
    {
        //displayGrids();
        //assign to empty arrays
        tileArray = new GameObject[columns,rows];
        densityMap1 = new float[columns, rows];
        densityMap2 = new float[columns, rows];
        velocityMap1 = new Vector2[columns, rows];
        velocityMap2 = new Vector2[columns, rows];
        float x;
        float y;
        GameObject template = (GameObject)Instantiate(Resources.Load("tile"));
        for (int i = 0; i < columns; i++)
        {
            for (int j = 0; j < rows; j++)
            {
                tileArray[i, j] = (GameObject)Instantiate(template, transform);
                x = i * tileSize;
                y = j * -tileSize;
                tileArray[i, j].transform.position = new Vector2(x, y);
                tileArray[i, j].GetComponent<addGasOnClick>().setXYManager(i, j, this);
            }
        }
        Destroy(template);
        float gridWidth = columns * tileSize;
        float gridHeight = rows * tileSize;
        // Center camera
        m_MainCamera.transform.position = new Vector3(gridWidth / 2 - tileSize / 2, -gridHeight / 2 + tileSize / 2, -10);
        //Generate gas
        generateGas();
    }
    void generateGas()
    {
        rnd = new System.Random();
        for (int i = 0; i < columns; i++)
        {
            for (int j = 0; j < rows; j++)
            {
                if ((rnd.Next(0, 20) + 1) % 20 == 0)
                {
                    velocityMap1[i, j] = randomVector(-3, 3f);
                    densityMap1[i, j] = randomFloat(0, 100);
                    //Create both so it works regardless of frame
                    velocityMap2[i, j] = new Vector2(velocityMap1[i, j].x, velocityMap1[i, j].y);
                    densityMap2[i, j] = densityMap1[i, j];
                }
                else
                {
                    velocityMap1[i, j] = new Vector2(0f, 0f);
                    densityMap1[i, j] = 0f;
                    velocityMap2[i, j] = new Vector2(0f, 0f);
                    densityMap2[i, j] = 0f;
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
        return UnityEngine.Random.Range(minValue, maxValue);
    }
    Color generateColor(float density)
    {
        float R = density;
        float G = density * density * 0.05f;
        float B = density * density * density * 0.0001f;
        return new Color(R, G, B, 1f);
    }

    void updateGas(float[,] thisDensityMap, float[,] nextDensityMap, Vector2[,] thisVelocityMap, Vector2[,] nextVelocityMap)
    {
        
        int xOffset = 0;
        float xFraction = 0f;
        int yOffset = 0;
        float yFraction = 0f;
        float massTL2 = 0f;
        float massTR2 = 0f;
        float massBL2 = 0f;
        float massBR2 = 0f;
        int T = 0; //top 
        int R = 0; //Right
        int L = 0; //Left
        int B = 0; //Bottom      
        //Debug.Log("L" + L + "R" + R + "B" + B + "T" + T);
        resetMap(nextDensityMap, nextVelocityMap);

        for (int i = 0; i < columns; i++)
        {
            for (int j = 0; j < rows; j++)
            {
                xOffset = (int)thisVelocityMap[i, j].x; //truncate
                xFraction = thisVelocityMap[i, j].x - xOffset; //remainder
                yOffset = (int)thisVelocityMap[i, j].y; //truncate
                yFraction = thisVelocityMap[i, j].y - yOffset; //remainder
                //Positions with wrap around
                if (xOffset < 0 || xFraction < 0) //If the x velocity is negative use left wrap around
                {
                    
                    L = (i + xOffset) % columns; //Mod for high speed wrap around
                    if (L < 0)
                    {
                        
                        L = columns + L;
                    }
                    R = (i + xOffset - 1) % columns;
                    if (R < 0)
                    {
                        R = columns + R;
                    }
                }
                else //right wrap around
                {
                    
                    L = (i + xOffset) % columns;
                    R = (i + xOffset + 1) % columns;


                }
                if (yOffset < 0 || yFraction < 0) // If Y is negative use top wrap around
                {
                    
                    T = (j + yOffset) % rows;
                    if (T < 0)
                    {
                        T = rows + T;
                    }
                    B = (j + yOffset - 1) % rows;
                    if (B < 0)
                    {
                        B = rows + B;
                    }

                }
                else //Bottom wrap around
                {
                    T = (j + yOffset) % rows;
                    B = (j + yOffset + 1) % rows;
                    
                }
                
                xFraction = Mathf.Abs(xFraction);
                yFraction = Mathf.Abs(yFraction);
                //calculate masses being added
                massTL2 = thisDensityMap[i, j] * (1 - xFraction) * (1 - yFraction);
                massTR2 = thisDensityMap[i, j] * (xFraction) * (1 - yFraction);
                massBL2 = thisDensityMap[i, j] * (1 - xFraction) * (yFraction);
                massBR2 = thisDensityMap[i, j] * (xFraction) * (yFraction);
                //Update mass and velocity
                //T,R,L,B named off positive vectors.  The square gets flipped when its going the other way.
                //Top Left
                if (massTL2 > 0f)
                {
                    nextVelocityMap[L, T].x = (thisVelocityMap[i, j].x * massTL2 + nextVelocityMap[L, T].x * nextDensityMap[L, T]) / (massTL2 + nextDensityMap[L, T]);
                    nextVelocityMap[L, T].y = (thisVelocityMap[i, j].y * massTL2 + nextVelocityMap[L, T].y * nextDensityMap[L, T]) / (massTL2 + nextDensityMap[L, T]);
                    nextDensityMap[L, T] = thisDensityMap[i, j] * (1 - xFraction) * (1 - yFraction) + nextDensityMap[L, T];
                }

                //Top Right
                if (massTR2 > 0f)
                {
                    nextVelocityMap[R, T].x = (thisVelocityMap[i, j].x * massTR2 + nextVelocityMap[R, T].x * nextDensityMap[R, T]) / (massTR2 + nextDensityMap[R, T]);
                    nextVelocityMap[R, T].y = (thisVelocityMap[i, j].y * massTR2 + nextVelocityMap[R, T].y * nextDensityMap[R, T]) / (massTR2 + nextDensityMap[R, T]);
                    nextDensityMap[R, T] = thisDensityMap[i, j] * (xFraction) * (1 - yFraction) + nextDensityMap[R, T];
                }
                //Bottom Left
                if (massBL2 > 0f)
                {
                    nextVelocityMap[L, B].x = (thisVelocityMap[i, j].x * massBL2 + nextVelocityMap[L, B].x * nextDensityMap[L, B]) / (massBL2 + nextDensityMap[L, B]);
                    nextVelocityMap[L, B].y = (thisVelocityMap[i, j].y * massBL2 + nextVelocityMap[L, B].y * nextDensityMap[L, B]) / (massBL2 + nextDensityMap[L, B]);
                    nextDensityMap[L, B] = thisDensityMap[i, j] * (1 - xFraction) * (yFraction) + nextDensityMap[L, B];
                }
                //Bottom Right
                if (massBR2 > 0f)
                {
                    nextVelocityMap[R, B].x = (thisVelocityMap[i, j].x * massBR2 + nextVelocityMap[R, B].x * nextDensityMap[R, B]) / (massBR2 + nextDensityMap[R, B]);
                    nextVelocityMap[R, B].y = (thisVelocityMap[i, j].y * massBR2 + nextVelocityMap[R, B].y * nextDensityMap[R, B]) / (massBR2 + nextDensityMap[R, B]);
                    nextDensityMap[R, B] = thisDensityMap[i, j] * (xFraction) * (yFraction) + nextDensityMap[R, B];
                }
            }
        }
    }
    //Purpose: Reset maps so we can write into them safely.
    void resetMap(float[,] thisDensityMap, Vector2[,] thisVelocityMap)
    {
        for (int i = 0; i < columns; i++)
        {
            for (int j = 0; j < rows; j++)
            {
                thisDensityMap[i, j] = 0f;
                thisVelocityMap[i, j].x = 0f;
                thisVelocityMap[i, j].y = 0f;
            }
        }
    }
    //Purpose: Update gas colors to current density
    void displayGas(float[,] thisDensityMap)
    {
        for (int i = 0; i < columns; i++)
        {
            for (int j = 0; j < rows; j++)
            {
                tileArray[i, j].GetComponent<SpriteRenderer>().color = generateColor(thisDensityMap[i, j]);
            }
        }
    }
    //Debug function
    //Purpose: Verify total mass does not change (within rounding error of Unity with floats).
    void sumMass()
    {
        float sum = 0f;
        for (int i = 0; i < columns; i++)
        {
            for (int j = 0; j < rows; j++)
            {
                sum += densityMap1[i, j];
            }
        }
        Debug.Log("Total Density: " + sum);
    }
    //Debug Function
    //Purpose: Display current and next values for velocity and density.
    void displayGrids(float[,] thisDensityMap, float[,] nextDensityMap, Vector2[,] thisVelocityMap, Vector2[,] nextVelocityMap)
    {
        for (int i = 0; i < columns; i++)
        {
            for (int j = 0; j < rows; j++)
            {
                Debug.Log("This: V:X" + thisVelocityMap[i, j].x + "Y" + thisVelocityMap[i, j].y + "Density " + thisDensityMap[i, j]);
                Debug.Log("Next: V:X" + nextVelocityMap[i, j].x + "Y" + nextVelocityMap[i, j].y + "Density " + nextDensityMap[i, j]);
            }
        }
    }
    // Update is called once per frame
    void Update()
    {
        if (!isPaused) //Creative feature
        {
            if (Time.frameCount % 2 == 0)
            {
                updateGas(densityMap1, densityMap2, velocityMap1, velocityMap2);
                displayGas(densityMap1);
                //sumMass();
            }
            else
            {
                updateGas(densityMap2, densityMap1, velocityMap2, velocityMap1);
                displayGas(densityMap2);
            }
        }
    }
    //Creative Feature**********************************
    //Button and input field functions
    public void incrementGasOnClick(int i, int j)
    {
        if (Time.frameCount % 2 == 0)
        {
            densityMap1[i, j] += clickDensity;
            velocityMap1[i, j] = randomVector(-clickSpeed, clickSpeed);
        }
        else
        {
            densityMap2[i, j] += clickDensity;
            velocityMap2[i, j] = randomVector(-clickSpeed, clickSpeed);
        }
        //Debug.Log("Density" + densityMap1[i, j] + "veloctiy" + velocityMap1[i, j].x + "Y" + velocityMap1[i, j].y);
    }
    //Button Input
    public void increaseDensityButton()
    {
        clickDensity += 10f;
        densityInput.text = clickDensity.ToString();
    }
    public void decreaseDensityButton()
    {
        clickDensity -= 10f;
        densityInput.text = clickDensity.ToString();
    }
    public void increaseSpeedButton()
    {
        clickSpeed += 1f;
        speedInput.text = clickSpeed.ToString();
    }
    public void decreaseSpeedButton()
    {
        clickSpeed -= 1f;
        speedInput.text = clickSpeed.ToString();
    }
    //Text input: Density
    public void typeDensity()
    {
        float val;
        if (float.TryParse(densityInput.text, out val))
        {
            clickDensity = val;
        }
        else
        {
            Debug.Log("Text is not a valid number");
        }
    }
    //Text input: Speed
    public void typeSpeed()
    {
        float val;
        if (float.TryParse(speedInput.text, out val))
        {
            clickSpeed = val;
        }
        else
        {
            Debug.Log("Text is not a valid number");
        }
    }
    //New game
    public void newGame()
    {
        //resetMap(densityMap1, velocityMap1);
        //resetMap(densityMap2, velocityMap2);
        clickDensity = 1000f;
        clickSpeed = 10f;
        generateGas();
        speedInput.text = clickSpeed.ToString();
        densityInput.text = clickDensity.ToString();
    }
    //Pause
    public void pause()
    {
        isPaused = ! isPaused;
        if (isPaused)
        {
            pauseTxt.text = "Resume";
        }else
        {
            pauseTxt.text = "Pause";
        }
    }
}
