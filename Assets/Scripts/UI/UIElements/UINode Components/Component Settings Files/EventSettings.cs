using System;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;

public interface IEventSettings : IComponentSettings
{
    UnityEvent OnEnterEvent { get; set; }
    UnityEvent OnExitEvent { get; set; }
    UnityEvent OnButtonClickEvent { get; set; }
    OnToggleEvent ToggleEvent { get; set; }
    OnDisabledEvent DisableEvent { get; set; }
}

[Serializable]
public class EventSettings : IEventSettings
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
    
    public UnityEvent OnEnterEvent
    {
        get => _highlightEvents._onEnterEvent;
        set => _highlightEvents._onEnterEvent = value;
    }

    public UnityEvent OnExitEvent
    {
        get => _highlightEvents._onExitEvent;
        set => _highlightEvents._onExitEvent = value;
    }

    public UnityEvent OnButtonClickEvent
    {
        get => _selectClickEvents._onButtonClickEvent;
        set => _selectClickEvents._onButtonClickEvent = value;
    }

    public OnToggleEvent ToggleEvent
    {
        get => _selectClickEvents._onToggleEvent;
        set => _selectClickEvents._onToggleEvent = value;
    }

    public OnDisabledEvent DisableEvent
    {
        get => _selectClickEvents._onDisable;
        set => _selectClickEvents._onDisable = value;
    }

    public NodeFunctionBase SetUp(IUiEvents uiNodeEvents, Setting functions)
    {
        if ((functions & Setting.Events) != 0)
        {
            uiNodeEvents.ReturnMasterNode.MyRunTimeSetter.SetEvents(this);
            return new UIEvents(this, uiNodeEvents);
        }
        return new NullFunction();
    }
}

//Custom Unity Events
[Serializable]
public class OnToggleEvent : UnityEvent<bool> { }
[Serializable]
public class OnDisabledEvent : UnityEvent<bool> { }

