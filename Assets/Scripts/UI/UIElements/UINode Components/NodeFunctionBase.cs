
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class NodeFunctionBase
{
    protected bool _pointerOver;
    protected bool _isSelected;
    protected Setting _enabledFunctions;
    protected MoveDirection _moveDirection;

    //Properties
    protected bool CanActivate { get; set; }
    protected void axisMoveDirection(MoveDirection moveDirection) => _moveDirection = moveDirection;
    protected abstract bool CanBeHighlighted();
    protected abstract bool CanBePressed();
    protected abstract bool FunctionNotActive(); //TODO Review and fix as may not be needed
    
    public virtual void OnAwake(UINode node, UiActions uiActions)
    {
        uiActions._whenPointerOver += SavePointerStatus;
        uiActions._isSelected += SaveIsSelected;
        uiActions._isPressed += ProcessPress;
        uiActions._isDisabled += IsDisabled;
        uiActions._onMove += axisMoveDirection;
        _enabledFunctions = node.ActiveFunctions;
    }

    public void OnDisable(UiActions uiActions)
    {
        uiActions._whenPointerOver -= SavePointerStatus;
        uiActions._isSelected -= SaveIsSelected;
        uiActions._isPressed -= ProcessPress;
        uiActions._isDisabled -= IsDisabled;
        uiActions._onMove -= axisMoveDirection;
    }
    protected abstract void SavePointerStatus(bool pointerOver);

    private void SaveIsSelected(bool isSelected)
    {
        if (FunctionNotActive()) return;
        _isSelected = isSelected;
    }

    private void IsDisabled(bool isDisabled)
    {
        ProcessDisabled(isDisabled);
    }
    
     private protected abstract void ProcessPress();
     
     private protected abstract void ProcessDisabled(bool isDisabled);
}
