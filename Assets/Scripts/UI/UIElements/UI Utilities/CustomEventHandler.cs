using System;

public class CustomEventHandler<T>
{
    private Action<T> _call;
    private Action _callNoParameter;

    public Action<T> Add(Action<T> subscriber)
    {
        _call = subscriber;
        return Event;
    }
    
    public Action Add(Action subscriber)
    {
        _callNoParameter = subscriber;
        return Event;
    }

    public void Event(T variable) => _call?.Invoke(variable);
    public void Event() => _callNoParameter?.Invoke();
}