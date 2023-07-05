using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;


/// <summary>
/// <para>这个类的作用就是管理网格的GridPropertyDetails——将数据容器中的GridProperty细化为GridPropertyDetails</para>
/// <para>场景保存的时候——保存旧场景的网格数据</para>
/// <para>场景加载的时候——加载新场景的网格数据，接着清除再显示新场景的锄地、浇水贴图，作物</para>
/// <para>日期推进的时候——清除再显示当前场景的锄地、浇水贴图，作物</para>
/// <para>我们用gridPropertyDictionary来指明当前场景的网格数据</para>
/// </summary>
[RequireComponent(typeof(GenerateGUID))]
public class GridPropertiesManager : SingletonMonobehaviour<GridPropertiesManager>,ISaveable
{
    private Transform cropParentTransform;
    
    private Grid grid; //暂时未用
    private Tilemap groundDecoration1; //锄地的地面
    private Tilemap groundDecoration2; //锄地+浇水的地面
    [SerializeField] private Tile[] dugGround = null; //保存锄地贴图
    [SerializeField] private Tile[] wateredGround = null; //保存浇水贴图
    
    //保存玩家当前所在场景的特殊贴图
    private Dictionary<string, GridPropertyDetails> gridPropertyDictionary;
    
    //获取场景中全部的数据容器
    [SerializeField] private SO_GridProperties[] so_gridPropertiesArray = null;

    //保存种植信息的数据容器
    [SerializeField] private SO_CropDetailsList so_CropDetailsList = null;
    
    private string _iSaveableUniqueID;
    public string ISaveableUniqueID { get { return _iSaveableUniqueID; } set { _iSaveableUniqueID = value; } }
    
    
    private GameObjectSave _gameObjectSave;
    public GameObjectSave GameObjectSave { get { return _gameObjectSave; } set { _gameObjectSave = value; } }
    
    
    protected override void Awake()
    {
        base.Awake();
        ISaveableUniqueID = GetComponent<GenerateGUID>().GUID;
        GameObjectSave = new GameObjectSave();
    }

    private void OnEnable()
    {
        ISaveableRegister();
        EventHandler.AfterSceneLoadEvent += AfterSceneLoaded;
        EventHandler.AdvanceGameDayEvent += AdvanceDay;
    }
    
    private void OnDisable()
    {
        ISaveableDeregister();
        EventHandler.AfterSceneLoadEvent -= AfterSceneLoaded;
        EventHandler.AdvanceGameDayEvent -= AdvanceDay;
    }


    private void AfterSceneLoaded()
    {
        if((GameObject.FindGameObjectWithTag(Tags.CropsParentTransform)!=null))
        {
            cropParentTransform = GameObject.FindGameObjectWithTag(Tags.CropsParentTransform).transform;
        }
        else
        {
            cropParentTransform = null;
        }
        
        grid = GameObject.FindObjectOfType<Grid>();
        
        
        groundDecoration1 = GameObject.FindGameObjectWithTag(Tags.GroundDecoration1).GetComponent<Tilemap>();
        groundDecoration2 = GameObject.FindGameObjectWithTag(Tags.GroundDecoration2).GetComponent<Tilemap>();
    }

    private void AdvanceDay(int gameYear, Season gameSeason, int gameDay, string gameDayOfWeek, int gameHour,
        int gameMinute, int gameSecond)
    {
        //清除
        ClearDisplayGridPropertyDetails();

        //遍历多个场景的数据容器
        foreach (SO_GridProperties so_GridProperties in so_gridPropertiesArray)
        {
            //获取场景的数据
            if (GameObjectSave.sceneData.TryGetValue(so_GridProperties.sceneName.ToString(), out SceneSave sceneSave))
            {
                if (sceneSave.gridPropertyDetailsDictionary != null)
                {
                    //处理gridPropertyDetails的字典
                    for (int i = sceneSave.gridPropertyDetailsDictionary.Count - 1; i >= 0; i--)
                    {
                        KeyValuePair<string, GridPropertyDetails> item =
                            sceneSave.gridPropertyDetailsDictionary.ElementAt(i);

                        GridPropertyDetails gridPropertyDetails = item.Value;

                        #region 随着日子推进，更新相应网格的性质
                        
                        //若有作物，长一天
                        if (gridPropertyDetails.growthDays > -1)
                        {
                            gridPropertyDetails.growthDays++;
                        }
                        
                        //若已浇水，则恢复为未浇水状态
                        if (gridPropertyDetails.daysSinceWatered > -1)
                        {
                            gridPropertyDetails.daysSinceWatered = -1;
                        }

                        //保存gridPropertyDetails
                        SetGridPropertyDetails(gridPropertyDetails.gridX, gridPropertyDetails.gridY,
                            gridPropertyDetails, sceneSave.gridPropertyDetailsDictionary);

                        #endregion 
                    }
                }
            }
        }
    
        //显示
        DisplayGridPropertyDetails();
    }

