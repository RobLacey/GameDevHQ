
using UnityEngine;

public abstract class NodeFunctionBase
{
    private bool _pointerOver;
    //protected bool _isPressed;
    protected bool _isSelected;
    protected Setting _enabledFunctions;
    UIControlsEvents _uiControlsEvents = new UIControlsEvents();

    //Properties
    protected virtual bool CanActivate { get; set; }
    protected abstract bool CanBeSelected();
    protected abstract bool CanBeHighlighted();
    protected abstract bool CanBePressed();
    protected abstract bool FunctionNotActive();
    protected virtual void SavePointerStatus(bool pointerOver) => _pointerOver = pointerOver;
    private void OnChangeControls() => _pointerOver = false;

    public virtual void OnAwake(UINode node, UiActions uiActions)
    {
        uiActions._whenPointerOver += SavePointerStatus;
        uiActions._isHighlighted += SaveHighlighted;
        uiActions._isSelected += SaveIsSelected;
        uiActions._isPressed += SaveIsPressed;
        uiActions._isDisabled += IsDisabled;
        _uiControlsEvents.SubscribeOnChangeControls(OnChangeControls);
        _enabledFunctions = node.ActiveFunctions;
    }

    public void OnDisable(UiActions uiActions)
    {
        uiActions._whenPointerOver -= SavePointerStatus;
        uiActions._isHighlighted -= SaveHighlighted;
        uiActions._isSelected -= SaveIsSelected;
        uiActions._isPressed -= SaveIsPressed;
        uiActions._isDisabled -= IsDisabled;
    }

    private void SaveIsSelected(bool isSelected)
    {
        if (FunctionNotActive()) return;
        _isSelected = isSelected;
        ProcessFunctionActivation(_isSelected);
    }
    
    private void SaveHighlighted(bool isHighlighted)
    {
        if (FunctionNotActive()) return;
        ProcessFunctionActivation(isHighlighted);
    }
    
    private void SaveIsPressed(/*bool pressed*/)
    {
        if (FunctionNotActive()) return;
        if (!CanBePressed()) return;
        ProcessPress();
    }

    private void IsDisabled(bool isDisabled)
    {
        if (FunctionNotActive()) return;
        ProcessDisabled(isDisabled);
    }
    
    private void ProcessFunctionActivation(bool activate)
    {
        if (_pointerOver || activate)
        {
            SetActive();
        }
        else
        {
            SetNotActive();
        }
    }

    private void SetActive()
    {
        if (CanBeSelected() && _isSelected)
        {
            ProcessSelectedAndHighLighted();
        }
        else if(CanBeHighlighted())
        {
            ProcessHighlighted();
        }
        else
        {
            ProcessToNormal();
        }
    }

    private void SetNotActive()
    {
        if (CanBeSelected() && _isSelected)
        {
            ProcessSelected();
        }
        else
        {
            ProcessToNormal();
        }
    }

    private protected abstract void ProcessSelectedAndHighLighted();
    private protected abstract void ProcessHighlighted();
    private protected abstract void ProcessSelected();
    private protected abstract void ProcessToNormal();
    private protected abstract void ProcessPress();
    private protected abstract void ProcessDisabled(bool isDisabled);
}
