using UnityEngine;

public class NullBucketObject : IBucketCreator, IIsAService
{
    private Transform _parent;
    private string _name;

    public IBucketCreator SetParent(Transform parent)
    {
        Debug.Log($"Set Parent to { parent }");
        return this;
    }

    public IBucketCreator SetName(string name)
    {
        Debug.Log($"Set Name to { name }");
        return this;
    }

    public Transform CreateBucket()
    {
        Debug.Log("Null Bucket Service Running - Expect inconsistent behaviour");
        return null;
    }

    public void OnDisable()
    {
        Debug.Log("Disabled");
    }
}