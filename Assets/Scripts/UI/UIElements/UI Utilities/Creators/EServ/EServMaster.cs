using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Debug = UnityEngine.Debug;

public interface IEServUser
{
    void UseEServLocator();
}

public interface IEServService
{
    void AddService();
    void OnDisable();
}

public class EServMaster
{
    private readonly Hashtable _services = new Hashtable();
    private readonly Dictionary<Type, List<IEServUser>> _waitingForServicesStore 
                            = new Dictionary<Type, List<IEServUser>>();
    private bool _locked;
    private bool _started;

    public void AddNew<T>(T newService)
    {
        SetUpForNewScene(); 
        
        if(_services.ContainsKey(typeof(T)) && !_locked)
            RemoveService<T>();
        
        if(_services.ContainsKey(typeof(T)) && _locked)
        {
            Debug.Log($"Service : {typeof(T)} already set. Unlock first to set");
            return;
        }
        
        _locked = true;
        _services.Add(typeof(T), newService);
        CheckForWaitingServiceUser(typeof(T));
    }

    private void SetUpForNewScene()
    {
        if (_started) return;
        EVent.Do.Subscribe<ISceneChange>(OnSceneEnd);
        Application.quitting += OnAppExit;
        _started = true;
    }

    public T Get<T>(IEServUser ieServUser)
    {
        if (!_services.ContainsKey(typeof(T)))
        {
            if (!_waitingForServicesStore.ContainsKey(typeof(T)))
            {
                _waitingForServicesStore.Add(typeof(T), new List<IEServUser>());
            }
            
            _waitingForServicesStore[typeof(T)].Add(ieServUser);
        }

        return (T) _services[typeof(T)];
    }
    
    public EServMaster Unlock()
    {
        _locked = false;
        return this;
    }
    
    /// <summary> Makes sure na new scene has no active services </summary>
    private void RemoveService<T>()
    {
        if (_services.ContainsKey(typeof(T)))
        {
            var temp = (IEServService) _services[typeof(T)];
            temp.OnDisable();
            _services.Remove(typeof(T));
        }
    }
    
    private void CheckForWaitingServiceUser(Type type)
    {
        if(_waitingForServicesStore.Count == 0 || !_waitingForServicesStore.ContainsKey(type)) return;
        
        foreach (var service in _waitingForServicesStore[type])
        {
            service.UseEServLocator();
        }
        _waitingForServicesStore.Remove(type);
    }

    private void OnSceneEnd(ISceneChange args) => CleanUpServices();

    private void OnAppExit() => CleanUpServices();

    private void CleanUpServices()
    {
        Application.quitting -= OnAppExit;
        EVent.Do.Unsubscribe<ISceneChange>(OnSceneEnd);
        _started = false;
        _services.Clear();
    }
}