    #region ISaveable接口
    public void ISaveableDeregister()
    {
        SaveLoadManager.Instance.iSaveableObjectList.Remove(this);
    }
    
    //禁用脚本时，把脚本从SaveLoadManager中的iSaveableObjectList移除
    public void ISaveableRegister()
    {
        SaveLoadManager.Instance.iSaveableObjectList.Add(this);
    }

    
    //先删除掉保存的当前场景中的特殊贴图数据，再将当前场景的信息写入
    public void ISaveableStoreScene(string sceneName)
    {
        GameObjectSave.sceneData.Remove(sceneName);

        SceneSave sceneSave = new SceneSave();

        sceneSave.gridPropertyDetailsDictionary = gridPropertyDictionary;

        GameObjectSave.sceneData.Add(sceneName, sceneSave);
    }

    
    //根据GameObjectSave的数据恢复当前场景的特殊贴图
    public void ISaveableRestoreScene(string sceneName)
    {
        if (GameObjectSave.sceneData.TryGetValue(sceneName, out SceneSave sceneSave))
        {
            if (sceneSave.gridPropertyDetailsDictionary != null)
            {
                gridPropertyDictionary = sceneSave.gridPropertyDetailsDictionary;
            }
            
            //遍历这个gridPropertyDictionary，根据其性质，逐一修改单个瓦片
            if (gridPropertyDictionary.Count > 0)
            {
                ClearDisplayGridPropertyDetails();
                
                DisplayGridPropertyDetails();
            }
        }
    }
    #endregion
    
    
    
    
    
