using System.Collections.Generic;
using UnityEngine;

public class ObjectPool
{
    private PoolableObject Prefab;
    private int Size;
    public List<PoolableObject> AvailableObjectsPool;

    private ObjectPool(PoolableObject Prefab, int Size)
    {
        this.Prefab = Prefab;
        this.Size = Size;
        AvailableObjectsPool = new List<PoolableObject>(Size);
    }

    public static ObjectPool CreateInstance(PoolableObject Prefab, int Size)
    {
        ObjectPool pool = new ObjectPool(Prefab, Size);

        GameObject poolGameObject = new GameObject(Prefab + " Pool");
        pool.CreateObjects(poolGameObject);

        return pool;
    }

    private void CreateObjects(GameObject parent)
    {
        for (int i = 0; i < Size; i++)
        {
            PoolableObject poolableObject = GameObject.Instantiate(Prefab, Vector3.zero, Quaternion.identity, parent.transform);
            poolableObject.Parent = this;
            poolableObject.gameObject.SetActive(false);
        }
    }

    public PoolableObject GetObject()
    {
        if (AvailableObjectsPool.Count == 0)
        {
            return null;
        }

        PoolableObject instance = AvailableObjectsPool[0];
        AvailableObjectsPool.RemoveAt(0);
        instance.gameObject.SetActive(true);

        return instance;
    }

    public void ReturnObjectToPool(PoolableObject Object)
    {
        AvailableObjectsPool.Add(Object);
    }
}
