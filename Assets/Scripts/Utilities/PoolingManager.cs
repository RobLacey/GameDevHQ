using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolingManager : MonoBehaviour
{
    [SerializeField] public GameObject newPoolPrefab;
    [SerializeField] PoolingID _poolType;
    [SerializeField] Pool[] _poolCollection;
    string _poolingFolderName = " Pool";

    public PoolingID PoolingID { get { return _poolType; } }

    [System.Serializable]
    public class Pool //Maybe have scriptable Object
    {
        [SerializeField] public string name; 
        [SerializeField] public GameObject prefab;
        [SerializeField] public int startingPrefabs = 0;
        [HideInInspector] public Transform _poolFolder = null;
    
        public Queue<GameObject> _prefabPool = new Queue<GameObject>();
    }

    private void Awake()
    {
        if (_poolCollection.Length == 0) return;

        foreach (var pool in _poolCollection)
        {
            StartNewPool(pool);

            if (pool.startingPrefabs > 0)
            {
                PopulatePool(pool);
            }
        }
    }

    public void StartNewPool(Pool pool)
    {
        GameObject newObject = Instantiate(newPoolPrefab, transform.position, Quaternion.identity, transform);
        newObject.name  = pool.name + _poolingFolderName;
        pool._poolFolder = newObject.transform;
    }

    private void PopulatePool(Pool poolOfObjects)
    {
        for (int index = 0; index < poolOfObjects.startingPrefabs; index++)
        {
            GameObject newObject = AddObjectToPool(poolOfObjects, transform.position, Quaternion.identity);
            newObject.SetActive(false);
        }
    }

    public GameObject PoolingProcess(GameObject gameObjectToCreate, Vector3 newPosition, Quaternion rotation)
    {
        foreach (var pool in _poolCollection)
        {
            if (pool.prefab == gameObjectToCreate)
            {
                foreach (var prefab in pool._prefabPool)
                {
                    if (!prefab.activeSelf)
                    {
                        prefab.transform.position = newPosition;
                        prefab.SetActive(true);
                        return prefab;
                    }
                }
                return AddObjectToPool(pool, newPosition, rotation);
            }
        }
        Debug.Log("No prefab found!! " + gameObjectToCreate);
        return newPoolPrefab;
    }

    private GameObject AddObjectToPool(Pool thisPool,  Vector3 newPosition, Quaternion rotation)
    {
        GameObject newObject = Instantiate(thisPool.prefab, newPosition, 
                                           rotation, thisPool._poolFolder);
        thisPool._prefabPool.Enqueue(newObject);
        return newObject;
    }

}
