using System.Collections.Generic;
using UnityEngine;

public class AStar : MonoBehaviour
{
    [Header("Tiles & Tilemap References")] [Header("Options")] [SerializeField]
    private bool observeMovementPenalties = true; //指明是否启用路径权重

    [Range(0, 20)] [SerializeField] private int pathMovementPenalty; //path的权重

    [Range(0, 20)] [SerializeField] private int defaultMovementPenalty; //默认的权重

    private HashSet<Node> closedNodeList; //使用hashset是为了更快的确定某个节点是不是在其中，同时我们也不关注节点的排序
    private List<Node> openNodeList; //openNodeList存放所有计算了代价的节点，更新时从中选取节点放到closedNodeList
    
    private Node startNode; //起始节点
    private Node targetNode; //目标节点
    
    private GridNodes gridNodes; //用来保存场景中全部节点的属性
    
    //用来指明场景的大小
    private int gridHeight;
    private int gridWidth;
    private int originX;
    private int originY;

    private bool pathFound;
    
    /// <summary>
    /// <para>唯一外部调用的函数</para>
    /// 给定的场景名，创建一个从startGridPosition到endGridPosition的路径，并把移动步骤添加到npcMovementStack
    /// 同时，如果找到了则返回true，否则返回false
    /// </summary>
    public bool BuildPath(SceneName sceneName, Vector2Int startGridPosition, Vector2Int endGridPosition,
        Stack<NPCMovementStep> npcMovementStepStack)
    {
        if (PopulateGridNodesFromGridPropertiesDictionary(sceneName, startGridPosition, endGridPosition))
            if (FindShortestPath())
            {
                //从ClosedList中，沿着目标节点，挨个抽出父节点
                UpdatePathOnNPCMovementStepStack(sceneName, npcMovementStepStack);

                return true;
            }

        return false;
    }
    
    
    /// <summary>
    /// 从ClosedList中，沿着目标节点，挨个抽出父节点，并将位置赋值给NPC
    /// </summary>
    /// <param name="sceneName"></param>
    /// <param name="npcMovementStepStack"></param>
    private void UpdatePathOnNPCMovementStepStack(SceneName sceneName, Stack<NPCMovementStep> npcMovementStepStack)
    {
        Node nextNode = targetNode;

        while (nextNode != null)
        {
            NPCMovementStep npcMovementStep = new NPCMovementStep();

            npcMovementStep.sceneName = sceneName;
            npcMovementStep.gridCoordinate =
                new Vector2Int(nextNode.gridPosition.x + originX, nextNode.gridPosition.y + originY);

            npcMovementStepStack.Push(npcMovementStep);

            nextNode = nextNode.parentNode;
        }
    }

    /// <summary>
    ///  A星寻路算法
    /// </summary>
    private bool FindShortestPath()
    {
        // Add start node to open list
        openNodeList.Add(startNode);

        // Loop through open node list until empty
        while (openNodeList.Count > 0)
        {
            // Sort List
            openNodeList.Sort();

            //  current node = the node in the open list with the lowest fCost
            Node currentNode = openNodeList[0];
            openNodeList.RemoveAt(0);

            // add current node to the closed list
            closedNodeList.Add(currentNode);

            // if the current node = target node
            //      then finish

            if (currentNode == targetNode)
            {
                pathFound = true;
                break;
            }

            // 评估当前节点的每个邻节点的F代价
            EvaluateCurrentNodeNeighbours(currentNode);
        }
        return pathFound;
    }

