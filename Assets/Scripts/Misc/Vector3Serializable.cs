/// <summary>
/// 这个类用来保存物品的坐标
/// </summary>
[System.Serializable]
public class Vector3Serializable
{
    public float x, y, z;

    public Vector3Serializable(float x, float y, float z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public Vector3Serializable()
    {
        
    }
}
