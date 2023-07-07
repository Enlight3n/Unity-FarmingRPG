using System.Collections.Generic;
/// <summary>
/// <para>GameSave是保存 GUID-GameObjectSave实例 的字典</para>
/// <para>GameSave实例在场景中只存在一个，声明于SaveLoadManager</para>
/// <para>其他脚本中，只有ISaveable接口会涉及GameSave</para>
/// </summary>
[System.Serializable]
public class GameSave
{
    // string key = GUID gameobject ID
    public Dictionary<string, GameObjectSave> gameObjectData;

    public GameSave()
    {
        gameObjectData = new Dictionary<string, GameObjectSave>();
    }
}
