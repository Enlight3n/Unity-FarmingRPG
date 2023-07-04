using UnityEngine;

public class Item : MonoBehaviour
{
    [ItemCodeDescription] [SerializeField] private int _itemCode;

    private SpriteRenderer _spriteRenderer;

    public int ItemCode
    {
        get { return _itemCode; } 
        set {_itemCode = value; }
    }

    private void Awake()
    {
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void Start()
    {
        if (ItemCode != 0)
        {
            Init(ItemCode);
        }
    }

    public void Init(int itemCodeParam)
    {
        if (itemCodeParam != 0)
        {
            ItemCode = itemCodeParam;
            
            ItemDetails itemDetails = InventoryManager.Instance.GetItemDetails(ItemCode);
            
            
            //我们可以通过修改物品编码来自动更新图片，而不用预先准备好预制体
            _spriteRenderer.sprite = itemDetails.itemSprite;

            //如果是可收割类型的物体，则可以摆动，像草，仙人掌这一类的
            if (itemDetails.itemType == ItemType.Reapable_scenery)
            {
                gameObject.AddComponent<ItemNudge>();
            }
        }
    }
}
