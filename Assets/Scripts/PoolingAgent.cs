using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolingAgent : MonoBehaviour
{
    [SerializeField] PoolingID _poolType;
    PoolingManager myPoolManager;

    private void Awake()
    {
        PoolingManager[] managers = FindObjectsOfType<PoolingManager>();

        foreach (var item in managers)
        {
            if (item.PoolingID == _poolType)
            {
                myPoolManager = item;
            }
        }
    }

    public GameObject InstantiateFromPool(GameObject gameObject, Vector3 position)
    {
        return myPoolManager.PoolingProcess(gameObject, position);
    }
}
