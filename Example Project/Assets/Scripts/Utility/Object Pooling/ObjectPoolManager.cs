using System;
using System.Collections.Generic;
using UnityEngine;

//namespace VirtualVoid.Util.ObjectPooling
//{
public class ObjectPoolManager : MonoBehaviour
{
    private static ObjectPoolManager _instance;
    private static ObjectPoolManager instance
    {
        get
        {
            if (_instance == null)
                _instance = new GameObject("Object Pool Manager").AddComponent<ObjectPoolManager>();
            return _instance;
        }
        set
        {
            _instance = value;
        }
    }
    private void Awake()
    {
        instance = this;
    }

    public class ObjectPool
    {
        public GameObject prefab; 
        public Transform poolHolder;
        public Queue<PooledObjectInstance> pool;

        public ObjectPool(GameObject prefab, Transform poolHolder, Queue<PooledObjectInstance> pool)
        {
            this.prefab = prefab;
            this.poolHolder = poolHolder;
            this.pool = pool;
        }
    }

    [Header("Created when the game starts. Can also call CreatePool()")]
    public List<InspectorObjectPool> inspectorPools = new List<InspectorObjectPool>();
    public Dictionary<PooledObject, ObjectPool> objectPools = new Dictionary<PooledObject, ObjectPool>();

    private void Start()
    {
        foreach (InspectorObjectPool pool in inspectorPools)
        {
            CreatePool(pool);
        }
    }

    private static void CreatePool(InspectorObjectPool pool)
    {
        if (instance.objectPools.ContainsKey(pool.objectType))
        {
            Debug.LogWarning($"Tried to create pool with type '{pool.objectType}', but a pool with that type already exists!");
            return;
        }

        Transform holder = new GameObject(pool.name + " - Object Pool").transform;
        holder.parent = instance.transform;

        Queue<PooledObjectInstance> objectPool = new Queue<PooledObjectInstance>(pool.numToSpawn);

        for (int i = 0; i < pool.numToSpawn; i++)
        {
            PooledObjectInstance obj = new PooledObjectInstance(Instantiate(pool.prefab, holder));
            objectPool.Enqueue(obj);
        }

        instance.objectPools.Add(pool.objectType, new ObjectPool(pool.prefab, holder, objectPool));
    }

    public static GameObject GetObject(PooledObject objectType)
    {
        if (instance.objectPools.TryGetValue(objectType, out ObjectPool pool))
        {
            PooledObjectInstance obj = pool.pool.Dequeue();
            if (obj.gameObject.activeInHierarchy)
            {
                IncreasePoolSize(objectType, pool.pool.Count);
                Debug.Log($"Doubled size of {objectType} pool ({pool.pool.Count} => {pool.pool.Count * 2}) due to dequeueing an active object.");
            }
            pool.pool.Enqueue(obj);

            obj.Spawn();
            return obj.gameObject;
        }
        else
        {
            Debug.LogWarning($"Tried to get object from pool '{objectType}', but pool does not exist! Call the CreatePool() method?");
        }

        return null;
    }

    public static GameObject GetObject(PooledObject objectType, Vector3 position, Quaternion rotation)
    {
        if (instance.objectPools.TryGetValue(objectType, out ObjectPool pool))
        {
            PooledObjectInstance obj = pool.pool.Dequeue();
            if (obj.gameObject.activeInHierarchy)
            {
                IncreasePoolSize(objectType, pool.pool.Count);
                Debug.Log($"Doubled size of {objectType} pool ({pool.pool.Count} => {pool.pool.Count * 2}) due to dequeueing an active object.");
            }
            pool.pool.Enqueue(obj);

            obj.Spawn(position, rotation);
            return obj.gameObject;
        }
        else
        {
            Debug.LogWarning($"Tried to get object from pool '{objectType}', but pool does not exist! Call the CreatePool() method?");
        }

        return null;
    }

    public static void IncreasePoolSize(PooledObject objectType, int numAdditionalObjects = 1)
    {
        if (numAdditionalObjects <= 0) return;

        if (instance.objectPools.TryGetValue(objectType, out ObjectPool pool))
        {
            for (int i = 0; i < numAdditionalObjects; i++)
            {
                PooledObjectInstance obj = new PooledObjectInstance(Instantiate(pool.prefab, pool.poolHolder));
                pool.pool.Enqueue(obj);
            }
        }
        else
        {
            Debug.LogWarning($"Tried to increase size of pool '{objectType}', but pool does not exist! Call the CreatePool() method?");
        }
    }

    public static void DecreasePoolSize(PooledObject objectType, int numObjectsToDelete = 1)
    {
        if (numObjectsToDelete <= 0) return;

        if (instance.objectPools.TryGetValue(objectType, out ObjectPool pool))
        {
            if (numObjectsToDelete > pool.pool.Count)
            {
                Debug.LogWarning($"Tried to remove more items from pool than the pools entire size! \n-Pool: {objectType}\n-Pool size: {pool.pool.Count}\n-Num objects requested: {numObjectsToDelete})");
                numObjectsToDelete = pool.pool.Count;
            }

            for (int i = 0; i < numObjectsToDelete; i++)
            {
                PooledObjectInstance obj = pool.pool.Dequeue();
                obj.Destroy();
            }

            if (pool.pool.Count == 0)
            {
                instance.objectPools.Remove(objectType);
            }
        }
        else
        {
            Debug.LogWarning($"Tried to decrease size of pool '{objectType}', but pool does not exist! Call the CreatePool() method?");
        }
    }

    public static void CreatePool(PooledObject objectType, GameObject prefab, int numObjects)
    {
        CreatePool(new InspectorObjectPool(objectType, prefab, numObjects));
    }
}

[Serializable]
public class InspectorObjectPool
{
    [Tooltip("Just for inspector")]
    public string name;

    public PooledObject objectType;

    [Tooltip("The prefab to spawn copies of")]
    public GameObject prefab;

    [Tooltip("The number of objects to spawn when the game starts")]
    public int numToSpawn;

    public InspectorObjectPool(PooledObject objectType, GameObject prefab, int numToSpawn)
    {
        this.name = objectType.ToString();
        this.objectType = objectType;
        this.prefab = prefab;
        this.numToSpawn = numToSpawn;
    }
}

public class PooledObjectInstance
{
    public GameObject gameObject { get; set; }
    Transform transform;

    bool isPooledObject;
    PoolObject poolObject;

    public PooledObjectInstance(GameObject objectInstance)
    {
        gameObject = objectInstance;
        gameObject.SetActive(false);

        transform = gameObject.transform;

        if (gameObject.TryGetComponent(out poolObject))
        {
            isPooledObject = true;
        }
    }

    public void Spawn()
    {
        gameObject.SetActive(true);

        if (isPooledObject) poolObject.OnObjectSpawn();
    }

    public void Spawn(Vector3 position, Quaternion rotation)
    {
        transform.position = position;
        transform.rotation = rotation;

        Spawn();
    }

    public void Destroy()
    {
        UnityEngine.Object.Destroy(gameObject);
        transform = null;
        poolObject = null;
    }
}
//}
