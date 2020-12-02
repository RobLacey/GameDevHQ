using System;
using System.Linq;
using UnityEngine;

public class CustomEVent<TType>
{
    private event Action<TType> Raise;

    public void RaiseEvent(TType args) => Raise?.Invoke(args);

    public void AddListener(Action<TType> newEvent)
    {
        Raise -= newEvent;
        Raise += newEvent;
    }

    public void RemoveListener(Action<TType> newEvent) => Raise -= newEvent;
}