using System;
using System.Collections;
using UnityEngine;

public interface IEVentSub : IEVentBase { }


public class EVentSub : EVentBaseClass<EVentSub>, IEVentSub  
{
    public override Hashtable EVentsList { get; set; } = new Hashtable();
    public override Action<T> Fetch<T>()
    {
        Debug.Log("Fetch");
        return default;
    }

    public override void Subscribe<TType>(Action<TType> listener)
    {
        Debug.Log("Subscribe");
    }

    public override void Unsubscribe<T>(Action<T> listener)
    {
        Debug.Log("Unsubscribe");
    }
}