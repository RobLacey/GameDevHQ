using System;
using System.Collections;

public interface IEVentBase
{
    Hashtable EVentsList { get; set; }
    Action<T> Fetch<T>();
    void Subscribe<TType>(Action<TType> listener);
    void Unsubscribe<T>(Action<T> listener);
}

public abstract class EVentBaseClass<TEVent> : IEVentBase where TEVent : new()
{
    public static TEVent Do { get; } = new TEVent();
    public abstract Hashtable EVentsList { get; set; }

    public abstract Action<T> Fetch<T>();
    public abstract void Subscribe<TType>(Action<TType> listener);
    public abstract void Unsubscribe<T>(Action<T> listener);
}