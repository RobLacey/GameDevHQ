
using UnityEngine;

public abstract class NodeFunctionBase
{
    private bool _pointerOver;
    protected bool _isSelected;
    protected Setting _enabledFunctions;

    //Properties
    protected bool CanActivate { get; set; }
    protected abstract bool CanBeHighlighted();
    protected abstract bool CanBePressed();
    protected abstract bool FunctionNotActive(); //TODO Review and fix as may not be needed

    
    public virtual void OnAwake(UINode node, UiActions uiActions)
    {
        uiActions._whenPointerOver += SavePointerStatus;
        uiActions._isSelected += SaveIsSelected;
        uiActions._isPressed += ProcessPress;
        uiActions._isDisabled += IsDisabled;
        _enabledFunctions = node.ActiveFunctions;
    }

    public void OnDisable(UiActions uiActions)
    {
        uiActions._whenPointerOver -= SavePointerStatus;
        uiActions._isSelected -= SaveIsSelected;
        uiActions._isPressed -= ProcessPress;
        uiActions._isDisabled -= IsDisabled;
    }
    protected abstract void SavePointerStatus(bool pointerOver);

    private void SaveIsSelected(bool isSelected)
    {
        if (FunctionNotActive()) return;
        _isSelected = isSelected;
    }
    
    // private void SaveIsPressed()
    // {
    //     if (FunctionNotActive()) return;
    //     ProcessPress();
    // }

    private void IsDisabled(bool isDisabled)
    {
        if (FunctionNotActive()) return;
        ProcessDisabled(isDisabled);
    }
    
     private protected abstract void ProcessPress();
     
     private protected abstract void ProcessDisabled(bool isDisabled);
}
