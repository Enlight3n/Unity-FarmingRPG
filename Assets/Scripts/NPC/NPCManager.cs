using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

//[RequireComponent(typeof(AStar))]
public class NPCManager : SingletonMonobehaviour<NPCManager>
{
    [HideInInspector] public NPC[] npcArray;
    
    //获取so_SceneRouteList，将其做成字典
    [SerializeField] private SO_SceneRouteList so_SceneRouteList = null;
    private Dictionary<string, SceneRoute> sceneRouteDictionary;


    private AStar aStar;

    protected override void Awake()
    {
        base.Awake();
        
        sceneRouteDictionary = new Dictionary<string, SceneRoute>();

        if (so_SceneRouteList.sceneRouteList.Count > 0)
        {
            foreach (SceneRoute so_sceneRoute in so_SceneRouteList.sceneRouteList)
            {
                // Check for duplicate routes in dictionary
                if (sceneRouteDictionary.ContainsKey(so_sceneRoute.fromSceneName.ToString() +
                                                     so_sceneRoute.toSceneName.ToString()))
                {
                    Debug.Log(
                        "** Duplicate Scene Route Key Found ** Check for duplicate routes in the scriptable object scene route list");
                    continue;
                }

                // Add route to dictionary
                sceneRouteDictionary.Add(so_sceneRoute.fromSceneName.ToString() + so_sceneRoute.toSceneName.ToString(), so_sceneRoute);
            }
        }

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
    
    //传入起始场景和目的场景，返回路线
    public SceneRoute GetSceneRoute(string fromSceneName, string toSceneName)
    {
        SceneRoute sceneRoute;

        // Get scene route from dictionary
        if (sceneRouteDictionary.TryGetValue(fromSceneName + toSceneName, out sceneRoute))
        {
            return sceneRoute;
        }
        else
        {
            return null;
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