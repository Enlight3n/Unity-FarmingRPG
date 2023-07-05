using System.Collections.Generic;
/// <summary>
/// SceneSave，场景保存类，用来保存单个场景的全部数据。其中含有的多个列表，用来保存同一场景中不同类型的数据
/// </summary>
[System.Serializable]
public class SceneSave
{
    public Dictionary<string, bool> boolDictionary;

    public List<SceneItem> listSceneItem;

    public Dictionary<string, GridPropertyDetails> gridPropertyDetailsDictionary;

}
