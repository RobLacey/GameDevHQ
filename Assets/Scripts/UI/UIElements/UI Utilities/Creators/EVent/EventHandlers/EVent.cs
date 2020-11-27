using System;
using UnityEngine;

public class EVent : EVentBaseClass<EVentBindings, EVent>
{
    public override Action<T> FetchEVent<T>() => EVentMaster.Get<T>();

    public override void Subscribe<T>(Action<T> listener) => EVentMaster.Subscribe(listener);

    public override void Unsubscribe<T>(Action<T> listener) => EVentMaster.Unsubscribe(listener);
}