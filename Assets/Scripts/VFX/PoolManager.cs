using System.Collections.Generic;
using UnityEngine;

public class PoolManager : SingletonMonobehaviour<PoolManager>
{
    //int是预制体的实例ID，队列里存有一定数量的由预制体创造的物体
    //这个字典是用来保存全部对象池的
    private Dictionary<int, Queue<GameObject>> poolDictionary = new Dictionary<int, Queue<GameObject>>();

    //指明创造的对象池物体在场景中父物体
    [SerializeField] private Transform objectPoolTransform = null;

    //start方法根据pool中设定的值，来初始化poolDictionary
    [SerializeField] private Pool[] pool = null;
    
    
    [System.Serializable]
    public struct Pool
    {
        public int poolSize;
        public GameObject prefab;
    }
    
    //start方法根据pool中设定的值，来初始化poolDictionary
    private void Start()
    {
        //根据pool
        for (int i = 0; i < pool.Length; i++)
        {
            CreatePool(pool[i].prefab, pool[i].poolSize);
        }
    }

    //给定预制体和数量，创造对象池
    private void CreatePool(GameObject prefab, int poolSize)
    {
        
        //获取预制体名字
        string prefabName = prefab.name;
        //为预制体创造一个父物体，取名为预制体名+Anchor
        GameObject parentGameObject = new GameObject(prefabName + "Anchor");
        //设定父物体为objectPoolTransform的子物体
        parentGameObject.transform.SetParent(objectPoolTransform);


        //获取预制体的实例ID
        int poolKey = prefab.GetInstanceID();
        
        if (!poolDictionary.ContainsKey(poolKey))
        {
            poolDictionary.Add(poolKey, new Queue<GameObject>());

            for (int i = 0; i < poolSize; i++)
            {
                GameObject newObject = Instantiate(prefab, parentGameObject.transform) as GameObject;
                
                newObject.SetActive(false);

                poolDictionary[poolKey].Enqueue(newObject);
            }
        }
    }

    //唯一的public方法，外部使用对象池时，只需调用这个方法即可
    public GameObject ReuseObject(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        int poolKey = prefab.GetInstanceID();

        if (poolDictionary.ContainsKey(poolKey))
        {
            //从对象池里的队头取出物体，扔到队尾去，再SetActive=false
            GameObject objectToReuse = GetObjectFromPool(poolKey);

            //重置这个物体的位置和大小
            ResetObject(position, rotation, objectToReuse, prefab);

            return objectToReuse;
        }
        else
        {
            Debug.Log("No object pool for " + prefab);
            return null;
        }
    }

    //获取队头的物体，扔到队尾去，再SetActive=false
    private GameObject GetObjectFromPool(int poolKey)
    {
        GameObject objectToReuse = poolDictionary[poolKey].Dequeue();
        poolDictionary[poolKey].Enqueue(objectToReuse);

        if (objectToReuse.activeSelf == true)
        { 
            objectToReuse.SetActive(false);
        }
        
        return objectToReuse;
    }
    
    //重置传入物体的位置
    private static void ResetObject(Vector3 position, Quaternion rotation, GameObject objectToReuse, GameObject prefab)
    {
        objectToReuse.transform.position = position;
        objectToReuse.transform.rotation = rotation;


        //objectToReuse.GetComponent<Rigidbody2D>().velocity=Vector3.zero;
        objectToReuse.transform.localScale = prefab.transform.localScale;

    }

}
