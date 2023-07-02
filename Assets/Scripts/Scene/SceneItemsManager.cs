using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GenerateGUID))]
public class SceneItemsManager : SingletonMonobehaviour<SceneItemsManager>, ISaveable
{
    
    private Transform parentItem; //获取场景中所有可拾取物体的父物体的Item
    
    [SerializeField] private GameObject itemPrefab = null; //预制体中的item，用来作为生成物体的模板

    
    //用于存储和访问这个类的唯一ID
    private string _iSaveableUniqueID;
    public string ISaveableUniqueID
    {
        get { return _iSaveableUniqueID; }
        set { _iSaveableUniqueID = value; }
    }

    //用于存储和访问这个类的游戏对象保存数据
    //GameObjectSave用来保存所有场景中的物品数据
    private GameObjectSave _gameObjectSave;
    public GameObjectSave GameObjectSave
    {
        get { return _gameObjectSave; }
        set { _gameObjectSave = value; }
    }


    protected override void Awake()
    {
        base.Awake();
        
        ISaveableUniqueID = GetComponent<GenerateGUID>().GUID;

        GameObjectSave = new GameObjectSave();
    }

    
    
    #region 场景物体生成和销毁的相关函数
    

    //摧毁场景中所有带Item脚本的物体,这里应该用在恢复场景的时候，先全部删掉，看有什么再逐一补上
    private void DestroySceneItems()
    {
        //寻找场景中，所有具有Item脚本的物体
        Item[] itemInScene = GameObject.FindObjectsOfType<Item>();

        //倒序遍历，逐一销毁
        for (int i = itemInScene.Length - 1; i > -1; i--)
        {
            Destroy(itemInScene[i].gameObject);
        }
    }

    //根据传入的物品代码和位置，生成单个物品
    public void InstantiateSceneItem(int itemCode, Vector3 itemPosition)
    {
        GameObject itemGameObject = Instantiate(itemPrefab, itemPosition, Quaternion.identity, parentItem);

        Item item = itemGameObject.GetComponent<Item>();

        item.Init(itemCode);
    }

    //根据SceneItem的数据列表，逐一生成物体
    private void InstantiateSceneItems(List<SceneItem> sceneItemList)
    {
        GameObject itemGameObject;

        foreach (SceneItem sceneItem in sceneItemList)
        {
            itemGameObject = Instantiate(itemPrefab,
                new Vector3(sceneItem.position.x, sceneItem.position.y, sceneItem.position.z),
                Quaternion.identity, parentItem);

            Item item = itemGameObject.GetComponent<Item>();

            item.ItemCode = sceneItem.itemCode;
            
            item.name = sceneItem.itemName;
        }
    }
    
    #endregion
    
    
    
    //获取Item物体的transform
    private void AfterSceneLoaded()
    {
        parentItem = GameObject.FindGameObjectWithTag(Tags.ItemParentTransform).transform;
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

    #region 接口函数
    
    //启用脚本时，把脚本添加到SaveLoadManager中的iSaveableObjectList
    public void ISaveableDeregister()
    {
        SaveLoadManager.Instance.iSaveableObjectList.Remove(this);
    }
    
    //禁用脚本时，把脚本从SaveLoadManager中的iSaveableObjectList移除
    public void ISaveableRegister()
    {
        SaveLoadManager.Instance.iSaveableObjectList.Add(this);
    }

    //传入加载场景的名字，把场景应该存在中的Item加载出来
    public void ISaveableRestoreScene(string sceneName)
    {
        //这里两个if，保证了如果GameObjectSave中没有保存场景数据的话，将不会执行任何代码，也就是说，如果是新场景的话，也同样适用
        if (GameObjectSave.sceneData.TryGetValue(sceneName, out SceneSave sceneSave))
        {
            if (sceneSave.listSceneItem != null)
            {
                DestroySceneItems();
                InstantiateSceneItems(sceneSave.listSceneItem);
            }
        }
    }
    
    
    //传入保存场景的名字，把场景中应该保存的Item写到场景名对应的sceneItemList中去
    public void ISaveableStoreScene(string sceneName)
    {
        //先去掉场景中对应的SceneSave，后面重新写入
        GameObjectSave.sceneData.Remove(sceneName);

        List<SceneItem> sceneItemList = new List<SceneItem>();

        Item[] itemsInScene = FindObjectsOfType<Item>();

        foreach (Item item in itemsInScene)
        {
            SceneItem sceneItem = new SceneItem();

            sceneItem.itemCode = item.ItemCode;
            sceneItem.position = new Vector3Serializable(item.transform.position.x, item.transform.position.y,
                item.transform.position.z);
            sceneItem.itemName = item.name;
            sceneItemList.Add(sceneItem);
        }

        SceneSave sceneSave = new SceneSave();

        sceneSave.listSceneItem = new List<SceneItem>();

        sceneSave.listSceneItem = sceneItemList;
        
        GameObjectSave.sceneData.Add(sceneName,sceneSave);
    }
    #endregion
}