    private void Start()
    {
        InitialiseGridProperties();
    }
    
    
    /// <summary>
    /// 遍历全部场景的数据容器，根据数据容器中记载的gridProperty，更新对应的gridPropertyDetails，
    /// 并将其添加到字典<key, gridPropertyDictionary>中，然后将字典赋值到sceneSave.gridPropertyDetailsDictionary
    /// 最后把sceneSave.gridPropertyDetailsDictionary字典，按照场景名保存到GameObjectSave.sceneData中
    /// 这其中，用全局字段gridPropertyDictionary保存当前场景的特殊贴图数据
    /// </summary>
    private void InitialiseGridProperties()
    {
        foreach (SO_GridProperties so_GridProperties in so_gridPropertiesArray)
        {
            //每个场景都声明一个gridPropertyDictionary
            Dictionary<string, GridPropertyDetails> gridPropertyDictionary =
                new Dictionary<string, GridPropertyDetails>();

            //根据数据容器中记载的gridProperty，写入到gridPropertyDetails，并将其添加到字典gridPropertyDictionary中
            foreach (GridProperty gridProperty in so_GridProperties.gridPropertyList)
            {
                GridPropertyDetails gridPropertyDetails;

                //从传入的gridPropertyDictionary中，找到对应xy坐标的特殊贴图的详细信息，按理来说是空的，但还是看看有没有
                gridPropertyDetails = GetGridPropertyDetails(gridProperty.gridCoordinate.x,
                    gridProperty.gridCoordinate.y, gridPropertyDictionary);

                if (gridPropertyDetails == null)
                {
                    gridPropertyDetails = new GridPropertyDetails();
                }
                
                switch(gridProperty.gridBoolProperty)
                {
                    case GridBoolProperty.diggable:
                        gridPropertyDetails.isDiggable = gridProperty.gridBoolValue;
                        break;
                    case GridBoolProperty.canDropItem:
                        gridPropertyDetails.canDropItem = gridProperty.gridBoolValue;
                        break;
                    case GridBoolProperty.canPlaceFurniture:
                        gridPropertyDetails.canPlaceFurniture = gridProperty.gridBoolValue;
                        break;
                    case GridBoolProperty.isPath:
                        gridPropertyDetails.isPath = gridProperty.gridBoolValue;
                        break;
                    case GridBoolProperty.isNPCObstacle:
                        gridPropertyDetails.isNPCObstacle = gridProperty.gridBoolValue;
                        break;
                }

                //把xy坐标保存到gridPropertyDetails中，并为gridPropertyDictionary新设键值对<key, gridPropertyDetails>
                SetGridPropertyDetails(gridProperty.gridCoordinate.x, gridProperty.gridCoordinate.y,
                    gridPropertyDetails, gridPropertyDictionary);
            }

            //将字典添加到sceneSave.gridPropertyDetailsDictionary中
            SceneSave sceneSave = new SceneSave();
            sceneSave.gridPropertyDetailsDictionary = gridPropertyDictionary;

            if (so_GridProperties.sceneName.ToString() == SceneControllerManager.Instance.startingSceneName.ToString())
            {
                this.gridPropertyDictionary = gridPropertyDictionary;
            }

            //把sceneSave.gridPropertyDetailsDictionary字典，按照场景名保存到GameObjectSave.sceneData中
            GameObjectSave.sceneData.Add(so_GridProperties.sceneName.ToString(), sceneSave);
        }
    }
    
    
    #region (private)重要方法：清除和显示全部：锄地、浇水贴图，作物
    
    private void ClearDisplayGridPropertyDetails()
    {
        //清除锄地浇水的瓦片贴图
        ClearDisplayGroundDecorations(); 

        //清除所有的作物
        ClearDisplayAllPlantedCrops();
    }

    

    private void DisplayGridPropertyDetails()
    {
        foreach (KeyValuePair<string, GridPropertyDetails> item in gridPropertyDictionary)
        {
            GridPropertyDetails gridPropertyDetails = item.Value;

            DisplayDugGround(gridPropertyDetails);
            
            DisplayWateredGround(gridPropertyDetails);

            DisplayPlantedCrop(gridPropertyDetails);
        }
    }
    
    #endregion



    #region 销毁和重生物体（这些方法都被上面两个重要方法调用）

    #region (private)清除全部的锄地和浇水贴图 + 销毁作物
    
    //清除所有的锄地和浇水贴图
    private void ClearDisplayGroundDecorations()
    {
        groundDecoration1.ClearAllTiles();
        groundDecoration2.ClearAllTiles();
    }
    
    //销毁所有的作物
    private void ClearDisplayAllPlantedCrops()
    {
        Crop[] cropArray;
        cropArray = FindObjectsOfType<Crop>();

        foreach (Crop crop in cropArray)
        {
            Destroy(crop.gameObject);
        }
    }
    #endregion
    

    #region (public)生成作物——根据单个网格的gridPropertyDetails
    
