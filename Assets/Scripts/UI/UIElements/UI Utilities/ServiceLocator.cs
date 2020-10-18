using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IServiceUser
{
    void SubscribeToService();
    //void ChangeService();
}

public static class ServiceLocator
{
    private static readonly Hashtable services = new Hashtable();
    private static readonly Dictionary<Type, List<IServiceUser>> waitingForServicesStore 
                            = new Dictionary<Type, List<IServiceUser>>();
    private static bool locked;

    public static void AddService<T>(T newService)
    {
        if(services.ContainsKey(typeof(T)) && !locked)
            services.Remove(typeof(T));
        
        if(services.ContainsKey(typeof(T)) && locked)
        {
            Debug.Log("Service already set. Unlock first to set");
            return;
        }
        
        locked = true;
        services.Add(typeof(T), newService);
        CheckForWaitingServiceUser(typeof(T));
    }
    
    public static T GetNewService<T>(IServiceUser user)
    {
        if (!services.ContainsKey(typeof(T)))
        {
            if (!waitingForServicesStore.ContainsKey(typeof(T)))
            {
                waitingForServicesStore.Add(typeof(T), new List<IServiceUser>());
                //Debug.Log("Make new Service store for : " + typeof(T));
            }
            waitingForServicesStore[typeof(T)].Add(user);
            //Debug.Log("No Service so save method : " + user);
        }
        return (T) services[typeof(T)];
    }
    
    public static void UnlockService() => locked = false;
    
    //
    //  *****TODO Add Remove Service *******
    //
    private static void CheckForWaitingServiceUser(Type type)
    {
        if(waitingForServicesStore.Count == 0 || !waitingForServicesStore.ContainsKey(type)) return;
        
        foreach (var service in waitingForServicesStore[type])
        {
           // Debug.Log("Subscribe : " + service);
            service.SubscribeToService();
        }
        waitingForServicesStore.Remove(type);
    }
}