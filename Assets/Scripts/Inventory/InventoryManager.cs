using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
/// <summary>
/// InventoryManager用来保存玩家的物品
/// </summary>
public class InventoryManager : SingletonMonobehaviour<InventoryManager>, ISaveable
{
    //待使用的数据容器，需拖拽赋值
    [SerializeReference] private SO_ItemList itemList = null;

    //创建一个字典遍历SO_ItemList中的所有物品详细信息，并将它们添加到字典中，以后直接使用字典即可
    private Dictionary<int, ItemDetails> _itemDetailsDictionary;

    //用inventoryLists[]数据列表数组来存储物品，使用inventoryLists[0]来保存背包的物品，inventoryLists[1]来保存箱子的物品
    public List<InventoryItem>[] inventoryLists;
    
    //inventoryListCapacityIntArray[0]表示背包物品数量上限，inventoryListCapacityIntArray[1]表示箱子物品数量上限
    [HideInInspector] public int[] inventoryListCapacityIntArray;

    //数组的下标表示是哪个库存，玩家or箱子？ 数组的值表示当前选中的物品的代码
    private int[] selectedInventoryItem;
    
    
    //保存相关变量
    private string _iSaveableUniqueID;
    public string ISaveableUniqueID { get { return _iSaveableUniqueID; } set { _iSaveableUniqueID = value; } }
    private GameObjectSave _gameObjectSave;
    public GameObjectSave GameObjectSave { get { return _gameObjectSave; } set { _gameObjectSave = value; } }
    private UIInventoryBar inventoryBar;

    
    
    protected override void Awake()
    {
        base.Awake();

        //声明inventoryListCapacityIntArray[2]，inventoryListCapacityIntArray[0]表示的是玩家初始库存
        CreateInventoryList();

        //创建一个字典，其中存储了物品代码和物品详细信息之间的映射。遍历了SO_ItemList中的所有物品详细信息，并将它们添加到字典中
        CreateItemDetailsDictionary();

        //初始化
        selectedInventoryItem = new int[(int)InventoryLocation.count];
        for (int i = 0; i < selectedInventoryItem.Length; i++)
        {
            selectedInventoryItem[i] = -1;
        }
        
        ISaveableUniqueID = GetComponent<GenerateGUID>().GUID;
        GameObjectSave = new GameObjectSave();
        
    }
    
    
    private void OnDisable()
    {
        ISaveableDeregister();
    }


    private void OnEnable()
    {
        ISaveableRegister();
    }

    private void Start()
    {
        inventoryBar = FindObjectOfType<UIInventoryBar>();
    }

    #region Awake初始化相关函数

    //初始化inventoryLists[i]和inventoryListCapacityIntArray[0]，
    private void CreateInventoryList()
    {
        //代码创建了一个数组，但数组中的元素都是 null。for 循环则用来初始化这些元素，使它们指向新创建的列表。
        inventoryLists = new List<InventoryItem>[(int)InventoryLocation.count];

        for (int i = 0; i < (int)InventoryLocation.count; i++)
        {
            inventoryLists[i] = new List<InventoryItem>();
        }

        inventoryListCapacityIntArray = new int[(int)InventoryLocation.count];

        inventoryListCapacityIntArray[(int)InventoryLocation.player] = Settings.playerInitialInventoryCapacity;
    }


    //创建一个字典，其中存储了物品代码和物品详细信息之间的映射。遍历了SO_ItemList中的所有物品详细信息，并将它们添加到字典中
    private void CreateItemDetailsDictionary()
    {
        _itemDetailsDictionary = new Dictionary<int, ItemDetails>();
        foreach (ItemDetails itemDetails in itemList.itemDetails)
        {
            _itemDetailsDictionary.Add(itemDetails.itemCode, itemDetails);
        }
    }

    #endregion




    #region 接口实现

    public void ISaveableRegister()
    {
        SaveLoadManager.Instance.iSaveableObjectList.Add(this);
    }

    public void ISaveableDeregister()
    {
        SaveLoadManager.Instance.iSaveableObjectList.Remove(this);
    }

    public GameObjectSave ISaveableSave()
    {
        // Create new scene save
        SceneSave sceneSave = new SceneSave();

        // Remove any existing scene save for persistent scene for this gameobject
        GameObjectSave.sceneData.Remove(Settings.PersistentScene);

        // Add inventory lists array to persistent scene save
        sceneSave.listInvItemArray = inventoryLists;

        // Add  inventory list capacity array to persistent scene save
        sceneSave.intArrayDictionary = new Dictionary<string, int[]>();
        sceneSave.intArrayDictionary.Add("inventoryListCapacityArray", inventoryListCapacityIntArray);

        // Add scene save for gameobject
        GameObjectSave.sceneData.Add(Settings.PersistentScene, sceneSave);

        return GameObjectSave;
    }


