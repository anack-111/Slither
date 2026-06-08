using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class 
    Counter : MonoBehaviour
{
    private float deltaTime = 0f;

    [SerializeField] private int size = 30;
    [SerializeField] private Color color = Color.red;
    private void Awake()
    {
        Application.targetFrameRate = 60;
        // Application.targetFrameRate = 120;
    }

    void Update()
    {
        //deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
    }

    //private void OnGUI()
    //{
    //    GUIStyle style = new GUIStyle();

    //    Rect rect = new Rect(40, 40, Screen.width, Screen.height);
    //    style.alignment = TextAnchor.UpperLeft;
    //    style.fontSize = size;
    //    style.normal.textColor = color;

    //    float ms = deltaTime * 1000f;
    //    float fps = 1.0f / deltaTime;
    //    string text = string.Format("{0:0.} FPS ({1:0.0} ms)", fps, ms);

    //    GUI.Label(rect, text, style);
    //}
}