    //根据网格的gridPropertyDetails，计算出作物的阶段，生成作物，并给他们添加Crop脚本组件
    public void DisplayPlantedCrop(GridPropertyDetails gridPropertyDetails)
    {
        if (gridPropertyDetails.seedItemCode > -1)
        {
            
            CropDetails cropDetails = so_CropDetailsList.GetCropDetails(gridPropertyDetails.seedItemCode);

            if (cropDetails != null)
            {
                int growthStages = cropDetails.growthDays.Length;
                int currentGrowthStage = 0;

                //通过作物的生长周期，每个周期的生长天数，总成熟天数，以及当前生长天数，计算出当前的阶段
                for (int i = growthStages - 1; i >= 0; i--)
                {
                    if (gridPropertyDetails.growthDays >= cropDetails.growthDays[i]) //growthDays[i]是作物i阶段所需的生长天数
                    {
                        currentGrowthStage = i;
                        break;
                    }
                }


                GameObject cropPrefab = cropDetails.growthPrefab[currentGrowthStage];

                Sprite growthSprite = cropDetails.growthSprite[currentGrowthStage];

                Vector3 worldPosition =
                    groundDecoration2.CellToWorld(new Vector3Int(gridPropertyDetails.gridX, gridPropertyDetails.gridY,
                        0));

                //在网格底边中心生成作物
                worldPosition = new Vector3(worldPosition.x + Settings.gridCellSize / 2, worldPosition.y,
                    worldPosition.z);

                GameObject cropInstance = Instantiate(cropPrefab, worldPosition, Quaternion.identity);

                cropInstance.GetComponentInChildren<SpriteRenderer>().sprite = growthSprite;

                cropInstance.transform.SetParent(cropParentTransform);

                cropInstance.GetComponent<Crop>().cropGridPosition =
                    new Vector2Int(gridPropertyDetails.gridX, gridPropertyDetails.gridY);
            }
        }
    }
    
    #endregion

    
    #region 显示锄地和浇水后的单个瓦片贴图
    
    #region (public)显示的主要方法 

    public void DisplayDugGround(GridPropertyDetails gridPropertyDetails)
    {
        //如果>-1，说明这块地被锄过，则应该显示
        if (gridPropertyDetails.daysSinceDug > -1)
        {
            ConnectDugGround(gridPropertyDetails);
        }
    }
    public void DisplayWateredGround(GridPropertyDetails gridPropertyDetails)
    {
        // Watered
        if (gridPropertyDetails.daysSinceWatered > -1)
        {
            ConnectWateredGround(gridPropertyDetails);
        }
    }

    #endregion

    
    #region (private)显示的主要方法调用到的方法

    private void ConnectDugGround(GridPropertyDetails gridPropertyDetails)
    {
        //根据周围的瓦片是否被挖掘来寻找合适的瓦片
        Tile dugTile0 = SetDugTile(gridPropertyDetails.gridX, gridPropertyDetails.gridY);
        
        //设置合适的瓦片
        groundDecoration1.SetTile(new Vector3Int(gridPropertyDetails.gridX, gridPropertyDetails.gridY, 0), 
            dugTile0);

        //接下来处理周围四个网格的贴图
        GridPropertyDetails adjacentGridPropertyDetails;

        adjacentGridPropertyDetails = GetGridPropertyDetails(gridPropertyDetails.gridX, gridPropertyDetails.gridY + 1);
        if (adjacentGridPropertyDetails != null && adjacentGridPropertyDetails.daysSinceDug > -1)
        {
            Tile dugTile1 = SetDugTile(gridPropertyDetails.gridX, gridPropertyDetails.gridY + 1);
            groundDecoration1.SetTile(new Vector3Int(gridPropertyDetails.gridX, gridPropertyDetails.gridY + 1, 0),
                dugTile1);
        }

        adjacentGridPropertyDetails = GetGridPropertyDetails(gridPropertyDetails.gridX, gridPropertyDetails.gridY - 1);
        if (adjacentGridPropertyDetails != null && adjacentGridPropertyDetails.daysSinceDug > -1)
        {
            Tile dugTile2 = SetDugTile(gridPropertyDetails.gridX, gridPropertyDetails.gridY - 1);
            groundDecoration1.SetTile(new Vector3Int(gridPropertyDetails.gridX, gridPropertyDetails.gridY - 1, 0),
                dugTile2);
        }
        adjacentGridPropertyDetails = GetGridPropertyDetails(gridPropertyDetails.gridX - 1, gridPropertyDetails.gridY);
        if (adjacentGridPropertyDetails != null && adjacentGridPropertyDetails.daysSinceDug > -1)
        {
            Tile dugTile3 = SetDugTile(gridPropertyDetails.gridX - 1, gridPropertyDetails.gridY);
            groundDecoration1.SetTile(new Vector3Int(gridPropertyDetails.gridX - 1, gridPropertyDetails.gridY, 0),
                dugTile3);
        }

        adjacentGridPropertyDetails = GetGridPropertyDetails(gridPropertyDetails.gridX + 1, gridPropertyDetails.gridY);
        if (adjacentGridPropertyDetails != null && adjacentGridPropertyDetails.daysSinceDug > -1)
        {
            Tile dugTile4 = SetDugTile(gridPropertyDetails.gridX + 1, gridPropertyDetails.gridY);
            groundDecoration1.SetTile(new Vector3Int(gridPropertyDetails.gridX + 1, gridPropertyDetails.gridY, 0),
                dugTile4);
        }
    }
    
