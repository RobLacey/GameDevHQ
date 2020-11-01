using System;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;

public interface IEventSetttings : IComponentSettings
{
    UnityEvent OnEnterEvent { get; }
    UnityEvent OnExitEvent { get; }
    UnityEvent OnButtonClickEvent { get; }
    OnToggleEvent ToggleEvent { get; }
    OnDisabledEvent DisableEvent { get; }
}

[Serializable]
public class EventSettings : IEventSetttings
{
    [Header("Highlight Events")] [HorizontalLine(4, color: EColor.Blue, order = 1)] 
    [SerializeField] private HighlightEvents _highlightEvents;

    [Header("Click/Selected Events")] [HorizontalLine(4, color: EColor.Blue, order = 1)] 
    [SerializeField] private SelectClickEvents _selectClickEvents;

    [Serializable]
    private class HighlightEvents
    {
        public UnityEvent _onEnterEvent;
        public UnityEvent _onExitEvent;

    }
    
    [Serializable]
    private class SelectClickEvents
    {
        public UnityEvent _onButtonClickEvent;
        public OnDisabledEvent _onDisable;
        public OnToggleEvent _onToggleEvent;
    }
    public UnityEvent OnEnterEvent => _highlightEvents._onEnterEvent;
    public UnityEvent OnExitEvent => _highlightEvents._onExitEvent;
    public UnityEvent OnButtonClickEvent => _selectClickEvents._onButtonClickEvent;
    public OnToggleEvent ToggleEvent => _selectClickEvents._onToggleEvent;
    public OnDisabledEvent DisableEvent => _selectClickEvents._onDisable;
    
    public NodeFunctionBase SetUp(UiActions uiActions, Setting functions)
    {
        if ((functions & Setting.Events) != 0)
        {
            return new UIEvents(this, uiActions);
        }
        return new NullFunction();
    }
}

//Custom Unity Events
[Serializable]
public class OnToggleEvent : UnityEvent<bool> { }
[Serializable]
public class OnDisabledEvent : UnityEvent<bool> { }

