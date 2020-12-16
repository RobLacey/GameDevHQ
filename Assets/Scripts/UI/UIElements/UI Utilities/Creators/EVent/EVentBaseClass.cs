using System;
using System.Collections;

public abstract class EVentBaseClass<TEVent> : ISetNewEVent where TEVent : new()
{
    public static TEVent Do { get; private set; }

    public void SetUpEVentHandler(IEVentBase eVentBase, Hashtable eventList)
    {
        Do = (TEVent) eVentBase;
        EVentsList = eventList;
    }

    protected Hashtable EVentsList { get; set; }
    
    public abstract Action<T> Fetch<T>();
    public abstract void Subscribe<TType>(Action<TType> listener);
    public abstract void Unsubscribe<T>(Action<T> listener);
}