    private void ConnectWateredGround(GridPropertyDetails gridPropertyDetails)
    {
        // Select tile based on surrounding watered tiles

        Tile wateredTile0 = SetWateredTile(gridPropertyDetails.gridX, gridPropertyDetails.gridY);
        groundDecoration2.SetTile(new Vector3Int(gridPropertyDetails.gridX, gridPropertyDetails.gridY, 0), wateredTile0);

        // Set 4 tiles if watered surrounding current tile - up, down, left, right now that this central tile has been watered

        GridPropertyDetails adjacentGridPropertyDetails;

        adjacentGridPropertyDetails = GetGridPropertyDetails(gridPropertyDetails.gridX, gridPropertyDetails.gridY + 1);
        if (adjacentGridPropertyDetails != null && adjacentGridPropertyDetails.daysSinceWatered > -1)
        {
            Tile wateredTile1 = SetWateredTile(gridPropertyDetails.gridX, gridPropertyDetails.gridY + 1);
            groundDecoration2.SetTile(new Vector3Int(gridPropertyDetails.gridX, gridPropertyDetails.gridY + 1, 0), wateredTile1);
        }

        adjacentGridPropertyDetails = GetGridPropertyDetails(gridPropertyDetails.gridX, gridPropertyDetails.gridY - 1);
        if (adjacentGridPropertyDetails != null && adjacentGridPropertyDetails.daysSinceWatered > -1)
        {
            Tile wateredTile2 = SetWateredTile(gridPropertyDetails.gridX, gridPropertyDetails.gridY - 1);
            groundDecoration2.SetTile(new Vector3Int(gridPropertyDetails.gridX, gridPropertyDetails.gridY - 1, 0), wateredTile2);
        }

        adjacentGridPropertyDetails = GetGridPropertyDetails(gridPropertyDetails.gridX - 1, gridPropertyDetails.gridY);
        if (adjacentGridPropertyDetails != null && adjacentGridPropertyDetails.daysSinceWatered > -1)
        {
            Tile wateredTile3 = SetWateredTile(gridPropertyDetails.gridX - 1, gridPropertyDetails.gridY);
            groundDecoration2.SetTile(new Vector3Int(gridPropertyDetails.gridX - 1, gridPropertyDetails.gridY, 0), wateredTile3);
        }

        adjacentGridPropertyDetails = GetGridPropertyDetails(gridPropertyDetails.gridX + 1, gridPropertyDetails.gridY);
        if (adjacentGridPropertyDetails != null && adjacentGridPropertyDetails.daysSinceWatered > -1)
        {
            Tile wateredTile4 = SetWateredTile(gridPropertyDetails.gridX + 1, gridPropertyDetails.gridY);
            groundDecoration2.SetTile(new Vector3Int(gridPropertyDetails.gridX + 1, gridPropertyDetails.gridY, 0), wateredTile4);
        }
    }
    
