using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

//声明一个委托MovementDelegate，这个委托有传入参数，因此后面装入该委托的方法应当具有同样参数
//这里采用自定义委托，而不是unity自带的Action是因为Action最多只能传入16个参数
public delegate void MovementDelegate(
    float inputX, float inputY, bool isWalking, bool isRunning, bool isIdle, bool isCarrying,
    ToolEffect toolEffect,
    bool isUsingToolRight, bool isUsingToolLeft, bool isUsingToolUp, bool isUsingToolDown,
    bool isLiftingToolRight, bool isLiftingToolLeft, bool isLiftingToolUp, bool isLiftingToolDown,
    bool isPickingRight, bool isPickingLeft, bool isPickingUp, bool isPickingDown,
    bool isSwingToolRight, bool isSwingToolLeft, bool isSwingToolUp, bool isSwingToolDown,
    bool idleUp, bool idleDown, bool idleRight, bool idleLeft);

public static class EventHandler
{
    //这个事件用来当玩家拾取的时候，对物品栏状态进行更新
    public static event Action<InventoryLocation, List<InventoryItem>> InventoryUpdatedEvent;
    public static void CallInventoryUpdatedEvent(InventoryLocation inventoryLocation,
        List<InventoryItem> inventoryList)
    {
        InventoryUpdatedEvent?.Invoke(inventoryLocation,inventoryList);
    }
    
    
    //依据声明好的委托类型来声明事件
    public static event MovementDelegate MovementEvent;
    public static void CallMovementEvent(float inputX, float inputY, bool isWalking, bool isRunning, bool isIdle,
        bool isCarrying,
        ToolEffect toolEffect,
        bool isUsingToolRight, bool isUsingToolLeft, bool isUsingToolUp, bool isUsingToolDown,
        bool isLiftingToolRight, bool isLiftingToolLeft, bool isLiftingToolUp, bool isLiftingToolDown,
        bool isPickingRight, bool isPickingLeft, bool isPickingUp, bool isPickingDown,
        bool isSwingToolRight, bool isSwingToolLeft, bool isSwingToolUp, bool isSwingToolDown,
        bool idleUp, bool idleDown, bool idleRight, bool idleLeft)
    {
        MovementEvent?.Invoke(inputX, inputY,
            isWalking, isRunning, isIdle, isCarrying,
            toolEffect,
            isUsingToolRight, isUsingToolLeft, isUsingToolUp, isUsingToolDown,
            isLiftingToolRight, isLiftingToolLeft, isLiftingToolUp, isLiftingToolDown,
            isPickingRight, isPickingLeft, isPickingUp, isPickingDown,
            isSwingToolRight, isSwingToolLeft, isSwingToolUp, isSwingToolDown,
            idleUp, idleDown, idleRight, idleLeft);
    }

    
    
    
    #region 处理时间推进的事件
    
    public static event Action<int, Season, int, string, int, int, int> AdvanceGameMinuteEvent;
    public static void CallAdvanceGameMinuteEvent(int gameYear, Season gameSeason, int gameDay, string gameDayOfWeek,
        int gameHour, int gameMinute, int gameSecond)
    {
        AdvanceGameMinuteEvent?.Invoke(gameYear, gameSeason, gameDay, gameDayOfWeek, gameHour, gameMinute, gameSecond);
    }
    
    public static event Action<int, Season, int, string, int, int, int> AdvanceGameHourEvent;
    public static void CallAdvanceGameHourEvent(int gameYear, Season gameSeason, int gameDay, string gameDayOfWeek,
        int gameHour, int gameMinute, int gameSecond)
    {
        AdvanceGameHourEvent?.Invoke(gameYear, gameSeason, gameDay, gameDayOfWeek, gameHour, gameMinute, gameSecond);
    }
    
    public static event Action<int, Season, int, string, int, int, int> AdvanceGameDayEvent;
    public static void CallAdvanceGameDayEvent(int gameYear, Season gameSeason, int gameDay, string gameDayOfWeek,
        int gameHour, int gameMinute, int gameSecond)
    {
        AdvanceGameDayEvent?.Invoke(gameYear, gameSeason, gameDay, gameDayOfWeek, gameHour, gameMinute, gameSecond);
    }
    
    public static event Action<int, Season, int, string, int, int, int> AdvanceGameSeasonEvent;
    public static void CallAdvanceGameSeasonEvent(int gameYear, Season gameSeason, int gameDay, string gameDayOfWeek,
        int gameHour, int gameMinute, int gameSecond)
    {
        AdvanceGameSeasonEvent?.Invoke(gameYear, gameSeason, gameDay, gameDayOfWeek, gameHour, gameMinute, gameSecond);
    }
    
    public static event Action<int, Season, int, string, int, int, int> AdvanceGameYearEvent;
    public static void CallAdvanceGameYearEvent(int gameYear, Season gameSeason, int gameDay, string gameDayOfWeek,
        int gameHour, int gameMinute, int gameSecond)
    {
        AdvanceGameYearEvent?.Invoke(gameYear, gameSeason, gameDay, gameDayOfWeek, gameHour, gameMinute, gameSecond);
    }
    #endregion

    
    
    
    #region 处理场景加载的事件
    
    //场景切换之前，本游戏中，只有场景切换前禁用人物移动的功能用到了这个事件，位于Player脚本中
    public static event Action BeforeSceneUnloadFadeOutEvent;
    public static void CallBeforeSceneUnloadFadeOutEvent()
    {
        BeforeSceneUnloadFadeOutEvent?.Invoke();
    }
    
    public static event Action BeforeSceneUnloadEvent;
    public static void CallBeforeSceneUnloadEvent()
    {
        BeforeSceneUnloadEvent?.Invoke();
    }
    
    public static event Action AfterSceneLoadEvent;
    public static void CallAfterSceneLoadEvent()
    {
        AfterSceneLoadEvent?.Invoke();
    }
    
    //场景切换之后，本游戏中，只有场景切换后启用人物移动的功能用到了这个事件，位于Player脚本中
    public static event Action AfterSceneLoadFadeInEvent;
    public static void CallAfterSceneLoadFadeInEvent()
    {
        AfterSceneLoadFadeInEvent?.Invoke();
        
    }
    
    #endregion
    
    
    
    
    public static event Action DropSelectedItemEvent;
    public static void CallDropSelectedItemEvent()
    {
        DropSelectedItemEvent?.Invoke();
    }


    public static event Action<Vector3, HarvestActionEffect> HarvestActionEffectEvent;
    public static void CallHarvestActionEffectEvent(Vector3 effectPosition, HarvestActionEffect harvestActionEffect)
    {
        HarvestActionEffectEvent?.Invoke(effectPosition, harvestActionEffect);
    }

    public static event Action RemoveSelectedItemFromInventoryEvent;
    public static void CallRemoveSelectedItemFromInventoryEvent()
    {
        RemoveSelectedItemFromInventoryEvent?.Invoke();
    }
    
    //生成作物预制体
    public static event Action InstantiateCropPrefabsEvent;
    public static void CallInstantiateCropPrefabsEvent()
    {
        if (InstantiateCropPrefabsEvent != null)
        {
            InstantiateCropPrefabsEvent();
        }
    }
}