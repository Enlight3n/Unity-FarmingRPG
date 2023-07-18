using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 使用这个数据容器来记录绘制的瓦片地图
/// </summary>
[CreateAssetMenu(fileName = "so_GridProperties",menuName = "Scriptable Objects/Grid Properties")]
public class SO_GridProperties : ScriptableObject
{
   public SceneName sceneName;
   
   //设定记录的起点（左下）坐标，以及组成范围矩形的高宽，只在寻路的时候用到
   public int gridHeight;
   public int gridWidth;
   public int originX;
   public int originY;

   //用GridProperty组成的列表记录整个场景中的特殊贴图
   [SerializeField] public List<GridProperty> gridPropertyList;
}
