using System;

public interface IEVent : IEVentBase { }

public class EVent : EVentBaseClass<EVent>, IEVent
{
    public override Action<T> Fetch<T>() => EVentMaster.Get<T>(EVentsList);
    public override void Return<T>(T obj) => EVentMaster.Get<T>(EVentsList)?.Invoke(obj);

    public override void Subscribe<T>(Action<T> listener) => EVentMaster.Subscribe(listener, EVentsList);

    public override void Unsubscribe<T>(Action<T> listener) => EVentMaster.Unsubscribe(listener, EVentsList);
}