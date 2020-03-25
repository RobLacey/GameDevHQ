using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventCleaner : MonoBehaviour
{
    List<EventManager> newEvents = new List<EventManager>();

    private void OnDisable()
    {
        foreach (var item in newEvents)
        {
            if (item != null)
            {
                item.RemoveListeners();
            }
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


