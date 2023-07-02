using UnityEngine;


public abstract class SingletonMonobehaviour<T> : MonoBehaviour where T:MonoBehaviour  //泛型类,这是一个通用的单例模式
{
   //属性的写法
   private static T instance;
   public static T Instance //将instance定义为静态变量可以保证在整个程序中只有一个。
   {
      get
      {
         return instance;
      }
   }

   protected virtual void Awake() //抽象方法，必须重写，因为这个涉及Awake（）这个保证了没有多余的类
   {
      if (instance == null)
      {
         instance = this as T;
      }
      else
      {
         Destroy(gameObject);
      }
   }
}
