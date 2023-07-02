using UnityEngine;

[System.Serializable]
public class GridCoordinate
{
    public int x;
    public int y;
    public GridCoordinate(int p1, int p2)
    {
        x = p1;
        y = p2;
    }
    
    
    //如果你有一个 GridCoordinate 对象 gridCoordinate，并且想将它转换为 Vector2 类型，你可以这样写：
    // Vector2 vector2 = (Vector2)gridCoordinate;
    
    public static explicit operator Vector2(GridCoordinate gridCoordinate)
    {
        return new Vector2((float)gridCoordinate.x, (float)gridCoordinate.y);
    }

    public static explicit operator Vector2Int(GridCoordinate gridCoordinate)
    {
        return new Vector2Int(gridCoordinate.x, gridCoordinate.y);
    }
    
    public static explicit operator Vector3(GridCoordinate gridCoordinate)
    {
        return new Vector3((float)gridCoordinate.x, (float)gridCoordinate.y, 0f);
    }

    public static explicit operator Vector3Int(GridCoordinate gridCoordinate)
    {
        return new Vector3Int(gridCoordinate.x, gridCoordinate.y, 0);
    }
}
