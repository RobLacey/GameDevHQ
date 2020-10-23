using System;
using UnityEngine;

public interface ICustomEvent<TType>
{
    void RaiseEvent(TType args);
    void AddListener(Action<TType> newEvent);
    void RemoveListener(Action<TType> newEvent);
}

public class CustomEvent<TType> : ICustomEvent<TType>
{
    private event Action<TType> Raise;
    
    public CustomEvent()
    {
        EventLocator.AddEvent(this);
        Application.quitting += OnQuit;
    }

    public void RaiseEvent(TType args) => Raise?.Invoke(args);

    public void AddListener(Action<TType> newEvent)
    {
        Raise -= newEvent;
        Raise += newEvent;
    }

    public void RemoveListener(Action<TType> newEvent) => Raise -= newEvent;

    public void FlushEvent()
    {
        Raise = null;
        EventLocator.FlushEventsList<ICustomEvent<TType>>();
    }

    private void OnQuit() => Raise = null;
}