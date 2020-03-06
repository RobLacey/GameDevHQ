using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolingAgent : MonoBehaviour
{
    [SerializeField] PoolingID _poolType;
    PoolingManager _myPoolManager;

    private void Awake()
    {
        PoolingManager[] allManagers = FindObjectsOfType<PoolingManager>();

        foreach (var poolManager in allManagers)
        {
            if (poolManager.PoolingID == _poolType)
            {
                _myPoolManager = poolManager;
            }
        }
    }

    public GameObject InstantiateFromPool(GameObject gameObject, Vector3 position, Quaternion rotation)
    {
        return _myPoolManager.PoolingProcess(gameObject, position, rotation);
    }
}
