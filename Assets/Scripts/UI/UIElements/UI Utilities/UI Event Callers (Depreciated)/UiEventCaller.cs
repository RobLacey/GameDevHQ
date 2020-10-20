using UnityEngine;

public abstract class UiEventCaller
{
    //TODo Add ability to handle scene changes
    protected UiEventCaller()
    {
        Application.quitting += OnExit;
    }

    protected abstract void OnExit();
}