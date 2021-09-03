using System;
using System.Collections;
using System.Collections.Generic;
using Debug = UnityEngine.Debug;


namespace EZ.Service
{
    public class EZServiceMaster
    {
        private readonly Hashtable _services = new Hashtable();
        private readonly Dictionary<Type, List<IServiceUser>> _waitingForServicesStore 
            = new Dictionary<Type, List<IServiceUser>>();
        private bool _locked;

        public void AddNew<T>(IIsAService newService)
        {
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

        public T Get<T>(IServiceUser ieServUser)
        {
            if (!_services.ContainsKey(typeof(T)))
            {
                if (!_waitingForServicesStore.ContainsKey(typeof(T)))
                {
                    _waitingForServicesStore.Add(typeof(T), new List<IServiceUser>());
                }
            
                _waitingForServicesStore[typeof(T)].Add(ieServUser);
            }
            return (T) _services[typeof(T)];
        }
        
        public T Get<T>() => (T) _services[typeof(T)];

        public EZServiceMaster Unlock()
        {
            _locked = false;
            return this;
        }
    
        /// <summary> Makes sure na new scene has no active services </summary>
        private void RemoveService<T>()
        {
            if (_services.ContainsKey(typeof(T)))
            {
                var temp = (IIsAService) _services[typeof(T)];
                temp.OnRemoveService();
                _services.Remove(typeof(T));
            }
        }
    
        private void CheckForWaitingServiceUser(Type type)
        {
            if(_waitingForServicesStore.Count == 0 || !_waitingForServicesStore.ContainsKey(type)) return;
        
            foreach (var service in _waitingForServicesStore[type])
            {
                service.UseEZServiceLocator();
            }
            _waitingForServicesStore.Remove(type);
        }

        public void CleanUpServices() => _services.Clear();
    }
}