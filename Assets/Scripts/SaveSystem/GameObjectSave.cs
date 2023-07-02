using System.Collections.Generic;

/// <summary>
/// GameObjectSave：用来保存物品数据的类。GameObjectSave.sceneData是按照场景名保存场景数据的字典
/// </summary>
[System.Serializable]
public class GameObjectSave
{
   //string是场景名，SceneSave存储的是这个场景中的所有物品信息
   public Dictionary<string, SceneSave> sceneData;

   //为了避免声明类时，再逐一给类中的字段分配空间，我们将内存分配的代码写在构造函数里
   public GameObjectSave()
   {
      sceneData = new Dictionary<string, SceneSave>();
   }

   
   public GameObjectSave(Dictionary<string, SceneSave> sceneData)
   {
      this.sceneData = sceneData;
   }
}
