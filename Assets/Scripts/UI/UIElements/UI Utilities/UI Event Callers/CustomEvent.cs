using System;
using UnityEngine;

public interface ICustomEvent<T>
{
    void RaiseEvent();
    void AddListener(Action newEvent);
    void RemoveListener(Action newEvent);
}

public interface ICustomEvent<T1, T2>
{
    void RaiseEvent(T2 obj);
    void AddListener(Action<T2> newEvent);
    void RemoveListener(Action<T2> newEvent);
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

    public void AddListener(Action newEvent)
    {
        _raise -= newEvent;
        _raise += newEvent;
    }

    public void RemoveListener(Action newEvent) => _raise -= newEvent;

    private void OnQuit() => _raise = null;
}

public class CustomEvent<T1, T2> : ICustomEvent<T1, T2>
{
    private Action<T2> _raise;
    
    public CustomEvent()
    {
        EventLocator.AddEvent(this);
        Application.quitting += OnQuit;
    }

    public void RaiseEvent(T2 obj) => _raise?.Invoke(obj);

    public void AddListener(Action<T2> newEvent)
    {
        _raise -= newEvent;
        _raise += newEvent;
    }

    public void RemoveListener(Action<T2> newEvent) => _raise -= newEvent;

    private void OnQuit() => _raise = null;
}

//********* TODO Make sure events are removed and test it works *********




