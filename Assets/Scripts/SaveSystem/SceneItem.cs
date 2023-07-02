/// <summary>
/// 这个类用来保存场景中的物品，一个实例对应一个物品
/// </summary>
[System.Serializable]
public class SceneItem
{
    public int itemCode;
    public Vector3Serializable position;
    public string itemName;

    public SceneItem()
    {
        position = new Vector3Serializable();
    }
}
