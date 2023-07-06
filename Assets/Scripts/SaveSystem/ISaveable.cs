using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISaveable
{
    string ISaveableUniqueID { get; set; }

    GameObjectSave GameObjectSave{get; set; }

    void ISaveableRegister();

    void ISaveableDeregister();

    
    
    
    #region 存储和恢复

    void ISaveableStoreScene(string sceneName);

    void ISaveableRestoreScene(string sceneName);
    
    #endregion
    
    
    
    
    #region 保存和加载
    
    GameObjectSave ISaveableSave();

    void ISaveableLoad(GameSave gameSave);
    
    #endregion
}
