using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

public interface IEventUser
{
    void ObserveEvents();
    void RemoveFromEvents();
}

public static class EventLocator
{
    private static readonly Hashtable events = new Hashtable();
    private static readonly Dictionary<Type, List<IEventUser>> waitingForEventStore
        = new Dictionary<Type, List<IEventUser>>();
    private static bool locked;

    public static void AddEvent<TType>(ICustomEventOld<TType> newEventOld)
    {
        var eventType = typeof(ICustomEventOld<TType>);
        if(EventExistsAndLocked(eventType)) return;
        
        events.Add(eventType, newEventOld);
        locked = true;
        CheckForWaitingServiceUser(eventType);
    }
    
    public static void AddEventTest<TType>(ICustomEvent<TType> newEvent)
    {
        var eventType = typeof(ICustomEvent<TType>);
        if(EventExistsAndLocked(eventType)) return;
        
        events.Add(eventType, newEvent);
        locked = true;
        CheckForWaitingServiceUser(eventType);
    }
    
    public static void Subscribe<TType>(Action<TType> listener, IEventUser caller)
    {
        var eventType = typeof(ICustomEvent<TType>);
        if (EventDoesNotExistYet(caller, eventType)) return;

        var t = (ICustomEvent<TType>) events[eventType];
        t.AddListener(listener);
    }

    public static void Unsubscribe<TType>(Action<TType> listener)
    {
        var eventType = typeof(ICustomEvent<TType>);
        if (!events.ContainsKey(eventType)) return;
        var t = (ICustomEvent<TType>) events[eventType];
        t.RemoveListener(listener);
    }


    public static void AddEvent<TType, TParameter>(ICustomEvent<TType, TParameter> newEvent)
    {
        var eventType = typeof(ICustomEvent<TType, TParameter>);
        if(EventExistsAndLocked(eventType)) return;
        
        events.Add(eventType, newEvent);
        CheckForWaitingServiceUser(eventType);
    }

    private static bool EventExistsAndLocked(Type eventType)
    {
        if (!events.ContainsKey(eventType)) return false;
        if (locked)
        {
            Debug.Log($"{eventType} already set. Flush existing and Unlock first to set");
            return true;
        }
        return false;
    }

    public static void UnlockEventsList() => locked = false;
    
    /// <summary> Need to supply the EVENT TYPE (as an Interface) </summary>
    public static void SubscribeToEvent<TType>(Action listener, IEventUser caller)
    {
        var eventType = typeof(ICustomEventOld<TType>);
        if (EventDoesNotExistYet(caller, eventType)) return;
        
        var t = (ICustomEventOld<TType>) events[eventType];
        t.AddListener(listener);
    }
    
    /// <summary> Need to supply the EVENT TYPE (as an Interface)
    /// and the passed PARAMETER that matches the delegate type </summary>
    public static void SubscribeToEvent<TType, TParameter>(Action<TParameter> listener, IEventUser caller)
    {
        var eventType = typeof(ICustomEvent<TType, TParameter>);
        if (EventDoesNotExistYet(caller, eventType)) return;

        var t = (ICustomEvent<TType, TParameter>) events[eventType];
        t.AddListener(listener);
    }
    

    /// <summary> Need to supply the EVENT TYPE (as an Interface)
    /// and the returned PARAMETER that matches the delegate type </summary>
    public static void SubscribeToEvent<TType, TParameter>(Func<TParameter> listener, IEventUser caller)
    {
        var eventType = typeof(ICustomEvent<TType, TParameter>);
        if (EventDoesNotExistYet(caller, eventType)) return;

        var t = (ICustomEvent<TType, TParameter>) events[eventType];
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

    public static void UnsubscribeFromEvent<TType>(Action listener)
    {
        var eventType = typeof(ICustomEventOld<TType>);
        if (!events.ContainsKey(eventType)) return;
        var t = (ICustomEventOld<TType>) events[eventType];
        t.RemoveListener(listener);
    }
    
    public static void UnsubscribeFromEvent<TType, TParameter>(Action<TParameter> listener)
    {
        var eventType = typeof(ICustomEvent<TType, TParameter>);
        if (!events.ContainsKey(eventType)) return;
        var t = (ICustomEvent<TType, TParameter>) events[eventType];
        t.RemoveListener(listener);
    }
    
    public static void UnsubscribeFromEvent<TType, TParameter>(Func<TParameter> listener)
    {
        var eventType = typeof(ICustomEvent<TType, TParameter>);
        if (!events.ContainsKey(eventType)) return;
        var t = (ICustomEvent<TType, TParameter>) events[eventType];
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

    /// <summary> Removes the Event from the static events list. Do this when changing scene if you want to
    /// clear event totally for fresh scene </summary>
    public static void FlushEventsList<TType>() => DoFlush(typeof(ICustomEventOld<TType>));

    /// <summary> Removes the Event from the static events list. Do this when changing scene if you want to
    /// clear event totally for fresh scene </summary>

    public static void FlushEventsList<TType, TParameter>() => DoFlush(typeof(ICustomEvent<TType, TParameter>));

    private static void DoFlush(Type type)
    {
        if (events.ContainsKey(type))
        {
            events.Remove(type);
        }
    }
    
    //TODO Add replace event logic. This wil need to use reflection to search existing assembly.
    // Logic will be:
    //Find existing service
    //Get invocationList from custom event (T and T, T versions)
    //Flush custom Event (T and T,T versions)
    //Remove from events
    //Add new and subscribe all people on old invocation list
}

