using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventCleaner : MonoBehaviour
{
    List<EventManager> newEvents = new List<EventManager>();
    [SerializeField] EventInvocationLists _eventInvocationLists;

    private void OnDisable()
    {
        foreach (var item in newEvents)
        {
            if (item != null)
            {
                item.RemoveListeners();
            }
        }

        if (_eventInvocationLists != null)
        {
            _eventInvocationLists.SortLists(); 
        }
    }

    public void AddListenerToClear(EventManager eventManager)
    {
        if (!newEvents.Contains(eventManager))
        {
            newEvents.Add(eventManager);
        }
    }
}