    private void EvaluateCurrentNodeNeighbours(Node currentNode)
    {
        Vector2Int currentNodeGridPosition = currentNode.gridPosition;

        Node validNeighbourNode;

        // Loop through all directions
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (i == 0 && j == 0)
                    continue;

                validNeighbourNode = GetValidNodeNeighbour(currentNodeGridPosition.x + i, 
                    currentNodeGridPosition.y + j);

                if (validNeighbourNode != null)
                {
                    // Calculate new gcost for neighbour
                    int newCostToNeighbour;

                    if (observeMovementPenalties)
                        newCostToNeighbour = currentNode.gCost + GetDistance(currentNode, validNeighbourNode) +
                                             validNeighbourNode.movementPenalty;
                    else
                        newCostToNeighbour = currentNode.gCost + GetDistance(currentNode, validNeighbourNode);

                    var isValidNeighbourNodeInOpenList = openNodeList.Contains(validNeighbourNode);

                    if (newCostToNeighbour < validNeighbourNode.gCost || !isValidNeighbourNodeInOpenList)
                    {
                        validNeighbourNode.gCost = newCostToNeighbour;
                        validNeighbourNode.hCost = GetDistance(validNeighbourNode, targetNode);

                        validNeighbourNode.parentNode = currentNode;

                        if (!isValidNeighbourNodeInOpenList)
                        {
                            openNodeList.Add(validNeighbourNode);
                        }
                    }
                }
            }
        }
    }
    
    
    /// <summary>
    /// 根据场景名称和网格属性字典填充网格节点
    /// </summary>
    /// <param name="sceneName">场景名称</param> 
    /// <param name="startGridPosition">开始的网格位置，是整数</param> 
    /// <param name="endGridPosition">结束的网格位置，是整数</param> 
    /// <returns></returns>
    private bool PopulateGridNodesFromGridPropertiesDictionary(SceneName sceneName, Vector2Int startGridPosition,
        Vector2Int endGridPosition)
    {
        // 获取场景的grid properties dictionary
        SceneSave sceneSave;

        if (GridPropertiesManager.Instance.GameObjectSave.sceneData.TryGetValue(sceneName.ToString(), out sceneSave))
        {
            // Get Dict grid property details
            if (sceneSave.gridPropertyDetailsDictionary != null)
            {
                // 获取网格的范围，一个场景中只有这一个范围
                if (GridPropertiesManager.Instance.GetGridDimensions(sceneName, out var gridDimensions,
                        out var gridOrigin))
                {
                    // 创建网格范围大小的网格节点，这个大小就是它的上限
                    gridNodes = new GridNodes(gridDimensions.x, gridDimensions.y);
                    gridWidth = gridDimensions.x;
                    gridHeight = gridDimensions.y;
                    originX = gridOrigin.x;
                    originY = gridOrigin.y;

                    //创建openNodeList
                    openNodeList = new List<Node>();

                    //创建closedNodeList
                    closedNodeList = new HashSet<Node>();
                }
                else
                {
                    return false;
                }

                // 初始化开始节点
                startNode = gridNodes.GetGridNode(startGridPosition.x - gridOrigin.x,
                    startGridPosition.y - gridOrigin.y);

                // 初始化目标节点
                targetNode = gridNodes.GetGridNode(endGridPosition.x - gridOrigin.x, 
                    endGridPosition.y - gridOrigin.y);

                // populate obstacle and path info for grid
                for (int x = 0; x < gridDimensions.x; x++)
                {
                    for (int y = 0; y < gridDimensions.y; y++)
                    {
                        GridPropertyDetails gridPropertyDetails =
                            GridPropertiesManager.Instance.GetGridPropertyDetails(x + gridOrigin.x, y + gridOrigin.y,
                                sceneSave.gridPropertyDetailsDictionary);

                        //根据gridPropertyDetails，把信息转录进gridNodes
                        if (gridPropertyDetails != null)
                        {
                            // If NPC obstacle
                            if (gridPropertyDetails.isNPCObstacle == true)
                            {
                                Node node = gridNodes.GetGridNode(x, y);
                                node.isObstacle = true;
                            }
                            else if (gridPropertyDetails.isPath == true)
                            {
                                Node node = gridNodes.GetGridNode(x, y);
                                node.movementPenalty = pathMovementPenalty;
                            }
                            else
                            {
                                Node node = gridNodes.GetGridNode(x, y);
                                node.movementPenalty = defaultMovementPenalty;
                            }
                        }
                    }
                }
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }

        return true;
    }

    #region EvaluateCurrentNodeNeighbours调用到的方法

    private int GetDistance(Node nodeA, Node nodeB)
    {
        var dstX = Mathf.Abs(nodeA.gridPosition.x - nodeB.gridPosition.x);
        var dstY = Mathf.Abs(nodeA.gridPosition.y - nodeB.gridPosition.y);

        if (dstX > dstY)
            return 14 * dstY + 10 * (dstX - dstY);
        return 14 * dstX + 10 * (dstY - dstX);
    }

    private Node GetValidNodeNeighbour(int neighboutNodeXPosition, int neighbourNodeYPosition)
    {
        // If neighbour node position is beyond grid then return null
        if (neighboutNodeXPosition >= gridWidth || neighboutNodeXPosition < 0 || neighbourNodeYPosition >= gridHeight ||
            neighbourNodeYPosition < 0) return null;

        // if neighbour is an obstacle or neighbour is in the closed list then skip
        var neighbourNode = gridNodes.GetGridNode(neighboutNodeXPosition, neighbourNodeYPosition);

        if (neighbourNode.isObstacle || closedNodeList.Contains(neighbourNode))
        {
            return null;
        }
        else
        {
            return neighbourNode;
        }
    }

    #endregion
}