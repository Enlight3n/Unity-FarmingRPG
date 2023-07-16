[System.Serializable]
public class GridProperty
{
    public GridCoordinate gridCoordinate; //坐标，这个坐标对应的是瓦片贴图的坐标
    public GridBoolProperty gridBoolProperty; //枚举性质，指明网格的用途
    public bool gridBoolValue;

    public GridProperty(GridCoordinate gridCoordinate, GridBoolProperty gridBoolProperty, bool gridBoolValue)
    {
        this.gridCoordinate = gridCoordinate;
        this.gridBoolProperty = gridBoolProperty;
        this.gridBoolValue = gridBoolValue;
    }
}
