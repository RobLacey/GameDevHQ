using System;
using UnityEngine;

public interface ICustomEvent<TType>
{
    void RaiseEvent();
    void AddListener(Action newEvent);
    void RemoveListener(Action newEvent);
}

public interface ICustomEvent<TType, T2>
{
    void RaiseEvent(T2 obj);
    T2 RaiseEvent();
    void AddListener(Action<T2> newEvent);
    void AddListener(Func<T2> newEvent);
    void RemoveListener(Action<T2> newEvent);
    void RemoveListener(Func<T2> newEvent);
}

public class CustomEvent<TType> : ICustomEvent<TType>
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

    public void FlushEvent()
    {
        _raise = null;
        EventLocator.FlushEventsList<ICustomEvent<TType>>();
    }

    private void OnQuit() => _raise = null;
}

public class CustomEvent<TType, TParameter> : ICustomEvent<TType, TParameter>
{
    private Action<TParameter> _raise;
    private Func<TParameter> _raiseReturn;
    
    public CustomEvent()
    {
        EventLocator.AddEvent(this);
        Application.quitting += OnQuit;
    }

    public void RaiseEvent(TParameter obj) => _raise?.Invoke(obj);

    public TParameter RaiseEvent() => _raiseReturn.Invoke();

    public void AddListener(Action<TParameter> newEvent)
    {
        _raise -= newEvent;
        _raise += newEvent;
    }
    
    public void AddListener(Func<TParameter> newEvent)
    {
        _raiseReturn -= newEvent;
        _raiseReturn += newEvent;
    }

    public void RemoveListener(Action<TParameter> newEvent) => _raise -= newEvent;

    public void RemoveListener(Func<TParameter> newEvent) => _raiseReturn -= newEvent;

    public void FlushEvent()
    {
        _raise = null;
        EventLocator.FlushEventsList<ICustomEvent<TType, TParameter>>();
    }

    private void OnQuit() => _raise = null;
}


//********* TODO Make sure events are removed and test it works *********




