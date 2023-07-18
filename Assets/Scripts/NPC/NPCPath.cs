using System;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 只需调用BuildPath，创建事件
/// </summary>
[RequireComponent(typeof(NPCMovement))]
public class NPCPath : MonoBehaviour
{
    public Stack<NPCMovementStep> npcMovementStepStack;

    private NPCMovement npcMovement;

    private void Awake()
    {
        npcMovement = GetComponent<NPCMovement>();
        npcMovementStepStack = new Stack<NPCMovementStep>();
    }

    public void ClearPath()
    {
        npcMovementStepStack.Clear();
    }

    /// <summary>
    /// 根据传入的NPCScheduleEvent，创造一个从npc当前网格坐标到npc目标网格坐标的路径，存到npcMovementStepStack中，并更新其中的时间
    /// 最后将npcScheduleEvent的一部分属性赋值到npcMovement的相应变量里面
    /// </summary>
    /// <param name="npcScheduleEvent"></param>
    public void BuildPath(NPCScheduleEvent npcScheduleEvent)
    {
        ClearPath();

        // If schedule event is for the same scene as the current NPC scene
        if (npcScheduleEvent.toSceneName == npcMovement.npcCurrentScene)
        {
            //获取npc当前的网格坐标
            Vector2Int npcCurrentGridPosition = (Vector2Int)npcMovement.npcCurrentGridPosition;

            //获取npc目标坐标的的网格坐标
            Vector2Int npcTargetGridPosition = (Vector2Int)npcScheduleEvent.toGridCoordinate;

            //调用BuildPath，创造一个从npc当前网格坐标到npc目标网格坐标的路径，将其添加到npcMovementStepStack
            //注意，此时npcMovementStepStack中每个元素的时间都未设置
            NPCManager.Instance.BuildPath(npcScheduleEvent.toSceneName, npcCurrentGridPosition, 
                npcTargetGridPosition, npcMovementStepStack);

            //如果栈中元素大于1
            if (npcMovementStepStack.Count > 1)
            {
                //更新npcMovementStepStack的时间，栈顶的时间是当前游戏时间，其他元素的时间是从上一格到这一格所需的时间
                UpdateTimesOnPath();
                
                // 取出开始坐标
                npcMovementStepStack.Pop(); 

                //将npcScheduleEvent的一部分属性赋值到npcMovement的相应变量里面
                npcMovement.SetScheduleEventDetails(npcScheduleEvent);
            }
        }
    }

    /// <summary>
    /// 更新npcMovementStepStack的时间，栈顶的时间是当前游戏时间，其他元素的时间是从上一格到这一格所需的时间
    /// Update the path movement steps with expected gametime
    /// </summary>
    public void UpdateTimesOnPath()
    {
        //获取当前的游戏内时间
        TimeSpan currentGameTime = TimeManager.Instance.GetGameTime();

        NPCMovementStep previousNPCMovementStep = null;

        //遍历npcMovementStepStack，补充其他信息
        foreach (NPCMovementStep npcMovementStep in npcMovementStepStack)
        {
            if (previousNPCMovementStep == null)
            {
                previousNPCMovementStep = npcMovementStep;
            }

            npcMovementStep.hour = currentGameTime.Hours;
            npcMovementStep.minute = currentGameTime.Minutes;
            npcMovementStep.second = currentGameTime.Seconds;

            TimeSpan movementTimeStep;

            // 根据人物移动是否为对角线，决定移动一格的时间
            if (MovementIsDiagonal(npcMovementStep, previousNPCMovementStep))
            {
                movementTimeStep = new TimeSpan(0, 0, 
                    (int)(Settings.gridCellDiagonalSize / Settings.secondsPerGameSecond / npcMovement.npcNormalSpeed));
            }
            else
            {
                movementTimeStep = new TimeSpan(0, 0, 
                    (int)(Settings.gridCellSize / Settings.secondsPerGameSecond / npcMovement.npcNormalSpeed));
            }

            //返回一个新的 TimeSpan 对象，其值为指定的 TimeSpan 对象与此实例的值之和。
            currentGameTime = currentGameTime.Add(movementTimeStep);

            previousNPCMovementStep = npcMovementStep;
        }

    }

    /// <summary>
    /// 判断此次移动是否是对角线移动，因为对角线移动的消耗是1.4倍。如果此前的移动是对角线，返回true，否则false
    /// returns true if the previous movement step is diagonal to movement step, else returns false
    /// </summary>
    private bool MovementIsDiagonal(NPCMovementStep npcMovementStep, NPCMovementStep previousNPCMovementStep)
    {
        if ((npcMovementStep.gridCoordinate.x != previousNPCMovementStep.gridCoordinate.x) &&
            (npcMovementStep.gridCoordinate.y != previousNPCMovementStep.gridCoordinate.y))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}