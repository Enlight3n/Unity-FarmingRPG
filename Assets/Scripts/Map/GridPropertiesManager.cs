using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 整体来说，这个类的作用就是，将数据容器中保存的GridProperty细化为GridPropertyDetails，然后按场景装载到GameObjectSave中，
/// 每次切换场景，用gridPropertyDictionary来替换或者被替换
/// </summary>
[RequireComponent(typeof(GenerateGUID))]
public class GridPropertiesManager : SingletonMonobehaviour<GridPropertiesManager>,ISaveable
{
    
    public Grid grid; //暂时未用
    
    //保存玩家当前在的场景的特殊贴图
    private Dictionary<string, GridPropertyDetails> gridPropertyDictionary;
    
    //获取场景中全部的数据容器
    [SerializeField] private SO_GridProperties[] so_gridPropertiesArray = null;

    
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
    }
    
    private void OnDisable()
    {
        ISaveableDeregister();
        EventHandler.AfterSceneLoadEvent -= AfterSceneLoaded;
    }

    private void AfterSceneLoaded()
    {
        grid = GameObject.FindObjectOfType<Grid>();
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
        //
        GameObjectSave.sceneData.Remove(sceneName);

        SceneSave sceneSave = new SceneSave();

        sceneSave.gridPropertyDetailsDictionary = gridPropertyDictionary;
        
        GameObjectSave.sceneData.Add(sceneName,sceneSave);
    }

    
    //根据GameObjectSave的数据恢复当前场景的特殊贴图
    public void ISaveableRestoreScene(string sceneName)
    {
        if (GameObjectSave.sceneData.TryGetValue(sceneName, out SceneSave sceneSave))
        {
            if (sceneSave.gridPropertyDetailsDictionary != null);
            {
                gridPropertyDictionary = sceneSave.gridPropertyDetailsDictionary;
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
            Dictionary<string, GridPropertyDetails> gridPropertyDictionary =
                new Dictionary<string, GridPropertyDetails>();

            //根据数据容器中记载的gridProperty，写入到gridPropertyDetails，并将其添加到字典gridPropertyDictionary中
            foreach (GridProperty gridProperty in so_GridProperties.gridPropertyList)
            {
                GridPropertyDetails gridPropertyDetails;

                //从传入的gridPropertyDictionary中，找到对应xy坐标的特殊贴图的详细信息
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

    public void SetGridPropertyDetails(int gridX, int gridY, GridPropertyDetails gridPropertyDetails)
    {
        SetGridPropertyDetails(gridX,gridY,gridPropertyDetails,gridPropertyDictionary);
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
    
}
