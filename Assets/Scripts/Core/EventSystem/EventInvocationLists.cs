using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

[CreateAssetMenu(fileName = "EventInnList", menuName = "Event Innvocation List")]

public class EventInvocationLists : ScriptableObject
{
    [SerializeField] List<NewEntry> _listenerList;
    [SerializeField] List<NewEntry> _invokerList;
    [SerializeField] bool _resetListenerListOnPlay = false;
    [SerializeField] bool _resetInvokerListOnPlay = false;

    [Serializable]
    public class NewEntry
    {
        [HideInInspector] public string name;
        [HideInInspector] public EventManager eventManager;
        public List<string> methods;
    }

    private void OnEnable()
    {
        if (_resetListenerListOnPlay)
        {
            _listenerList = new List<NewEntry>();
        }  
        
        if (_resetInvokerListOnPlay)
        {
            _invokerList = new List<NewEntry>();
        }    
    }

    public void AddToListenerList (string newMethod, EventManager newEventManager)
    {
        if (_listenerList.Any(n => n.eventManager == newEventManager))
        {
            foreach (var item in _listenerList)
            {
                if (item.eventManager == newEventManager &&  !item.methods.Contains(newMethod))
                {
                    item.methods.Add(newMethod);
                    break;
                }
            }
        }
        else
        {
            _listenerList.Add(new NewEntry{ name = newEventManager.name,
                                               eventManager = newEventManager,
                                               methods = new List<string> { newMethod }});
        }
    }

    public void AddToInvokerList (string newMethod, EventManager newEventManager)
    {
        if (_invokerList.Any(n => n.eventManager == newEventManager))
        {
            foreach (var item in _invokerList)
            {
                if (item.eventManager == newEventManager &&  !item.methods.Contains(newMethod))
                {
                    item.methods.Add(newMethod);
                    break;
                }
            }
        }
        else
        {
            _invokerList.Add(new NewEntry{ name = newEventManager.name,
                                               eventManager = newEventManager,
                                               methods = new List<string> { newMethod }});
        }
    }

    public List<string> ReturnListenerList(EventManager newEventManager)
    {
        if (_listenerList.Count > 0)
        {
            return _listenerList.First(n => n.eventManager == newEventManager).methods;
        }
        return null;
    }

    public List<string> ReturnInvokerList(EventManager newEventManager)
    {
        if (_invokerList.Count > 0)
        {
            var result = _invokerList.FirstOrDefault(n => n.eventManager == newEventManager);

            if (result != null)
            {
                return result.methods;
            }
        }
        return null;
    }

    public void SortLists()
    {
        _listenerList = _listenerList.OrderBy(n => n.name).ToList();
        _invokerList = _invokerList.OrderBy(n => n.name).ToList();
    }
}
