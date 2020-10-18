using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public interface IEventUser
{
    void ObserveEvents();
    void RemoveFromEvents();
    //void ChangeEvent();
}

public static class EventLocator
{
    private static readonly Hashtable events = new Hashtable();
    private static readonly Dictionary<Type, List<IEventUser>> waitingForEventStore
        = new Dictionary<Type, List<IEventUser>>();
    private static bool locked;

    public static void AddEvent<T>(ICustomEvent<T> newEvent)
    {
        var eventType = typeof(ICustomEvent<T>);
        if(EventExistsAndLocked(eventType)) return;
        
        events.Add(eventType, newEvent);
        CheckForWaitingServiceUser(eventType);
    }

    public static void AddEvent<T1, T2>(ICustomEvent<T1, T2> newEvent)
    {
        var eventType = typeof(ICustomEvent<T1, T2>);
        if(EventExistsAndLocked(eventType)) return;
        
        events.Add(eventType, newEvent);
        CheckForWaitingServiceUser(eventType);
    }

    private static bool EventExistsAndLocked(Type eventType)
    {
        if (!events.ContainsKey(eventType)) return false;
        if (locked)
        {
            Debug.Log($"{eventType} already set. Unlock first to set");
            return true;
        }
        events.Remove(eventType);
        locked = true;
        return false;
    }

    public static void UnlockEventsList() => locked = false;
    
    public static void SubscribeToEvent<T>(Action listener, IEventUser caller)
    {
        var eventType = typeof(ICustomEvent<T>);
        if (EventDoesNotExistYet(caller, eventType)) return;
        
        var t = (ICustomEvent<T>) events[eventType];
        t.AddListener(listener);
    }
    
    public static void SubscribeToEvent<T1, T2>(Action<T2> listener, IEventUser caller)
    {
        var eventType = typeof(ICustomEvent<T1, T2>);
        if (EventDoesNotExistYet(caller, eventType)) return;

        var t = (ICustomEvent<T1, T2>) events[eventType];
        t.AddListener(listener);
    }

    private static bool EventDoesNotExistYet(IEventUser caller, Type eventType)
    {
        if (events.ContainsKey(eventType)) return false;
        
        if (!waitingForEventStore.ContainsKey(eventType))
        {
            waitingForEventStore.Add(eventType, new List<IEventUser>());
        }
        waitingForEventStore[eventType].Add(caller);
        return true;
    }

    public static void UnsubscribeFromEvent<T>(Action listener)
    {
        var eventType = typeof(ICustomEvent<T>);
        if (!events.ContainsKey(eventType)) return;
        var t = (ICustomEvent<T>) events[eventType];
        t.RemoveListener(listener);
    }
    
    public static void UnsubscribeFromEvent<T1, T2>(Action<T2> listener)
    {
        var eventType = typeof(ICustomEvent<T1, T2>);
        if (!events.ContainsKey(eventType)) return;
        var t = (ICustomEvent<T1, T2>) events[eventType];
        t.RemoveListener(listener);
    }
    
    /// <summary> /// Add Services to objects that have asked for it but service doesn't exist /// </summary>
    
    private static void CheckForWaitingServiceUser(Type type)
    {
        if(waitingForEventStore.Count == 0 || 
           !waitingForEventStore.ContainsKey(type)) return;
        
        foreach (var listener in waitingForEventStore[type])
        {
           // Debug.Log($"{type} Added to : " + listener);
            listener.ObserveEvents();
        }

        waitingForEventStore.Remove(type);
    }
}

