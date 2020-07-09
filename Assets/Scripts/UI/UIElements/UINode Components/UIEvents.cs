using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using NaughtyAttributes;

[System.Serializable]
public class UIEvents
{
    [Header("Highlight Events")]
    [HorizontalLine(4, color: EColor.Blue, order = 1)]
    public UnityEvent OnEnterEvent;
    public UnityEvent OnExitEvent;
    [Header("Click/Selected Events")]
    [HorizontalLine(4, color: EColor.Blue, order = 1)]
    public UnityEvent _OnButtonClickEvent;
    public OnToggleEvent _OnToggleEvent;

    public bool CanActivate { get; private set; }

    public void OnAwake(Setting setting)
    {
        CanActivate = (setting & Setting.Events) != 0;
    }

    [System.Serializable]
    public class OnToggleEvent : UnityEvent<bool> { }

}
