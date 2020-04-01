using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Event", menuName = "New Event")]

public class EventManager : ScriptableObject
{
    [SerializeField] List<string> _listenerlist = new List<string>();
    [SerializeField] List<string> _invokerlist = new List<string>();
    [SerializeField] EventInvocationLists _innvocationLists;
    [SerializeField] List<string> _parameterTypes = new List<string>();
    [SerializeField] string _returnTypes = "";
    [SerializeField] bool _justInvoke = false;
    [SerializeField] bool _resetTypeData = true;

    private Action newEvent;
    private Action<object> newEvent_Object;
    private Action<object, object> newEvent_Object_Object;
    public Func<object> newEvent_ReturnValue;
    public Func<object, object> newEvent_ReturnValue_Object;

    Action<string> EvaluateParameter;
    Action<string> EvaluateReturnParameter;

    private void OnEnable()
    {
        EvaluateParameter = delegate (string value) { _parameterTypes.Add(value); };
        EvaluateReturnParameter = delegate (string value) { _returnTypes = value; };

        if (_resetTypeData)
        {
            _listenerlist = new List<string>();
            _invokerlist = new List<string>();
            _parameterTypes = new List<string>();
            _returnTypes = "";
        }
    }

    //Add listener Methods
    public void AddListener(Action listener, MonoBehaviour sender)
    {
        newEvent += listener;
        if(_innvocationLists != null) _innvocationLists.AddToListenerList(sender.ToString(), this);
        AddToEventCleaner();
    }

    public void AddListener(Action<object> listener, MonoBehaviour sender)
    {
        newEvent_Object += listener;
        if (_innvocationLists != null) _innvocationLists.AddToListenerList(sender.ToString(), this);
        AddToEventCleaner();
    }

    public void AddListener(Action<object, object> listener, MonoBehaviour sender)
    {
        newEvent_Object_Object += listener;
        if (_innvocationLists != null) _innvocationLists.AddToListenerList(sender.ToString(), this);
        AddToEventCleaner();
    }

    public void AddReturnParameter(Func<object> listener, MonoBehaviour sender)
    {
        newEvent_ReturnValue += listener;
        if (_innvocationLists != null) _innvocationLists.AddToListenerList(sender.ToString(), this);
        AddToEventCleaner();
    }

    public void AddReturnParameter(Func<object, object> listener, MonoBehaviour sender)
    {
        newEvent_ReturnValue_Object += listener;
        if (_innvocationLists != null) _innvocationLists.AddToListenerList(sender.ToString(), this);
        AddToEventCleaner();
    }

    //Invoke Methods

    public void Invoke(MonoBehaviour invoker)
    {
        newEvent?.Invoke();

        _justInvoke = true;

        if (_innvocationLists != null && newEvent != null)
        {
            _innvocationLists.AddToInvokerList(invoker.ToString(), this);
        }
    }

    public void Invoke(object value, MonoBehaviour invoker)
    {
        newEvent_Object?.Invoke(value);

        if (_innvocationLists != null && newEvent_Object != null)
        {
            _innvocationLists.AddToInvokerList(invoker.ToString(), this);
            CollectData(invoker, value);
        }
    }

    public void Invoke(object value1, object value2, MonoBehaviour invoker)
    {
        newEvent_Object_Object?.Invoke(value1, value2);

        if (_innvocationLists != null && newEvent_Object_Object != null)
        {
            _innvocationLists.AddToInvokerList(invoker.ToString(), this);
            CollectData(invoker, value1, value2);
        }
    }

    public object ReturnParameter(MonoBehaviour invoker)
    {
        var temp = newEvent_ReturnValue?.Invoke();

        if (_innvocationLists != null && newEvent_ReturnValue != null)
        {
            _innvocationLists.AddToInvokerList(invoker.ToString(), this);
            CollectData(invoker, null, null, temp);
        }
        return temp;
    }

    public object ReturnParameter(object value, MonoBehaviour invoker)
    {
        var temp = newEvent_ReturnValue_Object?.Invoke(value);

        if (_innvocationLists != null && newEvent_ReturnValue_Object != null)
        {
            _innvocationLists.AddToInvokerList(invoker.ToString(), this);
            CollectData(invoker, value, null, temp);
        }
        return temp;
    }

    //Remove Listener Methods
    private void AddToEventCleaner()
    {
        EventCleaner eventCleaner = FindObjectOfType<EventCleaner>();
        eventCleaner.AddListenerToClear(this);
    }

    public void RemoveListeners()
    {
        if (_innvocationLists != null)
        {
            SetListenerList();
            SetInvokerList();
        }        
        newEvent = null;
        newEvent_ReturnValue = null;
        newEvent_Object = null;
        newEvent_Object_Object = null;
    }

    //Send to Innvocation Lists
    private void SetListenerList()
    {
        _listenerlist = _innvocationLists.ReturnListenerList(this);
    }

    public void SetInvokerList()
    {
        _invokerlist = _innvocationLists.ReturnInvokerList(this);
    }

    private void CollectData(MonoBehaviour invoker, object value1, object value2 = null, object return1 = null)
    {
        if (value1 != null && _parameterTypes.Count == 0)
        {
            ProcessString(value1.GetType().ToString(), EvaluateParameter);
        }

        if (value2 != null && _parameterTypes.Count == 1)
        {
            ProcessString(value2.GetType().ToString(), EvaluateParameter);
        }

        if (return1 != null && _returnTypes != return1.GetType().ToString())
        {
            ProcessString(return1.GetType().ToString(), EvaluateReturnParameter);
        }
        
    }

    private void ProcessString(string stringToTest, Action<string> SetFieldTo)
    {
        if (stringToTest == "System.Single")
        {
            SetFieldTo("Float");
        }
        else if (stringToTest == "System.Int32")
        {
            SetFieldTo("Int");
        }
        else if (stringToTest == "System.Boolean")
        {
            SetFieldTo("Boolean");
        }
        else
        {
            SetFieldTo(stringToTest);
        }
    }
}




