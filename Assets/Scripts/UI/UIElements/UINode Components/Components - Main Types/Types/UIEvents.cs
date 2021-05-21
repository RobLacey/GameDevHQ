using UnityEngine.Events;

public class UIEvents : NodeFunctionBase
{
    private readonly IEventSettings _eventSettings;

    public UIEvents(IEventSettings settings, IUiEvents uiEvents) : base(uiEvents)
    {
        _eventSettings = settings;
        CanActivate = true;
    }

    private UnityEvent OnEnterEvent => _eventSettings.OnEnterEvent;
    private UnityEvent OnExitEvent => _eventSettings.OnExitEvent;
    private UnityEvent OnButtonClickedEvent => _eventSettings.OnButtonClickEvent;
    private OnDisabledEvent OnDisableEvent => _eventSettings.DisableEvent;
    private OnToggleEvent OnToggleEvent => _eventSettings.ToggleEvent;

    //Properties
    protected override bool CanBeHighlighted() => true;
    protected override bool CanBePressed() => true;
    protected override bool FunctionNotActive() => !CanActivate;
    
    protected override void SavePointerStatus(bool pointerOver)
    {
        if (FunctionNotActive()) return;

        if (pointerOver)
        {
            OnEnterEvent?.Invoke();
        }
        else
        {
            OnExitEvent?.Invoke();
        }
    }

    private protected override void ProcessPress()
    {
        if (FunctionNotActive()) return;
        OnButtonClickedEvent?.Invoke();
        OnToggleEvent?.Invoke(_isSelected);
    }

    private protected override void ProcessDisabled()
    {
        if(FunctionNotActive()) return;
        OnDisableEvent?.Invoke(_isDisabled);
    }
}