    public void ISaveableLoad(GameSave gameSave)
    {
        if (gameSave.gameObjectData.TryGetValue(ISaveableUniqueID, out GameObjectSave gameObjectSave))
        {
            GameObjectSave = gameObjectSave;

            // Need to find inventory lists - start by trying to locate saveScene for game object
            if (gameObjectSave.sceneData.TryGetValue(Settings.PersistentScene, out SceneSave sceneSave))
            {
                // list inv items array exists for persistent scene
                if (sceneSave.listInvItemArray != null)
                {
                    inventoryLists = sceneSave.listInvItemArray;

                    //  Send events that inventory has been updated
                    for (int i = 0; i < (int)InventoryLocation.count; i++)
                    {
                        EventHandler.CallInventoryUpdatedEvent((InventoryLocation)i, inventoryLists[i]);
                    }

                    // Clear any items player was carrying
                    Player.Instance.ClearCarriedItem();

                    // Clear any highlights on inventory bar
                    inventoryBar.ClearHighlightOnInventorySlots();
                }

                // int array dictionary exists for scene
                if (sceneSave.intArrayDictionary != null && sceneSave.intArrayDictionary.TryGetValue("inventoryListCapacityArray", out int[] inventoryCapacityArray))
                {
                    inventoryListCapacityIntArray = inventoryCapacityArray;
                }
            }

        }
    }

    public void ISaveableStoreScene(string sceneName)
    {
        // Nothing required her since the inventory manager is on a persistent scene;
    }

        public void ISaveableRestoreScene(string sceneName)
    {
        // Nothing required here since the inventory manager is on a persistent scene;
    }

    #endregion


    #region 添加物品的相关函数

    //拾取物品时调用的AddItem函数：添加物体 + 删除场景中的物体
    public void AddItem(InventoryLocation inventoryLocation, Item item, GameObject gameObjectToDelete)
    {
        AddItem(inventoryLocation, item);

        Destroy(gameObjectToDelete);
    }


    //传入item，添加到inventoryLocation（玩家或者箱子）对应的inventoryList里面
    public void AddItem(InventoryLocation inventoryLocation, Item item)
    {
        int itemCode = item.ItemCode;

        List<InventoryItem> inventoryList = inventoryLists[(int)inventoryLocation];

        //检查物品是否已经存在，不在则添加新的，在则在对应的itemPosition处数量+1
        int itemPosition = FindItemInInventory(inventoryLocation, itemCode);

        if (itemPosition != -1)
        {
            AddItemAtPosition(inventoryList, itemCode, itemPosition);
        }
        else
        {
            AddItemAtPosition(inventoryList, itemCode);
        }

        EventHandler.CallInventoryUpdatedEvent(inventoryLocation, inventoryLists[(int)inventoryLocation]);
    }

    //传入itemCode，添加到inventoryLocation（玩家或者箱子）对应的inventoryList里面
    public void AddItem(InventoryLocation inventoryLocation, int itemCode)
    {
        List<InventoryItem> inventoryList = inventoryLists[(int)inventoryLocation];
        
        int itemPosition = FindItemInInventory(inventoryLocation, itemCode);

        if (itemPosition != -1)
        {
            AddItemAtPosition(inventoryList, itemCode, itemPosition);
        }
        else
        {
            AddItemAtPosition(inventoryList, itemCode);
        }
        
        EventHandler.CallInventoryUpdatedEvent(inventoryLocation, inventoryLists[(int)inventoryLocation]);
    }

    //添加新的物品，声明一个新的数据列表元素添加到最后
    private void AddItemAtPosition(List<InventoryItem> inventoryList, int itemCode)
    {
        InventoryItem inventoryItem = new InventoryItem();

        inventoryItem.itemCode = itemCode;
        inventoryItem.itemQuantity = 1;
        
        inventoryList.Add(inventoryItem);

        //DebugPrintInventoryList(inventoryList);
    }


    //在position处添加物品数量，声明一个新的数据列表元素替换掉原来的
    private void AddItemAtPosition(List<InventoryItem> inventoryList, int itemCode, int position)
    {
        InventoryItem inventoryItem = new InventoryItem();

        int quantity = inventoryList[position].itemQuantity + 1;
        
        inventoryItem.itemQuantity = quantity;
        inventoryItem.itemCode = itemCode;
        
        inventoryList[position] = inventoryItem;

        //Debug.ClearDeveloperConsole();
        //DebugPrintInventoryList(inventoryList);
    }


    //在指定的inventoryLists[(int)inventoryLocation]中寻找给定itemCode的东西的位置，不存在则返回-1
    public int FindItemInInventory(InventoryLocation inventoryLocation, int itemCode)
    {
        List<InventoryItem> inventoryList = inventoryLists[(int)inventoryLocation];

        for (int i = 0; i < inventoryList.Count; i++)
        {
            if (inventoryList[i].itemCode == itemCode)
            {
                return i;
            }
        }
        return -1;
    }
    #endregion


