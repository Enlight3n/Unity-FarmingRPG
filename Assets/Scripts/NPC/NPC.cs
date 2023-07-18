using System;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 主要用于npc的保存，包括保存npc此时的位置，以及目的网格，加载后npc继续进行
/// </summary>
[RequireComponent(typeof(NPCMovement))]
[RequireComponent(typeof(GenerateGUID))]
public class NPC : MonoBehaviour, ISaveable
{
    private string _iSaveableUniqueID;
    public string ISaveableUniqueID { get { return _iSaveableUniqueID; } set { _iSaveableUniqueID = value; } }

    private GameObjectSave _gameObjectSave;
    public GameObjectSave GameObjectSave { get { return _gameObjectSave; } set { _gameObjectSave = value; } }

    private NPCMovement npcMovement;

    private void OnEnable()
    {
        ISaveableRegister();
    }

    private void OnDisable()
    {
        ISaveableDeregister();
    }

    private void Awake()
    {
        ISaveableUniqueID = GetComponent<GenerateGUID>().GUID;
        GameObjectSave = new GameObjectSave();
    }

    private void Start()
    {
        // get npc movement component
        npcMovement = GetComponent<NPCMovement>();
    }

    #region 实现接口

    

    
    public void ISaveableRegister()
    {
        SaveLoadManager.Instance.iSaveableObjectList.Add(this);
    }
    public void ISaveableDeregister()
    {
        SaveLoadManager.Instance.iSaveableObjectList.Remove(this);
    }

    //类似于保存玩家位置
    public GameObjectSave ISaveableSave()
    {
        // Remove current scene save
        GameObjectSave.sceneData.Remove(Settings.PersistentScene);

        // Create scene save
        SceneSave sceneSave = new SceneSave();

        // Create vector 3 serialisable dictionary
        sceneSave.vector3Dictionary = new Dictionary<string, Vector3Serializable>();

        // Create string dictionary
        sceneSave.stringDictionary = new Dictionary<string, string>();

        // Store target grid position, target world position, and target scene
        sceneSave.vector3Dictionary.Add("npcTargetGridPosition",
            new Vector3Serializable(npcMovement.npcTargetGridPosition.x, npcMovement.npcTargetGridPosition.y,
                npcMovement.npcTargetGridPosition.z));
        sceneSave.vector3Dictionary.Add("npcTargetWorldPosition",
            new Vector3Serializable(npcMovement.npcTargetWorldPosition.x, npcMovement.npcTargetWorldPosition.y,
                npcMovement.npcTargetWorldPosition.z));
        sceneSave.stringDictionary.Add("npcTargetScene", npcMovement.npcTargetScene.ToString());

        // Add scene save to game object
        GameObjectSave.sceneData.Add(Settings.PersistentScene, sceneSave);

        return GameObjectSave;
    }

    public void ISaveableLoad(GameSave gameSave)
    {
        // Get game object save
        if (gameSave.gameObjectData.TryGetValue(ISaveableUniqueID, out GameObjectSave gameObjectSave))
        {
            GameObjectSave = gameObjectSave;

            // Get scene save
            if (GameObjectSave.sceneData.TryGetValue(Settings.PersistentScene, out SceneSave sceneSave))
            {
                // if dictionaries are not null
                if (sceneSave.vector3Dictionary != null && sceneSave.stringDictionary != null)
                {
                    // target grid position
                    if (sceneSave.vector3Dictionary.TryGetValue("npcTargetGridPosition",
                            out Vector3Serializable savedNPCTargetGridPosition))
                    {
                        npcMovement.npcTargetGridPosition = new Vector3Int((int)savedNPCTargetGridPosition.x,
                            (int)savedNPCTargetGridPosition.y, (int)savedNPCTargetGridPosition.z);
                        npcMovement.npcCurrentGridPosition = npcMovement.npcTargetGridPosition;
                    }
                }

                // target world position
                if (sceneSave.vector3Dictionary.TryGetValue("npcTargetWorldPosition",
                        out Vector3Serializable savedNPCTargetWorldPosition))
                {
                    npcMovement.npcTargetWorldPosition = new Vector3(savedNPCTargetWorldPosition.x,
                        savedNPCTargetWorldPosition.y, savedNPCTargetWorldPosition.z);
                    transform.position = npcMovement.npcTargetWorldPosition;
                }

                // target scene
                if (sceneSave.stringDictionary.TryGetValue("npcTargetScene", out string savedTargetScene))
                {
                    if (Enum.TryParse<SceneName>(savedTargetScene, out SceneName sceneName))
                    {
                        npcMovement.npcTargetScene = sceneName;
                        npcMovement.npcCurrentScene = npcMovement.npcTargetScene;
                    }
                }

                // Clear any current NPC movement
                npcMovement.CancelNPCMovement();

            }

        }
    }
    

    public void ISaveableRestoreScene(string sceneName)
    {
        // Nothing required here since on persistent scene
    }
    
    public void ISaveableStoreScene(string sceneName)
    {
        // Nothing required here since on persistent scene
    }
    
    #endregion
}
