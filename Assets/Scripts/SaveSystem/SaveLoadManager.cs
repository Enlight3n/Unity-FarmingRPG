using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveLoadManager : SingletonMonobehaviour<SaveLoadManager>
{
    //用iSaveableObjectList来保存所有继承了接口的类，在这里，则是保存了SceneItemsManager类
    public List<ISaveable> iSaveableObjectList;
    public GameSave gameSave;

    protected override void Awake()
    {
        base.Awake();

        iSaveableObjectList = new List<ISaveable>();
    }

    #region 存储和恢复场景数据

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

    #endregion




    #region 从文件中加载和保存

    public void LoadDataFromFile()
    {
        BinaryFormatter bf = new BinaryFormatter();

        if (File.Exists(Application.persistentDataPath + "/WildHopeCreek.dat"))
        {
            gameSave = new GameSave();

            FileStream file = File.Open(Application.persistentDataPath + "/WildHopeCreek.dat", FileMode.Open);

            gameSave = (GameSave)bf.Deserialize(file);

            // 遍历iSaveableObjectList的长度次，根据每一个UniqueID，获取对应的gameSave，执行对应的加载接口函数
            for (int i = iSaveableObjectList.Count - 1; i > -1; i--)
            {
                if (gameSave.gameObjectData.ContainsKey(iSaveableObjectList[i].ISaveableUniqueID))
                {
                    iSaveableObjectList[i].ISaveableLoad(gameSave);
                }
                // else if iSaveableObject unique ID is not in the game object data then destroy object
                else
                {
                    Component component = (Component)iSaveableObjectList[i];
                    Destroy(component.gameObject);
                }
            }
            file.Close();
        }
    }

    public void SaveDataToFile()
    {
        gameSave = new GameSave();

        // 遍历iSaveableObjectList保存的每一个GameObjectSave，将其制作为键值对，添加到gameSave字典中
        foreach (ISaveable iSaveableObject in iSaveableObjectList)
        {
            gameSave.gameObjectData.Add(iSaveableObject.ISaveableUniqueID, iSaveableObject.ISaveableSave());
        }

        BinaryFormatter bf = new BinaryFormatter();

        FileStream file = File.Open(Application.persistentDataPath + "/WildHopeCreek.dat", FileMode.Create);

        bf.Serialize(file, gameSave);

        file.Close();
        
    }
    #endregion
}