    //传入xy坐标，根据周围格子的性质，传出当前锄的格子的恰当贴图
    private Tile SetDugTile(int xGrid, int yGrid)
    {
        //获取周围的网格是否被锄过
        bool upDug = IsGridSquareDug(xGrid, yGrid + 1);
        bool downDug = IsGridSquareDug(xGrid, yGrid - 1);
        bool leftDug = IsGridSquareDug(xGrid - 1, yGrid);
        bool rightDug = IsGridSquareDug(xGrid + 1, yGrid);

        #region 根据周围格子是否被锄过来设置合适的瓦片贴图

        if (!upDug && !downDug && !rightDug && !leftDug)
        {
            return dugGround[0];
        }
        else if (!upDug && downDug && rightDug && !leftDug)
        {
            return dugGround[1];
        }
        else if (!upDug && downDug && rightDug && leftDug)
        {
            return dugGround[2];
        }
        else if (!upDug && downDug && !rightDug && leftDug)
        {
            return dugGround[3];
        }
        else if (!upDug && downDug && !rightDug && !leftDug)
        {
            return dugGround[4];
        }
        else if (upDug && downDug && rightDug && !leftDug)
        {
            return dugGround[5];
        }
        else if (upDug && downDug && rightDug && leftDug)
        {
            return dugGround[6];
        }
        else if (upDug && downDug && !rightDug && leftDug)
        {
            return dugGround[7];
        }
        else if (upDug && downDug && !rightDug && !leftDug)
        {
            return dugGround[8];
        }
        else if (upDug && !downDug && rightDug && !leftDug)
        {
            return dugGround[9];
        }
        else if (upDug && !downDug && rightDug && leftDug)
        {
            return dugGround[10];
        }
        else if (upDug && !downDug && !rightDug && leftDug)
        {
            return dugGround[11];
        }
        else if (upDug && !downDug && !rightDug && !leftDug)
        {
            return dugGround[12];
        }
        else if (!upDug && !downDug && rightDug && !leftDug)
        {
            return dugGround[13];
        }
        else if (!upDug && !downDug && rightDug && leftDug)
        {
            return dugGround[14];
        }
        else if (!upDug && !downDug && !rightDug && leftDug)
        {
            return dugGround[15];
        }

        return null;

        #endregion 
    }
    
