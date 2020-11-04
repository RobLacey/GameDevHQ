using UnityEngine.EventSystems;

public abstract class NodeFunctionBase : IEventUser
{
    protected bool _pointerOver, _isSelected, _isDisabled;
    private UiActions _uiActions;

    //Properties
    protected bool CanActivate { get; set; }
    protected virtual void AxisMoveDirection(MoveDirection moveDirection) { }
    protected abstract bool CanBeHighlighted();
    protected abstract bool CanBePressed();
    protected abstract bool FunctionNotActive();
    
    protected virtual void OnAwake(UiActions uiActions)
    {
        _uiActions = uiActions;
        _uiActions._whenPointerOver += SavePointerStatus;
        _uiActions._isSelected += SaveIsSelected;
        _uiActions._isPressed += ProcessPress;
        _uiActions._isDisabled += IsDisabled;
        _uiActions._onMove += AxisMoveDirection;
        ObserveEvents();
    }
    
    public virtual void ObserveEvents() { }

    public virtual void RemoveFromEvents() { }


    public virtual void OnDisable()
    {
        if(_uiActions is null) return;
        _uiActions._whenPointerOver -= SavePointerStatus;
        _uiActions._isSelected -= SaveIsSelected;
        _uiActions._isPressed -= ProcessPress;
        _uiActions._isDisabled -= IsDisabled;
        _uiActions._onMove -= AxisMoveDirection;
    }
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
     
     private protected abstract void ProcessDisabled();
}
