using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "EventNoParameters", menuName = "Event No Parameters")]

public class EventManager : ScriptableObject
{
    private Action newEvent;
    private Action<object> newEvent_Object;
    private Func<object> newEvent_ReturnValue;

    public void AddListener(Action listener)
    {
        newEvent += listener;
    }
    public void AddListener(Action<object> listener)
    {
        newEvent_Object += listener;
    }

    public void AddListener(Func<object> listener)
    {
        newEvent_ReturnValue += listener;
    }

    public void Invoke()
    {
        newEvent?.Invoke();
    }

    public void Invoke(object value)
    {
        newEvent_Object?.Invoke(value);
    }

    public object Return_Parameter()
    {
        var temp = newEvent_ReturnValue?.Invoke();
        return temp;
    }

    public void RemoveListeners()
    {
        newEvent = null;
        newEvent_ReturnValue = null;
        newEvent_Object = null;
        //Debug.Log(newEvent_Object.GetInvocationList().Length);
    }

}




