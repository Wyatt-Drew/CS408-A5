using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class addGasOnClick : MonoBehaviour
{
    public int x;
    public int y;
    public bool selected = false;
    tileManager manager;
    public float gasCooldown = 0.3f;
    public float lastGasAdd;

    void start()
    {
        lastGasAdd = -5f;
    }
    void OnMouseOver()
    {
       // Debug.Log("add" + lastGasAdd + "cooldown" + gasCooldown + "time" + )
        if (Input.GetMouseButton(0) == true && (Time.realtimeSinceStartup >= lastGasAdd + gasCooldown))
        {
            manager.incrementGasOnClick(x, y);
            lastGasAdd = Time.realtimeSinceStartup;
        }
    }
    public void setXYManager(int i, int j, tileManager m)
    {
        x = i;
        y = j;
        manager = m;
    }
}
