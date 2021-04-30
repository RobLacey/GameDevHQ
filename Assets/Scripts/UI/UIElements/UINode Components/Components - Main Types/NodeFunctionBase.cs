using UnityEngine;
using UnityEngine.EventSystems;

public abstract class NodeFunctionBase : IEventUser
{
    protected bool _pointerOver, _isSelected, _isDisabled;
    protected IUiEvents _uiEvents;

    //Properties
    protected bool CanActivate { get; set; }
    protected virtual void AxisMoveDirection(MoveDirection moveDirection) { }
    protected abstract bool CanBeHighlighted();
    protected abstract bool CanBePressed();
    protected abstract bool FunctionNotActive();
    
    protected virtual void OnAwake(IUiEvents events)
    {
        _uiEvents = events;
        _uiEvents.WhenPointerOver += SavePointerStatus;
        _uiEvents.IsSelected += SaveIsSelected;
        _uiEvents.IsPressed += ProcessPress;
        _uiEvents.IsDisabled += IsDisabled;
        _uiEvents.OnMove += AxisMoveDirection;
    }

    public virtual void OnEnable()
    {
        ObserveEvents();
    }

    public virtual void OnDisable()
    {
        if(_uiEvents is null) return;
        _uiEvents.WhenPointerOver -= SavePointerStatus;
        _uiEvents.IsSelected -= SaveIsSelected;
        _uiEvents.IsPressed -= ProcessPress;
        _uiEvents.IsDisabled -= IsDisabled;
        _uiEvents.OnMove -= AxisMoveDirection;
    }

    public virtual void ObserveEvents() { }

    public virtual void Start() { }

    protected abstract void SavePointerStatus(bool pointerOver);

    protected virtual void SaveIsSelected(bool isSelected)
    {
        if (FunctionNotActive()) return;
        _isSelected = isSelected;
    }

    private void IsDisabled(bool isDisabled)
    {
        _isDisabled = isDisabled;
        ProcessDisabled();
    }
    
     private protected abstract void ProcessPress();
     
     private protected virtual void ProcessDisabled() { }
}
