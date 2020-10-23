using System;
using System.Linq;
using TMPro.SpriteAssetUtilities;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public interface ICustomEventOld<TType>
{
    void RaiseEvent();
    void AddListener(Action newEvent);
    void RemoveListener(Action newEvent);
}

public interface ICustomEvent<TType>
{
    void RaiseEvent(TType args);
    void AddListener(Action<TType> newEvent);
    void RemoveListener(Action<TType> newEvent);
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

public class CustomEventOldOld<TType> : ICustomEventOld<TType>
{
    private event Action Raise;
    
    public CustomEventOldOld()
    {
        EventLocator.AddEvent(this);
        Application.quitting += OnQuit;
    }

    public void RaiseEvent() => Raise?.Invoke();

    public void AddListener(Action newEvent)
    {
        Raise -= newEvent;
        Raise += newEvent;
    }

    public void RemoveListener(Action newEvent) => Raise -= newEvent;

    public void FlushEvent()
    {
        Raise = null;
        EventLocator.FlushEventsList<ICustomEventOld<TType>>();
    }

    private void OnQuit() => Raise = null;
}

public class CustomEvent<TType, TParameter> : ICustomEvent<TType, TParameter>
{
    private event Action<TParameter> Raise;
    private event Func<TParameter> RaiseReturn;
    
    public CustomEvent()
    {
        EventLocator.AddEvent(this);
        Application.quitting += OnQuit;
    }

    public void RaiseEvent(TParameter obj) => Raise?.Invoke(obj);

    public TParameter RaiseEvent() => RaiseReturn is null ? default : RaiseReturn.Invoke();

    public void AddListener(Action<TParameter> newEvent)
    {
        Raise -= newEvent;
        Raise += newEvent;
    }
    
    public void AddListener(Func<TParameter> newEvent)
    {
        RaiseReturn -= newEvent;
        RaiseReturn += newEvent;
    }

    public void RemoveListener(Action<TParameter> newEvent) => Raise -= newEvent;

    public void RemoveListener(Func<TParameter> newEvent) => RaiseReturn -= newEvent;

    public void FlushEvent()
    {
        Raise = null;
        EventLocator.FlushEventsList<ICustomEvent<TType, TParameter>>();
    }

    private void OnQuit() => Raise = null;
    
    
}

public class CustomEvent<TType> : ICustomEvent<TType>
{
    private event Action<TType> Raise;
    
    public CustomEvent()
    {
        EventLocator.AddEventTest(this);
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
        EventLocator.FlushEventsList<ICustomEventOld<TType>>();
    }

    private void OnQuit() => Raise = null;
}


//********* TODO Make sure events are removed and test it works *********




