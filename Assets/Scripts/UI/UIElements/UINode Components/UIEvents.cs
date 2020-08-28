using System;
using UnityEngine;
using UnityEngine.Events;
using NaughtyAttributes;

[Serializable]
public class UIEvents : NodeFunctionBase
{
    [Header("Highlight Events")]
    [HorizontalLine(4, color: EColor.Blue, order = 1)]
    
    public UnityEvent _onEnterEvent;
    public UnityEvent _onExitEvent;
    
    [Header("Click/Selected Events")]
    [HorizontalLine(4, color: EColor.Blue, order = 1)]
    
    public UnityEvent _onButtonClickEvent;
    public OnDisabledEvent _onDisable;
    public OnToggleEvent _onToggleEvent;

    //Custom Unity Events
    [Serializable]
    public class OnToggleEvent : UnityEvent<bool> { }
    [Serializable]
    public class OnDisabledEvent : UnityEvent<bool> { }
    
    //Properties
    protected override bool CanBeSelected() => true;
    protected override bool CanBeHighlighted() => true;
    protected override bool CanBePressed() => true;
    protected override bool FunctionNotActive() => !CanActivate;
    
    public override void OnAwake(UINode node, UiActions uiActions)
    {
        base.OnAwake(node, uiActions);
        CanActivate = (_enabledFunctions & Setting.Events) != 0;
    }

    protected override void SavePointerStatus(bool pointerOver)
    {
        if (pointerOver)
        {
            _onEnterEvent?.Invoke();
        }
        else
        {
            _onExitEvent?.Invoke();
        }
    }

    private protected override void ProcessSelectedAndHighLighted() { }

    private protected override void ProcessHighlighted() { }

    private protected override void ProcessSelected() { }

    private protected override void ProcessToNormal() { }

    private protected override void ProcessPress()
    {
        _onButtonClickEvent?.Invoke();
        _onToggleEvent?.Invoke(_isSelected);
    }

    private protected override void ProcessDisabled(bool isDisabled) => _onDisable?.Invoke(isDisabled);
}
