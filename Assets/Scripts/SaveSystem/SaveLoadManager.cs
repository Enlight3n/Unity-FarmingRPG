using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveLoadManager : SingletonMonobehaviour<SaveLoadManager>
{
    //用iSaveableObjectList来保存所有继承了接口的类，在这里，则是保存了SceneItemsManager类
    public List<ISaveable> iSaveableObjectList;

    protected override void Awake()
    {
        base.Awake();

        iSaveableObjectList = new List<ISaveable>();
    }

    public void StoreCurrentSceneData()
    {
        foreach (ISaveable iSaveableObject in iSaveableObjectList)
        {
            iSaveableObject.ISaveableStoreScene(SceneManager.GetActiveScene().name);
            
        }
    }
    
    public void ReStoreCurrentSceneData()
    {
        foreach (ISaveable iSaveableObject in iSaveableObjectList)
        {
            iSaveableObject.ISaveableRestoreScene(SceneManager.GetActiveScene().name);
        }
    }
}
