using System;
using UnityEngine;
using UnityEngine.Events;
using NaughtyAttributes;

[Serializable]
public class UIEvents : NodeFunctionBase
{
    [Header("Highlight Events")]
    [HorizontalLine(4, color: EColor.Blue, order = 1)]
    
    [SerializeField] private UnityEvent _onEnterEvent;
    [SerializeField] private UnityEvent _onExitEvent;
    
    [Header("Click/Selected Events")]
    [HorizontalLine(4, color: EColor.Blue, order = 1)]
    [SerializeField] private UnityEvent _onButtonClickEvent;
    [SerializeField] private OnDisabledEvent _onDisable;
    [SerializeField] private OnToggleEvent _onToggleEvent;

    //Custom Unity Events
    [Serializable]
    public class OnToggleEvent : UnityEvent<bool> { }
    [Serializable]
    public class OnDisabledEvent : UnityEvent<bool> { }
    
    //Properties
    protected override bool CanBeHighlighted() => true;
    protected override bool CanBePressed() => true;
    protected override bool FunctionNotActive() => !CanActivate;
    
    public override void OnAwake(UiActions uiActions, Setting activeFunctions)
    {
        base.OnAwake(uiActions, activeFunctions);
        CanActivate = (_enabledFunctions & Setting.Events) != 0;
    }

    protected override void SavePointerStatus(bool pointerOver)
    {
        if (FunctionNotActive()) return;

        if (pointerOver)
        {
            _onEnterEvent?.Invoke();
        }
        else
        {
            _onExitEvent?.Invoke();
        }
    }

    private protected override void ProcessPress()
    {
        if (FunctionNotActive()) return;
        _onButtonClickEvent?.Invoke();
        _onToggleEvent?.Invoke(_isSelected);
    }

    private protected override void ProcessDisabled()
    {
        if(FunctionNotActive()) return;
        _onDisable?.Invoke(_isDisabled);
    }
}
