using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : SingletonMonobehaviour<UIManager>
{
    //暂停菜单的整体
    [SerializeField] private GameObject pauseMenu = null;
    
    //暂停菜单的按钮
    [SerializeField] private Button[] menuButtons = null;
    //暂停菜单中，每个按钮打开的界面
    [SerializeField] private GameObject[] menuTabs = null;
    
    //暂停菜单开关的标志
    private bool _pauseMenuOn = false;
    public bool PauseMenuOn { get => _pauseMenuOn; set => _pauseMenuOn = value; }
    
    
    protected override void Awake()
    {
        base.Awake();

        pauseMenu.SetActive(false);
    }

    private void Update()
    {
        PauseMenu();
    }
    
    //每帧调用，处理暂停菜单的启用与关闭
    private void PauseMenu()
    {
        //检测是否按下按键
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Tab))
        {
            //如果菜单本来是开的，那么关闭菜单，否则启用菜单
            if (PauseMenuOn)
            {
                DisablePauseMenu();
            }
            else
            {
                EnablePauseMenu();
            }
        }
    }
    
    //启用菜单界面
    private void EnablePauseMenu()
    {
        PauseMenuOn = true;
        Player.Instance.PlayerInputIsDisabled = true;
        Time.timeScale = 0;
        pauseMenu.SetActive(true);

        //在暂停的时候调用gc，收回内存，玩家不会注意到卡顿
        System.GC.Collect();

        //高亮选中的按钮
        HighlightButtonForSelectedTab();
    }

    //关闭菜单界面
    public void DisablePauseMenu()
    {
        PauseMenuOn = false;
        Player.Instance.PlayerInputIsDisabled = false;
        Time.timeScale = 1;
        pauseMenu.SetActive(false);
    }

    #region 高亮按钮的方法
    
    /*高亮的意义：因为button按下去后虽然会高亮，但是点击其他地方后会恢复原来的颜色。手动高亮有助于玩家意识到现在选择的哪个panel*/
    private void HighlightButtonForSelectedTab()
    {
        for (int i = 0; i < menuTabs.Length; i++)
        {
            if (menuTabs[i].activeSelf)
            {
                SetButtonColorToActive(menuButtons[i]);
            }

            else
            {
                SetButtonColorToInactive(menuButtons[i]);
            }
        }
    }
    
    private void SetButtonColorToActive(Button button)
    {
        ColorBlock colors = button.colors;

        colors.normalColor = colors.pressedColor;

        button.colors = colors;

    }

    private void SetButtonColorToInactive(Button button)
    {
        ColorBlock colors = button.colors;

        colors.normalColor = colors.disabledColor;

        button.colors = colors;

    }

    #endregion
    
    
    //按钮的点击事件，每个按钮传入对应序号的整数，当点击其中一个按钮时，遍历，打开其对应的界面，同时关掉其他界面
    public void SwitchPauseMenuTab(int tabNum)
    {
        for (int i = 0; i < menuTabs.Length; i++)
        {
            if (i != tabNum)
            {
                menuTabs[i].SetActive(false);
            }
            else
            {
                menuTabs[i].SetActive(true);

            }
        }
        HighlightButtonForSelectedTab();
    }
}
