﻿using System;
using UnityEngine;

public interface IBucketCreator 
{
    IBucketCreator SetParent(Transform parent);
    IBucketCreator SetName(String name);
    Transform CreateBucket();
}

public class BucketCreator : IBucketCreator, IIsAService
{
    private Transform _parent;
    private string _name;
    private Transform _bucket;

    public IBucketCreator SetParent(Transform parent)
    {
        _parent = parent;
        return this;
    }

    public IBucketCreator SetName(string name)
    {
        _name = name;
        return this;
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

    public void OnDisable()
    {
        ;
    }
}