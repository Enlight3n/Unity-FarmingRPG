using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 这个是对单个特殊贴图性质的补充
/// </summary>
[System.Serializable]
public class GridPropertyDetails
{
    public int gridX;
    public int gridY;
    
    public bool isDiggable;
    public bool canDropItem;
    public bool canPlaceFurniture;
    public bool isPath;
    public bool isNPCObstacle;

    public int daysSinceDug = -1;
    public int daysSinceWatered = -1;
    public int seedItemCode = -1;
    public int growthDays = -1; //作物至今的生长天数
    public int daysSinceLastHarvest = -1;

    public GridPropertyDetails()
    {
        
    }

}
