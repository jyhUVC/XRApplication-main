using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;
    public Color startColor;
    public Color endColor;
    public bool isDrawing;
    public bool isStamping;
    public bool isErasing;
    public bool isMyCam = true;
    private void Awake()
    {
        instance = this;
        startColor = Color.white;
        endColor = Color.white;
    }

    public void WhiteLine()
    {
        startColor = Color.white;
        endColor = Color.white;
    }

    public void RedLine()
    {
        startColor = Color.red;
        endColor = Color.red;
    }

    public void DrawMode()
    {
        isDrawing = true;
        isErasing = false;
        isStamping = false;
    }
    public void EraseMode()
    {
        isErasing = true;
        isDrawing = false;
        isStamping = false;
    }
    public void StampMode()
    {
        isStamping = true;
        isErasing = false;
        isDrawing = false;
    }

    
}
