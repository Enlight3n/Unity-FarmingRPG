using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : SingletonMonobehaviour<GameManager>
{
    public float fps;
    
    protected override void Awake()
    {
        base.Awake();
        Screen.SetResolution(1920,1080,FullScreenMode.Windowed,0);
        //Application.targetFrameRate = 60;
    }

    private void Update()
    {
        fps = 1 / Time.unscaledDeltaTime;
    }
}
