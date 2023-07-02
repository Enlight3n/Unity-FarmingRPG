using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "so_ItemList",menuName = "Scriptable Objects/Item/Item List")]
//那个[CreateAssetMenu]是一个属性，它可以让你的ScriptableObject类
//在编辑器中的Assets/Create菜单下显示出来，
//这样你就可以方便地创建和保存ScriptableObject的实例12。你可以在属性中指定文件名、菜单名和顺序13。
//比如你这个例子中，就是在Assets/Create/Scriptable Objects/Item/Item List下
//创建一个名为so_ItemList的.asset文件。


//ScriptableObject类是一种数据容器，你可以用它来保存大量的数据，而不依赖于类的实例。
//ScriptableObject的一个主要用途是通过避免值的复制来减少你的项目的内存占用。
//这在你的项目中有一个附带了不变数据的MonoBehaviour脚本的预制体时很有用。
//每次你实例化那个预制体，它都会得到那些数据的自己的副本。
//而如果你使用ScriptableObject来存储那些数据，然后从所有的预制体中通过引用来访问它们，
//就意味着内存中只有一份数据
public class SO_ItemList : ScriptableObject
{
    [SerializeField] public List<ItemDetails> itemDetails;
}
