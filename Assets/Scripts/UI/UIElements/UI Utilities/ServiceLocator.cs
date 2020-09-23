
using System;
using UnityEngine;

public class ServiceLocator
{
    private static IBucketCreator service;

    public static void SetBucketCreateService(IBucketCreator newService) => service = newService;
    public static IBucketCreator GetBucket() => service ?? new NullObject();
}

public class NullObject : IBucketCreator
{
    public void SetParentTransFrom(Transform parent) { }

    public Transform CreateBucket() => throw new Exception("No Service Found ");
}
