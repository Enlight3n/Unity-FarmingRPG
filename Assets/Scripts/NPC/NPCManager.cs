using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

//[RequireComponent(typeof(AStar))]
public class NPCManager : SingletonMonobehaviour<NPCManager>
{
    [HideInInspector]
    public NPC[] npcArray;

    private AStar aStar;

    protected override void Awake()
    {
        base.Awake();

        aStar = GetComponent<AStar>();

        // Get NPC gameobjects in scene
        npcArray = FindObjectsOfType<NPC>();
    }

    private void OnEnable()
    {
        EventHandler.AfterSceneLoadEvent += AfterSceneLoad;
    }

    private void OnDisable()
    {
        EventHandler.AfterSceneLoadEvent -= AfterSceneLoad;
    }

    private void AfterSceneLoad()
    {

        SetNPCsActiveStatus();
    }

    //场景加载后，遍历每一个npc数组中的npc，获取其NPCMovement脚本，查看npc是否应该在当前场景，如果是，则显示npc，否则不显示
    private void SetNPCsActiveStatus()
    {
        foreach (NPC npc in npcArray)
        {
            NPCMovement npcMovement = npc.GetComponent<NPCMovement>();

            if (npcMovement.npcCurrentScene.ToString() == SceneManager.GetActiveScene().name)
            {
                npcMovement.SetNPCActiveInScene();
            }
            else
            {
                npcMovement.SetNPCInactiveInScene();
            }
        }
    }

    //调用NPCManager的BuildPath实质是在调用寻路算法的BuildPath
    public bool BuildPath(SceneName sceneName, Vector2Int startGridPosition, Vector2Int endGridPosition,
        Stack<NPCMovementStep> npcMovementStepStack)
    {
        if (aStar.BuildPath(sceneName, startGridPosition, endGridPosition, npcMovementStepStack))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}