using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(ItemCodeDescriptionAttribute))]
public class ItemCodeDescriptionDrawer : PropertyDrawer
{
    //这个类重写了GetPropertyHeight方法来返回两倍于默认高度的值，以便为额外的物品描述标签留出空间。
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        
        //change the returned property height to be double to cater for the additional
        //item code description that we will draw
        return EditorGUI.GetPropertyHeight(property) * 2;
    }

    //它还重写了OnGUI方法来绘制整数字段和物品描述标签。
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {

        //Using BeginProperty / EndProperty on the parent property means that prefab override
        //logic works on the entire property

        EditorGUI.BeginProperty(position, label, property);

        if (property.propertyType == SerializedPropertyType.Integer)
        {
            EditorGUI.BeginChangeCheck(); //start of check for changed values
            
            //Draw item code
            var newValue = EditorGUI.IntField(new Rect(position.x, position.y, 
                position.width, position.height / 2), label, property.intValue);
            
            
            //Draw item description
            EditorGUI.LabelField(new Rect(position.x, position.y + position.height / 2,
                    position.width, position.height / 2), "Item Description",
                GetItemDescription(property.intValue));
            
            
            //if item code value has changed, then set value to new value
            if (EditorGUI.EndChangeCheck())
            {
                property.intValue = newValue;
            }
            
        }
        
        EditorGUI.EndProperty();
    }
    
    //最后定义了一个名为GetItemDescription的私有方法，该方法根据给定的物品代码从一个名为so_ItemList的ScriptableObject资源中获取物品描述。
    private string GetItemDescription(int itemCode)
    {
        SO_ItemList so_itemList;

        so_itemList = AssetDatabase.LoadAssetAtPath("Assets/Scriptable Object Assets/Item/so_ItemList.asset",
            typeof(SO_ItemList)) as SO_ItemList;

        List<ItemDetails> itemDetailsList = so_itemList.itemDetails;

        ItemDetails itemDetail = itemDetailsList.Find(x => x.itemCode == itemCode);

        if (itemDetail != null)
        {
            return itemDetail.itemDescription;
        }
        else
        {
            return "";
        }
    }
}


