using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventCleaner : MonoBehaviour
{
    [SerializeField] public List<EventManager> events = new List<EventManager>();


    private void OnDisable()
    {
        foreach (var item in events)
        {
            if (item != null)
            {
                item.RemoveListeners();
            }
        }
    }
}


