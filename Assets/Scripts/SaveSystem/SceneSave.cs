using System.Collections.Generic;
using Mono.Cecil.Cil;

/// <summary>
/// SceneSave，场景保存类，用来保存单个场景的全部数据。
/// <para>其中含有的多个列表/字典，用来保存一个场景中不同类型的数据，但在使用实例时，某些字段在某个场景中可能不会用到</para>
/// </summary>
[System.Serializable]
public class SceneSave
{
    
    public Dictionary<string, bool> boolDictionary;

    //保存"currentScene"，"playerDirection"两个键
    public Dictionary<string, string> stringDictionary; //保存场景和对应的玩家方向
    
    //自定义的类，因为Vector3不能序列化，string是对应的类型，如果是玩家位置，string为playerPosition
    public Dictionary<string, Vector3Serializable> vector3Dictionary; 

    public List<SceneItem> listSceneItem;

    public Dictionary<string, GridPropertyDetails> gridPropertyDetailsDictionary;

    //保存库存
    public List<InventoryItem>[] listInvItemArray;
    public Dictionary<string, int[]> intArrayDictionary;
    
    //保存时间：如 "gameYear", gameYear ; "gameDay", gameDay
    public Dictionary<string, int> intDictionary;

}
