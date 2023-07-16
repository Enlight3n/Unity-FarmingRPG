using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISaveable
{
    string ISaveableUniqueID { get; set; }

    GameObjectSave GameObjectSave{get; set; }

    #region 注册到SaveLoadManager中
    
    void ISaveableRegister();

    void ISaveableDeregister();

    #endregion
    
    
    #region 存储和恢复

    void ISaveableStoreScene(string sceneName);

    void ISaveableRestoreScene(string sceneName);
    
    #endregion
    
    
    
    
    #region 保存和加载
    
    GameObjectSave ISaveableSave();

    void ISaveableLoad(GameSave gameSave);
    
    #endregion
}
