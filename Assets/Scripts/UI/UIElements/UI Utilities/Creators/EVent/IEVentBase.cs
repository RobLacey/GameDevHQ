using System;
using System.Collections;

public interface ISetNewEVent
{
    void SetUpEVentHandler(IEVentBase eVentBase, Hashtable eventList);
}

public interface IEVentBase
{
    Action<T> Fetch<T>();
    void Subscribe<TType>(Action<TType> listener);
    void Unsubscribe<T>(Action<T> listener);
}