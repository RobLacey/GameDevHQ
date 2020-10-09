using System;
using System.Collections;
using System.Collections.Generic;
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
    private static readonly Dictionary<Type, List<Action>> waitingForEventStore
        = new Dictionary<Type, List<Action>>();
    private static bool locked;

    public static void AddEvent<T>(ICustomEvent<T> newEvent)
    {
        if(events.ContainsKey(typeof(ICustomEvent<T>)) && !locked)
            events.Remove(typeof(ICustomEvent<T>));
        
        if(events.ContainsKey(typeof(ICustomEvent<T>)) && locked)
        {
            Debug.Log("Event already set. Unlock first to set");
            return;
        }
        
        locked = true;
        events.Add(typeof(ICustomEvent<T>), newEvent);
        CheckForWaitingServiceUser(newEvent);
    }
    
    public static void UnlockEventsList() => locked = false;
    
    public static void SubscribeToEvent<T>(Action listener)
    {
        if (!events.ContainsKey(typeof(ICustomEvent<T>)))
        {
            if (!waitingForEventStore.ContainsKey(typeof(ICustomEvent<T>)))
            {
                waitingForEventStore.Add(typeof(ICustomEvent<T>), new List<Action>());
                //Debug.Log("Make new Event store for : " + typeof(ICustomEvent<T>));
            }
            waitingForEventStore[typeof(ICustomEvent<T>)].Add(listener);
            //Debug.Log("No Event so save method : " + listener.Method);
            return;
        }
        var t = (ICustomEvent<T>) events[typeof(ICustomEvent<T>)];
        t.AddListener(listener);
    }

    public static void UnsubscribeFromEvent<T>(Action listener)
    {
        if (!events.ContainsKey(typeof(ICustomEvent<T>))) return;
        var t = (ICustomEvent<T>) events[typeof(ICustomEvent<T>)];
        t.RemoveListener(listener);
    }

    private static void CheckForWaitingServiceUser<T>(ICustomEvent<T> customEvent)
    {
        if(waitingForEventStore.Count == 0) return;
        
        foreach (var listener in waitingForEventStore[typeof(ICustomEvent<T>)])
        {
            //Debug.Log("Event Added to : " + listener);
            customEvent.AddListener(listener);
        }

        waitingForEventStore.Remove(typeof(ICustomEvent<T>));
    }
}