    private Tile SetWateredTile(int xGrid, int yGrid)
    {
        //获取相邻的网格是否被浇过水
        bool upWatered = IsGridSquareWatered(xGrid, yGrid + 1);
        bool downWatered = IsGridSquareWatered(xGrid, yGrid - 1);
        bool leftWatered = IsGridSquareWatered(xGrid - 1, yGrid);
        bool rightWatered = IsGridSquareWatered(xGrid + 1, yGrid);

        #region Set appropriate tile based on whether surrounding tiles are watered or not

        if (!upWatered && !downWatered && !rightWatered && !leftWatered)
        {
            return wateredGround[0];
        }
        else if (!upWatered && downWatered && rightWatered && !leftWatered)
        {
            return wateredGround[1];
        }
        else if (!upWatered && downWatered && rightWatered && leftWatered)
        {
            return wateredGround[2];
        }
        else if (!upWatered && downWatered && !rightWatered && leftWatered)
        {
            return wateredGround[3];
        }
        else if (!upWatered && downWatered && !rightWatered && !leftWatered)
        {
            return wateredGround[4];
        }
        else if (upWatered && downWatered && rightWatered && !leftWatered)
        {
            return wateredGround[5];
        }
        else if (upWatered && downWatered && rightWatered && leftWatered)
        {
            return wateredGround[6];
        }
        else if (upWatered && downWatered && !rightWatered && leftWatered)
        {
            return wateredGround[7];
        }
        else if (upWatered && downWatered && !rightWatered && !leftWatered)
        {
            return wateredGround[8];
        }
        else if (upWatered && !downWatered && rightWatered && !leftWatered)
        {
            return wateredGround[9];
        }
        else if (upWatered && !downWatered && rightWatered && leftWatered)
        {
            return wateredGround[10];
        }
        else if (upWatered && !downWatered && !rightWatered && leftWatered)
        {
            return wateredGround[11];
        }
        else if (upWatered && !downWatered && !rightWatered && !leftWatered)
        {
            return wateredGround[12];
        }
        else if (!upWatered && !downWatered && rightWatered && !leftWatered)
        {
            return wateredGround[13];
        }
        else if (!upWatered && !downWatered && rightWatered && leftWatered)
        {
            return wateredGround[14];
        }
        else if (!upWatered && !downWatered && !rightWatered && leftWatered)
        {
            return wateredGround[15];
        }

        return null;

        #endregion Set appropriate tile based on whether surrounding tiles are watered or not
    }
    
    
    //确认是否坐标为xy的格子被锄过
    private bool IsGridSquareDug(int xGrid, int yGrid)
    {
        GridPropertyDetails gridPropertyDetails = GetGridPropertyDetails(xGrid, yGrid);

        if (gridPropertyDetails == null)
        {
            return false;
        }
        else if (gridPropertyDetails.daysSinceDug > -1)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    
    private bool IsGridSquareWatered(int xGrid, int yGrid)
    {
        GridPropertyDetails gridPropertyDetails = GetGridPropertyDetails(xGrid, yGrid);

        if (gridPropertyDetails == null)
        {
            return false;
        }
        else if (gridPropertyDetails.daysSinceWatered > -1)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    
    #endregion
    #endregion
    #endregion
    
    
    
    
    #region 设置和获取的网格详细性质(public)
    
    public void SetGridPropertyDetails(int gridX, int gridY, GridPropertyDetails gridPropertyDetails)
    {
        SetGridPropertyDetails(gridX, gridY, gridPropertyDetails, gridPropertyDictionary);
    }
    
    
    //把xy坐标保存到gridPropertyDetails中，并为gridPropertyDictionary新设键值对<key, gridPropertyDetails>
    public void SetGridPropertyDetails(int gridX, int gridY, GridPropertyDetails gridPropertyDetails,
        Dictionary<string, GridPropertyDetails> gridPropertyDictionary)
    {
        string key = "x" + gridX + "y" + gridY;

        gridPropertyDetails.gridX = gridX;
        gridPropertyDetails.gridY = gridY;

        gridPropertyDictionary[key] = gridPropertyDetails;
    }
    
    
    public GridPropertyDetails GetGridPropertyDetails(int gridX, int gridY)
    {
        return GetGridPropertyDetails(gridX, gridY, gridPropertyDictionary);
    }
    
    
    //从gridPropertyDictionary中，找到对应xy坐标的特殊贴图的详细信息
    public GridPropertyDetails GetGridPropertyDetails(int gridX, int gridY,
        Dictionary<string, GridPropertyDetails> gridPropertyDictionary)
    {
        string key = "x" + gridX + "y" + gridY;

        GridPropertyDetails gridPropertyDetails;

        if (!gridPropertyDictionary.TryGetValue(key, out gridPropertyDetails))
        {
            return null;
        }
        else
        {
            return gridPropertyDetails;
        }
    }
    
    #endregion
    
    //根据网格的gridPropertyDetails来获取其上作物具有的Crop脚本组件
    public Crop GetCropObjectAtGridLocation(GridPropertyDetails gridPropertyDetails)
    {
        //获取网格中心的世界坐标
        Vector3 worldPosition =
            grid.GetCellCenterWorld(new Vector3Int(gridPropertyDetails.gridX, gridPropertyDetails.gridY, 0));
        Collider2D[] collider2DArray = Physics2D.OverlapPointAll(worldPosition);

        
        Crop crop = null;

        for (int i = 0; i < collider2DArray.Length; i++)
        {
            //GetComponentInParent逐层向上查找，从自己这一层开始递归
            crop = collider2DArray[i].gameObject.GetComponentInParent<Crop>();
            if (crop != null && crop.cropGridPosition ==
                new Vector2Int(gridPropertyDetails.gridX, gridPropertyDetails.gridY))
                break;
            //GetComponentInChildren逐层向下查找，从自己这一层开始递归
            crop = collider2DArray[i].gameObject.GetComponentInChildren<Crop>();
            if (crop != null && crop.cropGridPosition ==
                new Vector2Int(gridPropertyDetails.gridX, gridPropertyDetails.gridY))
                break;
            //所以这两个无论注释哪一个都能正常运行，因为crop和BoxCollider2D是属于同一component
        }

        return crop;
    }
    
    //根据种子ID获取其作物详情
    public CropDetails GetCropDetails(int seedItemCode)
    {
        return so_CropDetailsList.GetCropDetails(seedItemCode);
    }
}
