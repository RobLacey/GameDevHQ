using System;

public interface IEVent
{
    Action<T> FetchEVent<T>();
    void Subscribe<TType>(Action<TType> listener);
    void Unsubscribe<T>(Action<T> listener);
}