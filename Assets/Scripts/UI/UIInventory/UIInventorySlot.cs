using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.Build.Content;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class UIInventorySlot : MonoBehaviour, IBeginDragHandler,IDragHandler,IEndDragHandler, IPointerEnterHandler,IPointerExitHandler,IPointerClickHandler
{

    //处理鼠标对UIslot的拖拽
    
    [SerializeField] private UIInventoryBar inventoryBar = null; //获取父物体的脚本
    private Camera mainCamera; //为了使用Camera.main.ScreenToWorldPoint
    private Transform parentItem; //保存场景中，所有可互动物体的父物体Items的transform
    private GameObject draggedItem; //鼠标拖拽的物体，直接生成出来
    [SerializeField] private GameObject itemPrefab = null; //itemPrefab用来生成在场景中的物体
    
    [SerializeField] private int slotNumber = 0; //给物品栏编号，方便交换位置

    private GridCursor gridCursor; //获取gridCursor脚本
    
    
    //自带的一些属性
    public Image inventorySlotImage; 
    public TextMeshProUGUI textMeshProUGUI;
    [HideInInspector] public ItemDetails itemDetails;
    [HideInInspector] public int itemQuantity;
    
    
    
    //用于鼠标悬浮显示物品描述
    private Canvas parentCanvas;
    [SerializeField] private GameObject inventoryTextBoxPrefab = null; //详情文本框预制体
    
    //
    public Image inventorySlotHighlight; //框选中的图片
    [HideInInspector] public bool isSelected = false; //用于表面是否被选中

    private Cursor cursor;
    
    private void Awake()
    {
        parentCanvas = GetComponentInParent<Canvas>();
    }
    

    private void Start()
    {
        mainCamera = Camera.main; //Camera.main返回当前激活的相机
        gridCursor = FindObjectOfType<GridCursor>();
        cursor = FindObjectOfType<Cursor>();
    }

    
    private void OnEnable()
    {
        EventHandler.AfterSceneLoadEvent += SceneLoaded;
        EventHandler.DropSelectedItemEvent += DropSelectedItemAtMousePosition;
    }

    private void OnDisable()
    {
        EventHandler.AfterSceneLoadEvent -= SceneLoaded;
        EventHandler.DropSelectedItemEvent -= DropSelectedItemAtMousePosition;
    }

    private void SceneLoaded()
    {
        //这句话之前是放在start方法中的，但是因为我们开始的时候要等待场景加载完再来寻找，所以改写成事件的形式
        parentItem = GameObject.FindGameObjectWithTag(Tags.ItemParentTransform).transform; //找到场景中所有可拾取物体的父物体
    }


    #region 鼠标拖动UI开始互动
    
    
    //处理开始拖动的事件：禁用移动-生成物体-赋予当前物品栏的图像-设置选中框
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (itemDetails != null)
        {
            Player.Instance.DisablePlayerInputAndResetMovement();

            //根据脚本生成物体
            draggedItem = Instantiate(inventoryBar.InventoryBarDraggedItem, inventoryBar.transform);

            //获取生成物体的子物体的Iamge组件
            Image draggedItemImage = draggedItem.GetComponentInChildren<Image>();
            
            //设置生成的物体的子物体的图片为当前Slot的图片
            draggedItemImage.sprite = inventorySlotImage.sprite;
            
            SetSelectedItem();
        }
    }

    //处理拖动中的事件：生成的物品跟随鼠标移动
    public void OnDrag(PointerEventData eventData)
    {
        if (draggedItem != null)
        {
            draggedItem.transform.position = Input.mousePosition;
        }
    }

    //处理结束拖动的事件：摧毁draggedItem-获取鼠标所在位置-检查是物品栏交换还是丢弃-恢复玩家移动
    public void OnEndDrag(PointerEventData eventData)
    {
        if (draggedItem != null)
        {
            Destroy(draggedItem);
            
            //检查鼠标指针当前所在位置是否有一个游戏对象，并且该游戏对象是否具有UIInventorySlot组件。
            //如果是，则获取该组件的slotNumber属性，并将其存储在变量toSlotNumber中。
            //然后，调用InventoryManager.Instance.SwapInventoryItems(InventoryLocation.player, slotNumber, toSlotNumber)方法，
            //交换两个物品栏中的物品。
            if (eventData.pointerCurrentRaycast.gameObject != null &&
                eventData.pointerCurrentRaycast.gameObject.GetComponent<UIInventorySlot>() != null)
            {
                int toSlotNumber = eventData.pointerCurrentRaycast.gameObject.GetComponent<UIInventorySlot>().slotNumber;
                //交换背包中的物品位置，并更新图标
                InventoryManager.Instance.SwapInventoryItems(InventoryLocation.player, slotNumber, toSlotNumber);
                
                DestroyInventoryTextBar();
                
                ClearSelectedItem();
            }

            else
            {
                //检查是否可以被丢弃
                if (itemDetails.canBeDropped)
                {
                    DropSelectedItemAtMousePosition();
                }
            }
            Player.Instance.EnablePlayerInput();
        }
    }
    
    // 在鼠标位置放置物体
    private void DropSelectedItemAtMousePosition()
    {
        /*if (itemDetails != null && isSelected)
        {
            //获取鼠标位置
            Vector3 worldPosition = mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x,
                Input.mousePosition.y, -mainCamera.transform.position.z));

            Vector3Int gridPosition = GridPropertiesManager.Instance.grid.WorldToCell(worldPosition);
            GridPropertyDetails gridPropertyDetails =
                GridPropertiesManager.Instance.GetGridPropertyDetails(gridPosition.x, gridPosition.y);
            
            //判断这个格子是否可以放置物品
            if (gridPropertyDetails != null && gridPropertyDetails.canDropItem)
            {
                //生成itemPrefab
                GameObject itemGameObject = Instantiate(itemPrefab,
                    new Vector3(worldPosition.x, worldPosition.y - Settings.gridCellSize / 2f, worldPosition.z),
                    Quaternion.identity, parentItem);

                //添加脚本item，item会根据物品代码自动初始化图片
                Item item = itemGameObject.GetComponent<Item>();
                item.ItemCode = itemDetails.itemCode;

                //从InventoryManager的背包中删掉此物品
                InventoryManager.Instance.RemoveItem(InventoryLocation.player, item.ItemCode);

                if (InventoryManager.Instance.FindItemInInventory(InventoryLocation.player, item.ItemCode) == -1)
                {
                    ClearSelectedItem();
                }
            }
            
        }*/

        if (itemDetails != null && isSelected)
        {
            if (gridCursor.CursorPositionIsValid)
            {
                //获取鼠标位置
                Vector3 worldPosition = mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x,
                    Input.mousePosition.y, -mainCamera.transform.position.z));
                GameObject itemGameObject = Instantiate(itemPrefab,
                    new Vector3(worldPosition.x, worldPosition.y - Settings.gridCellSize / 2f, worldPosition.z),
                    Quaternion.identity, parentItem);

                //添加脚本item，item会根据物品代码自动初始化图片
                Item item = itemGameObject.GetComponent<Item>();
                item.ItemCode = itemDetails.itemCode;

                //从InventoryManager的背包中删掉此物品
                InventoryManager.Instance.RemoveItem(InventoryLocation.player, item.ItemCode);

                if (InventoryManager.Instance.FindItemInInventory(InventoryLocation.player, item.ItemCode) == -1)
                {
                    ClearSelectedItem();
                }
            }
        }
    }

    
    #endregion
    
    
    
    
    #region 鼠标悬浮显示物品描述相关函数
    
    
    //鼠标悬浮显示物品描述
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (itemQuantity != 0)
        {
            //生成详情文本框
            inventoryBar.inventoryTextBoxGameobject =
                Instantiate(inventoryTextBoxPrefab, transform.position, Quaternion.identity);

            //设置详情文本框的父级
            inventoryBar.inventoryTextBoxGameobject.transform.SetParent(parentCanvas.transform, false);

            //添加给详情文本框脚本
            UIInventoryTextBox inventoryTextBox =
                inventoryBar.inventoryTextBoxGameobject.GetComponent<UIInventoryTextBox>();

            //根据类型获取类型描述文本
            string itemTypeDescription = InventoryManager.Instance.GetItemTypeDescription(itemDetails.itemType);
            
            //设置详情文本框上的文本
            inventoryTextBox.SetTextboxText(itemDetails.itemDescription, itemTypeDescription,"",
                itemDetails.itemLongDescription,"","");

            //根据物品栏在底部还是顶部，生成不同位置的文本框
            if (inventoryBar.IsInventoryBarPositionBottom)
            {
                inventoryBar.inventoryTextBoxGameobject.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0f);
                inventoryBar.inventoryTextBoxGameobject.transform.position = new Vector3(transform.position.x,
                    transform.position.y + 50f, transform.position.z);
            }
            else
            {
                inventoryBar.inventoryTextBoxGameobject.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 1f);
                inventoryBar.inventoryTextBoxGameobject.transform.position = new Vector3(transform.position.x,
                    transform.position.y - 50f, transform.position.z);
            }

        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        DestroyInventoryTextBar();
    }
    
    public void DestroyInventoryTextBar()
    {
        if (inventoryBar.inventoryTextBoxGameobject != null)
        {
            Destroy(inventoryBar.inventoryTextBoxGameobject);
        }
    }
    

    #endregion



    #region 按下鼠标左键设置选中框
    
    //处理鼠标点击事件
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (isSelected == true)
            {
                ClearSelectedItem();
            }
            else
            {
                if (itemQuantity > 0)
                {
                    SetSelectedItem();
                }
            }
        }
    }
    //设置选中框
    private void SetSelectedItem()
    {
        inventoryBar.ClearHighlightOnInventorySlots();

        isSelected = true;

        inventoryBar.SetHighlightedInventorySlots();

        gridCursor.ItemUseGridRadius = itemDetails.itemUseGridRadius;
        
        cursor.ItemUseRadius = itemDetails.itemUseRadius;
        
        if (itemDetails.itemUseGridRadius > 0)
        {
            gridCursor.EnableCursor();
        }
        else
        {
            gridCursor.DisableCursor();
        }

        if (itemDetails.itemUseRadius > 0f)
        {
            cursor.EnableCursor();
        }
        else
        {
            cursor.DisableCursor();
        }

        gridCursor.SelectedItemType = itemDetails.itemType;
        
        cursor.SelectedItemType = itemDetails.itemType;
        
        InventoryManager.Instance.SetSelectedInventoryItem(InventoryLocation.player, itemDetails.itemCode);

        if (itemDetails.canBeCarried)
        {
            Player.Instance.ShowCarriedItem(itemDetails.itemCode);
        }
        else
        {
            Player.Instance.ClearCarriedItem();
        }
    }
    //清除选中框
    private void ClearSelectedItem()
    {
        ClearCursors();
         
        inventoryBar.ClearHighlightOnInventorySlots();

        isSelected = false;
        
        InventoryManager.Instance.ClearSelectedInventoryItem(InventoryLocation.player);
        
        Player.Instance.ClearCarriedItem();
    }

    private void ClearCursors()
    {
        gridCursor.DisableCursor();
        
        cursor.DisableCursor();

        gridCursor.SelectedItemType = ItemType.none;
        
        cursor.SelectedItemType = ItemType.none;
    }
    #endregion
}
