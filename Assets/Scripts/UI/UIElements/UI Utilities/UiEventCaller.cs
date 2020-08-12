using UnityEngine;

public abstract class UiEventCaller
{
    protected UiEventCaller()
    {
        Application.quitting += OnExit;
    }

    protected abstract void OnExit();
}