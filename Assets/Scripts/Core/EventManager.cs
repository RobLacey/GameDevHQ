using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "Event", menuName = "New Event")]

public class EventManager : ScriptableObject
{
    private Action newEvent;
    private Action<object> newEvent_Object;
    private Action<object, object> newEvent_Object_Object;
    public Func<object> newEvent_ReturnValue;



    //Add listener Methods
    public void AddListener(Action listener)
    {
        newEvent += listener;
        AddToEventCleaner();
    }

    public void AddListener(Action<object> listener)
    {
        newEvent_Object += listener;
        AddToEventCleaner();
    }

    public void AddListener(Action<object, object> listener)
    {
        newEvent_Object_Object += listener;
        AddToEventCleaner();
    }

    public void AddListener(Func<object> listener)
    {
        newEvent_ReturnValue += listener;
        AddToEventCleaner();
    }

    //Invoke Methods

    public void Invoke()
    {
        newEvent?.Invoke();
    }

    public void Invoke(object value)
    {
        newEvent_Object?.Invoke(value);
    }
    public void Invoke(object value1, object value2)
    {
        newEvent_Object_Object?.Invoke(value1, value2);
    }

    public object Return_Parameter()
    {
        var temp = newEvent_ReturnValue?.Invoke();
        return temp;
    }

    //Remove Listener Methods
    private void AddToEventCleaner()
    {
        EventCleaner eventCleaner = FindObjectOfType<EventCleaner>();
        eventCleaner.AddListenerToClear(this);
    }

    public void RemoveListeners()
    {
        newEvent = null;
        newEvent_ReturnValue = null;
        newEvent_Object = null;
        newEvent_Object_Object = null;
        //Debug.Log(newEvent_Object.GetInvocationList().Length);
    }
}




