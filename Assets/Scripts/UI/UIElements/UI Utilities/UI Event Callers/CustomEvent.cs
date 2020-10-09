using System;
using UnityEngine;

public interface ICustomEvent<T>
{
    void RaiseEvent();
    void AddListener(Action newEvent);
    void RemoveListener(Action newEvent);
}

public class CustomEvent<T> : ICustomEvent<T>
{
    private Action _raise;
    
    public CustomEvent()
    {
        EventLocator.AddEvent(this);
        Application.quitting += OnQuit;
    }

    public void RaiseEvent() => _raise?.Invoke();

    public void AddListener(Action newEvent) => _raise += newEvent;

    public void RemoveListener(Action newEvent) => _raise -= newEvent;

    private void OnQuit() => _raise = null;
}