using System.Collections.Generic;

/// <summary>
/// <para>GameObjectSave：用来保存物品数据的类</para>
/// <para>GameObjectSave.sceneData是按照场景名保存场景中全部数据的字典，玩家的信息可保存于持久化场景</para>
/// <para>一个Manager如果存在需保存的数据，则需持有一个自己的GameObjectSave实例</para>
/// <para>虽然看上去一个GameObjectSave实例足以保存全部数据，但实际上任意一个GameObjectSave实例都是不完整的</para>
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
