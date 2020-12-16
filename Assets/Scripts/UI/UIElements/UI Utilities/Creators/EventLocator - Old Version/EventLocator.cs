using System;
using System.Collections;
using System.Collections.Generic;
using Debug = UnityEngine.Debug;

public interface IEventDispatcher
{
    void FetchEvents();
}

public interface IEventUser
{
    void ObserveEvents();
}

public static class EventLocator
{
    private static readonly Hashtable events = new Hashtable();
    private static readonly Dictionary<Type, List<IEventUser>> waitingForEventStore
        = new Dictionary<Type, List<IEventUser>>();
    private static bool locked;
    
    public static void AddEvent<TType>(ICustomEvent<TType> newEvent)
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
    
    private static bool EventDoesNotExistYet(IEventUser caller, Type eventType)
    {
        if (events.ContainsKey(eventType))
        {
            return false;
        }
        
        if (!waitingForEventStore.ContainsKey(eventType))
        {
            waitingForEventStore.Add(eventType, new List<IEventUser>());
        }
        waitingForEventStore[eventType].Add(caller);
        return true;
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
    public static void FlushEventsList<TType>() => DoFlush(typeof(ICustomEvent<TType>));

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

