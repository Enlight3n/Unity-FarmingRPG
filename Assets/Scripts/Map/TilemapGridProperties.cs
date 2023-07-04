using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// 这个脚本仅附在不同类型的特殊瓦片贴图上，其他类也不会使用它
/// 当我们绘制完该层次的瓦片贴图以后，这个脚本负责将数据写入对应的数据容器中
/// </summary>


[ExecuteAlways] //使脚本的实例始终执行，无论是在 Play Mode 还是在编辑时
public class TilemapGridProperties : MonoBehaviour
{
    private Tilemap tilemap;
    private Grid grid;

    //需要一个数据容器，用来把贴图数据写入对应的容器
    [SerializeField] private SO_GridProperties gridProperties; 
    
    //指明这个脚本附在哪一类组件上，写入数据容器时直接使用这个字段即可，默认为diggable
    [SerializeField] private GridBoolProperty gridBoolProperty = GridBoolProperty.diggable; 

    
    
    //（仅在对象处于活动状态时调用）：此函数在启用对象后立即调用
    private void OnEnable()
    {
        //编辑器模式下，Application.IsPlaying(gameObject)始终返回false，加入这个可能是为了防止在播放模式的误操作
        if (!Application.IsPlaying(gameObject))
        {
            tilemap = GetComponent<Tilemap>();
        }

        //重新绘制时，清除原来的数据
        if (gridProperties != null)
        {
            gridProperties.gridPropertyList.Clear();
        }
    }

    private void OnDisable()
    {
        if (!Application.IsPlaying(gameObject))
        {
            UpdateGridProperties();
            
            if (gridProperties != null)
            {
                EditorUtility.SetDirty(gridProperties);
            }
        }
    }

    //更新
    private void UpdateGridProperties()
    {
        //
        tilemap.CompressBounds();

        if (!Application.IsPlaying(gameObject))
        {
            if (gridProperties != null)
            {
                Vector3Int startCell = tilemap.cellBounds.min;
                Vector3Int endCell = tilemap.cellBounds.max;

                for (int x = startCell.x; x < endCell.x; x++)
                {
                    for (int y = startCell.y; y < endCell.y; y++)
                    {
                        TileBase tile = tilemap.GetTile(new Vector3Int(x, y, 0));

                        if (tile != null)
                        {
                            gridProperties.gridPropertyList.Add(new GridProperty(new GridCoordinate(x, y),
                                gridBoolProperty, true));
                        }
                    }
                }
            }
        }
    }

    
    //编辑模式下的update仅在场景更改时调用一次
    private void Update()
    {
        if (!Application.IsPlaying(gameObject))
        {
            Debug.Log("Disable property tilemaps");
        }
    }
}

    
