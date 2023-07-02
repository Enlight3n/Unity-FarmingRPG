using System;
using System.Collections.Generic;
using UnityEngine;

public class UIInventoryBar : MonoBehaviour
{
    //用来表明物品栏是空白的图片，清空物品栏时使用
    [SerializeField] private Sprite blank16x16sprite = null;
    
    //声明一个UIInventorySlot数组，用来保存每个UIInventorySlot脚本
    [SerializeField] private UIInventorySlot[] inventorySlot = null;
    
    //预先制作好一个用来拖拽的预制体，拖拽赋值给InventoryBarDraggedItem，鼠标拖拽物品到场景中时，用InventoryBarDraggedItem跟随
    public GameObject InventoryBarDraggedItem;
    
    
    [HideInInspector] public GameObject inventoryTextBoxGameobject;
    
    
    private RectTransform rectTransform;

    
    private bool _isInventoryBarPositionBottom = true;
    public bool IsInventoryBarPositionBottom
    {
        get => _isInventoryBarPositionBottom;
        set => _isInventoryBarPositionBottom = value;
    }

    
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }
    
    
    
    private void OnEnable()
    {
        EventHandler.InventoryUpdatedEvent += InventoryUpdated;
    }
    
    private void OnDisable()
    {
        EventHandler.InventoryUpdatedEvent -= InventoryUpdated;
    }

    //更新物品栏，每次更新物品栏，都会先将物品栏清空再根据inventoryList重新绘制
    private void InventoryUpdated(InventoryLocation inventoryLocation, List<InventoryItem> inventoryList)
    {
        if (inventoryLocation == InventoryLocation.player)
        {
            ClearInventorySlots();

            if (inventorySlot.Length > 0 && inventoryList.Count > 0)
            {
                for (int i = 0; i < inventorySlot.Length; i++)
                {
                    if (i < inventoryList.Count)
                    {
                        int itemCode = inventoryList[i].itemCode;

                        ItemDetails itemDetails = InventoryManager.Instance.GetItemDetails(itemCode);
                        
                        if (itemDetails != null)
                        {
                            inventorySlot[i].inventorySlotImage.sprite = itemDetails.itemSprite;
                            inventorySlot[i].textMeshProUGUI.text = inventoryList[i].itemQuantity.ToString();
                            inventorySlot[i].itemDetails = itemDetails;
                            inventorySlot[i].itemQuantity = inventoryList[i].itemQuantity;
                            SetHighlightedInventorySlots(i);
                        }
                    }

                    else
                    {
                        break;
                    }
                }
            }
        }
    }
    
    //用来初始化物品栏，将物品栏清空，每次更新物品栏都会清空再重新绘制
    private void ClearInventorySlots()
    {
        if (inventorySlot.Length > 0)
        {
            for (int i = 0; i < inventorySlot.Length; i++)
            {
                inventorySlot[i].inventorySlotImage.sprite = blank16x16sprite;
                inventorySlot[i].textMeshProUGUI.text = "";
                inventorySlot[i].itemDetails = null;
                inventorySlot[i].itemQuantity = 0;
                
                SetHighlightedInventorySlots(i);
            }
        }
    }

    
    
    
    private void Update()
    {
        SwitchInventoryPosition();
    }
    
    
    private void SwitchInventoryPosition()
    {
        Vector3 playerViewportPosition = Player.Instance.GetPlayerViewportPosition();

        if (playerViewportPosition.y > 0.3f && IsInventoryBarPositionBottom == false)
        {
            rectTransform.pivot = new Vector2(0.5f, 0f);
            rectTransform.anchorMin = new Vector2(0.5f, 0f);
            rectTransform.anchorMax = new Vector2(0.5f, 0f);
            rectTransform.anchoredPosition = new Vector2(0f, 2.5f);

            IsInventoryBarPositionBottom = true;
        }
        else if(playerViewportPosition.y <= 0.3f && IsInventoryBarPositionBottom == true)
        {
            rectTransform.pivot = new Vector2(0.5f, 1f);
            rectTransform.anchorMin = new Vector2(0.5f, 1f);
            rectTransform.anchorMax = new Vector2(0.5f, 1f);
            rectTransform.anchoredPosition = new Vector2(0f, -2.5f);
            
            IsInventoryBarPositionBottom = false;
        }
    }

    #region 设置选中红框
    
    
    //清除背包中的选中红框
    public void ClearHighlightOnInventorySlots()
    {
        if (inventorySlot.Length > 0)
        {
            for (int i = 0; i < inventorySlot.Length; i++)
            {
                if (inventorySlot[i].isSelected)
                {
                    inventorySlot[i].isSelected = false;
                    inventorySlot[i].inventorySlotHighlight.color = new Color(0f, 0f, 0f, 0f);
                    InventoryManager.Instance.ClearSelectedInventoryItem(InventoryLocation.player);
                }
            }
        }
    }

    //遍历slot是否被选择，执行SetHighlightedInventorySlots(i);
    public void SetHighlightedInventorySlots()
    {
        if (inventorySlot.Length > 0)
        {
            for (int i = 0; i < inventorySlot.Length; i++)
            {
                SetHighlightedInventorySlots(i);
            }
        }
    }
    
    //根据给定位置的slot是否被选中，添加红框
    public void SetHighlightedInventorySlots(int itemPosition)
    {
        if (inventorySlot.Length > 0 && inventorySlot[itemPosition].itemDetails != null)
        {
            if (inventorySlot[itemPosition].isSelected)
            {
                inventorySlot[itemPosition].inventorySlotHighlight.color = new Color(1f, 1f, 1f, 1f);

                InventoryManager.Instance.SetSelectedInventoryItem(InventoryLocation.player,
                    inventorySlot[itemPosition].itemDetails.itemCode);
            }
        }
    }
    #endregion
}
