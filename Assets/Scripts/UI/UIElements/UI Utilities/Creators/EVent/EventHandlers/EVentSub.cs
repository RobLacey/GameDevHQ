using System;

public class EVentSub : EVentBaseClass<EVentBindingsSub, EVentSub> 
{
    public override Action<T> FetchEVent<T>() => EVentMaster.Get<T>();

    public override void Subscribe<TType>(Action<TType> listener) => EVentMaster.Subscribe(listener);

    public override void Unsubscribe<T>(Action<T> listener) => EVentMaster.Unsubscribe(listener);
}