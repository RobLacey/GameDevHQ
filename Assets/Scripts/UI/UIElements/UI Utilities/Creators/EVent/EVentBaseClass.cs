using System;

public abstract class EVentBaseClass<TBind, TEVent> : IEVent 
    where TBind : new() 
    where TEVent : new()
{
    public static TEVent Do { get; } = new TEVent();
    protected static TBind Bindings { get; set; } = new TBind();
    
    public abstract Action<T> FetchEVent<T>();
    public abstract void Subscribe<TType>(Action<TType> listener);
    public abstract void Unsubscribe<T>(Action<T> listener);
}