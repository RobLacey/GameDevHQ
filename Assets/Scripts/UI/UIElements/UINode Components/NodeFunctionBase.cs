using UnityEngine.EventSystems;

public abstract class NodeFunctionBase
{
    protected bool _pointerOver, _isSelected, _isDisabled;
    protected Setting _enabledFunctions;
    protected MoveDirection _moveDirection;

    //Properties
    protected bool CanActivate { get; set; }
    private void AxisMoveDirection(MoveDirection moveDirection) => _moveDirection = moveDirection;
    protected abstract bool CanBeHighlighted();
    protected abstract bool CanBePressed();
    protected abstract bool FunctionNotActive(); //TODO Review and fix as may not be needed
    
    public virtual void OnAwake(UiActions uiActions, Setting activeFunctions)
    {
        uiActions._whenPointerOver += SavePointerStatus;
        uiActions._isSelected += SaveIsSelected;
        uiActions._isPressed += ProcessPress;
        uiActions._isDisabled += IsDisabled;
        uiActions._onMove += AxisMoveDirection;
        _enabledFunctions = activeFunctions;
    }

    public void OnDisable(UiActions uiActions)
    {
        uiActions._whenPointerOver -= SavePointerStatus;
        uiActions._isSelected -= SaveIsSelected;
        uiActions._isPressed -= ProcessPress;
        uiActions._isDisabled -= IsDisabled;
        uiActions._onMove -= AxisMoveDirection;
    }
    protected abstract void SavePointerStatus(bool pointerOver);

    private void SaveIsSelected(bool isSelected)
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
