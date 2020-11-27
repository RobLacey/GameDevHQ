using System;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EVentMaster
{
    private EVentMaster()
    {
        Application.quitting += FlushOnAppExit;
        SceneManager.activeSceneChanged += FlushOnSceneChange;
    }
    
    private static readonly Hashtable events = new Hashtable();
    private static EVentMaster eVentMaster = new EVentMaster();

     public static void CreateEvent<TType>() => events.Add(typeof(TType), new CustomEVent<TType>());

     public static Action<TType> Get<TType>()
    {
        if (!events.Contains(typeof(TType)))
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
    
    public static void Subscribe<TType>(Action<TType> listener)
    {
        if (!events.Contains(typeof(TType)))
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

    public static void Unsubscribe<TType>(Action<TType> listener)
    {
        var eVent = (CustomEVent<TType>) events[typeof(TType)];
        
        eVent.RemoveListener(listener);
    }

    private void FlushOnAppExit()
    {
        Debug.Log("Flushed Events on Exit");
    }

    private void FlushOnSceneChange(Scene current, Scene next)
    {
        //Need To Get Active scene and check
        Debug.Log("Scene Change - Events Flushed");
    }
}