    //交换两个物品的位置
    public void SwapInventoryItems(InventoryLocation inventoryLocation, int fromItem, int toItem)
    {
        if (fromItem < inventoryLists[(int)inventoryLocation].Count &&
            toItem < inventoryLists[(int)inventoryLocation].Count && fromItem != toItem && fromItem >= 0 && toItem >= 0)
        {
            InventoryItem fromInventoryItem = inventoryLists[(int)inventoryLocation][fromItem];
            InventoryItem toInventoryItem = inventoryLists[(int)inventoryLocation][toItem];

            inventoryLists[(int)inventoryLocation][toItem] = fromInventoryItem;
            inventoryLists[(int)inventoryLocation][fromItem] = toInventoryItem;

            EventHandler.CallInventoryUpdatedEvent(inventoryLocation, inventoryLists[(int)inventoryLocation]);
        }

    }


    
    #region 查找的相关函数
    
    
    //该方法接受物品代码，返回在_itemDetailsDictionary字典中的，与该代码对应的物品详细信息
    public ItemDetails GetItemDetails(int itemCode)
    {
        ItemDetails itemDetails;

        if (_itemDetailsDictionary.TryGetValue(itemCode, out itemDetails))
        {
            return itemDetails;
        }
        else
        {
            return null;
        }
    }


    //给定物品类型，返回描述这个类型的string的文本
    public string GetItemTypeDescription(ItemType itemType)
    {
        string itemTypeDescription;
        switch (itemType)
        {
            case ItemType.Breaking_tool:
                itemTypeDescription = Settings.BreakingTool;
                break;
            case ItemType.Chopping_tool:
                itemTypeDescription = Settings.ChoppingTool;
                break;
            case ItemType.Hoeing_tool:
                itemTypeDescription = Settings.HoeingTool;
                break;
            case ItemType.Reaping_tool:
                itemTypeDescription = Settings.ReapingTool;
                break;
            case ItemType.Watering_tool:
                itemTypeDescription = Settings.WateringTool;
                break;
            case ItemType.Collecting_tool:
                itemTypeDescription = Settings.CollectingTool;
                break;

            default:
                itemTypeDescription = itemType.ToString();
                break;

        }

        return itemTypeDescription;
    }


    #endregion

    
    
    
    #region 删除物品的相关函数

    public void RemoveItem(InventoryLocation inventoryLocation, int itemCode)
    {
        List<InventoryItem> inventoryList = inventoryLists[(int)inventoryLocation];

        int itemPosition = FindItemInInventory(inventoryLocation, itemCode);

        if (itemPosition != -1)
        {
            RemoveItemAtPosition(inventoryList, itemCode, itemPosition);
        }

        //更新UI显示
        EventHandler.CallInventoryUpdatedEvent(inventoryLocation, inventoryLists[(int)inventoryLocation]);
    }

    private void RemoveItemAtPosition(List<InventoryItem> inventoryList, int itemCode, int position)
    {
        InventoryItem inventoryItem = new InventoryItem();

        int quantity = inventoryList[position].itemQuantity - 1;

        if (quantity > 0)
        {
            inventoryItem.itemQuantity = quantity;
            inventoryItem.itemCode = itemCode;
            inventoryList[position] = inventoryItem;
        }
        else
        {
            inventoryList.RemoveAt(position);
        }

    }
    
    #endregion
    
    
    
    
    #region 当前选中的物体记录，即selectedInventoryItem相关函数

    
    //传入物品的ID，保存到selectedInventoryItem
    public void SetSelectedInventoryItem(InventoryLocation inventoryLocation, int itemCode)
    {
        selectedInventoryItem[(int)inventoryLocation] = itemCode;
    }

    
    //将selectedInventoryItem里，当前选中的物品的ID清除
    public void ClearSelectedInventoryItem(InventoryLocation inventoryLocation)
    {
        selectedInventoryItem[(int)inventoryLocation] = -1;
    }


    //获取当前选中的物体的ID
    private int GetSelectedInventoryItem(InventoryLocation inventoryLocation)
    {
        return selectedInventoryItem[(int)inventoryLocation];
    }

    
    //获取当前选中的物体的itemDetails
    public ItemDetails GetSelectedInventoryItemDetails(InventoryLocation inventoryLocation)
    {
        int itemCode = GetSelectedInventoryItem(inventoryLocation);

        if (itemCode == -1)
        {
            return null;
        }
        else
        {
            return GetItemDetails(itemCode);
        }
    }
    
    #endregion


/*private void DebugPrintInventoryList(List<InventoryItem> inventoryList)
{
    foreach (InventoryItem inventoryItem in inventoryList)
    {
        Debug.Log("Item Description:"+InventoryManager.Instance.GetItemDetails(inventoryItem.itemCode).itemDescription
        +"    Item Quantity"+inventoryItem.itemQuantity);
        Debug.Log("------------------------------------------------");
    }
}*/
    

}
    
    