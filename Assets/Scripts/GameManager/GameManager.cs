using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : SingletonMonobehaviour<GameManager>
{
    protected override void Awake()
    {
        base.Awake();
        SetScreenMode(true);
        //Application.targetFrameRate = 60;
    }
    

    public void SetScreenMode(bool isFullScreen)
    {
        if (isFullScreen)
        {
            Screen.SetResolution(1920,1080,FullScreenMode.ExclusiveFullScreen,0);
        }
        else
        {
            Screen.SetResolution(1920,1080,FullScreenMode.Windowed,0);
        }
    }
}
