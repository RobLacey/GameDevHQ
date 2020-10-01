using UnityEngine;

public interface IBucketCreator
{
    void SetParentTransFrom(Transform parent);
    Transform CreateBucket();
}

public class BucketCreator : IBucketCreator
{
    private Transform _parent;
    private readonly string _name;
    private Transform _bucket;

    public BucketCreator(Transform parent, string name)
    {
        _parent = parent;
        _name = name;
    }
    public void SetParentTransFrom(Transform parent)
    {
        _parent = parent;
    }
    
    public Transform CreateBucket()
    {
        if (GameObject.Find(_name)) return _bucket;
        var newBucket = new GameObject();
        newBucket.transform.parent = _parent;
        newBucket.AddComponent<RectTransform>();
        newBucket.GetComponent<RectTransform>().anchoredPosition3D = Vector3.zero;
        newBucket.name = _name;
        _bucket = newBucket.transform;
        return newBucket.transform;
    }
}

public class NullObject : IBucketCreator
{
    public void SetParentTransFrom(Transform parent) { }

    public Transform CreateBucket()
    {
        Debug.Log("Null Bucket Service Running - Expect inconsistent behaviour");
        return null;
    }
}

