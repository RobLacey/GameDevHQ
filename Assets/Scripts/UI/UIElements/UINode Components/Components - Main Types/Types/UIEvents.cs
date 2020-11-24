using UnityEngine;
using UnityEngine.Events;
using NaughtyAttributes;

public class UIEvents : NodeFunctionBase
{
    [Header("Highlight Events")]
    [HorizontalLine(4, color: EColor.Blue, order = 1)]
    
    private readonly UnityEvent _onEnterEvent;
    private readonly UnityEvent _onExitEvent;
    
    [Header("Click/Selected Events")]
    [HorizontalLine(4, color: EColor.Blue, order = 1)]
    private readonly UnityEvent _onButtonClickEvent;
    private readonly OnDisabledEvent _onDisable;
    private readonly OnToggleEvent _onToggleEvent;

    public UIEvents(IEventSetttings settings, IUiEvents uiEvents)
    {
        _onEnterEvent = settings.OnEnterEvent;
        _onExitEvent = settings.OnExitEvent;
        _onButtonClickEvent = settings.OnButtonClickEvent;
        _onDisable = settings.DisableEvent;
        _onToggleEvent = settings.ToggleEvent;
        CanActivate = true;
        OnAwake(uiEvents);
    }
    
    //Properties
    protected override bool CanBeHighlighted() => true;
    protected override bool CanBePressed() => true;
    protected override bool FunctionNotActive() => !CanActivate;
    
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
