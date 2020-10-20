using System;

public class CustomEventHandler
{
    private Action _callNoParameter;
    
    public Action Add(Action subscriber)
    {
        _callNoParameter = subscriber;
        return Event;
    }

    public void Event() => _callNoParameter?.Invoke();
}

public class CustomEventHandler<T>
{
    private Action<T> _call;

    public Action<T> Add(Action<T> subscriber)
    {
        _call = subscriber;
        return Event;
    }
    
    public void Event(T variable) => _call?.Invoke(variable);
}

public class CustomReturnEventHandler<T>
{
    private Func<T> _call;

    public Func<T> Add(Func<T> subscriber)
    {
        _call = subscriber;
        return Event;
    }

    public T Event()
    {
        var T = _call.Invoke();
        return T;
    }
}

public class CustomEventHandler<T, TU>
{
    private Action<T, TU> _call;
    private Action _callNoParameter;

    public Action<T,TU> Add(Action<T, TU> subscriber)
    {
        _call = subscriber;
        return Event;
    }
    
    public void Event(T variable, TU variable2) => _call?.Invoke(variable, variable2);
}