using System;
using System.Collections;
using UnityEngine;

public static class ServiceLocator
{
    private static readonly Hashtable services = new Hashtable();
    private static bool locked;

    public static void AddService<T>(T newService)
    {
        if(services.ContainsKey(typeof(T)) && !locked)
            services.Remove(typeof(T));
        
        if(services.ContainsKey(typeof(T)) &&locked)
        {
            HandleLocked();
            return;
        }
        locked = true;
        services.Add(typeof(T), newService);
    }
    
    public static T GetNewService<T>()
    {
        if (!services.ContainsKey(typeof(T)))
        {
            throw new Exception("No Service Found ");
        }
        return (T) services[typeof(T)];
    }
    
    public static void UnlockService() => locked = false;

    private static void HandleLocked()
    {
        Debug.Log("Service already set. Unlock first to set");
    }
}


