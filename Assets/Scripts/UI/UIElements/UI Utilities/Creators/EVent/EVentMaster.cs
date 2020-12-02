using System;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;

public class EVentMaster
{
     public static Action<TType> Get<TType>(Hashtable events)
     {
        if (!events.ContainsKey(typeof(TType)))
        {
            HandleNoEvent<TType>();
        }
        else
        {
            var temp = (CustomEVent<TType>) events[typeof(TType)];
            return temp.RaiseEvent;
        }

        return default;
    }

     public static void Subscribe<TType>(Action<TType> listener, Hashtable events)
    {
        if (!events.ContainsKey(typeof(TType)))
        {
            HandleNoEvent<TType>();
        }
        else
        {
            var eVent = (CustomEVent<TType>) events[typeof(TType)];
            eVent.AddListener(listener);
        }
    }

    private static void HandleNoEvent<TType>([CallerMemberName]string from = null)
    {
        Debug.Log($"No Event Bound in {from} : {Environment.NewLine} Please Bind {typeof(TType)}");
    }

    public static void Unsubscribe<TType>(Action<TType> listener, Hashtable events)
    {
        var eVent = (CustomEVent<TType>) events[typeof(TType)];
        eVent.RemoveListener(listener);
    }
}

