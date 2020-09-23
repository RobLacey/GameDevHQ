using NaughtyAttributes;
using UnityEngine;
using UnityEngine.EventSystems;
using System;

[Serializable]
public class UINavigation : NodeFunctionBase
{
    [SerializeField] 
    [AllowNesting] [Label("Move To When Clicked")] [HideIf("CantNavigate")] private UIBranch _childBranch;
    [SerializeField] 
    private NavigationType _setNavigation = NavigationType.UpAndDown;
    [SerializeField] 
    [AllowNesting] [ShowIf("UpDownNav")] private UINode _up;
    [SerializeField] 
    [AllowNesting] [ShowIf("UpDownNav")] private UINode _down;
    [SerializeField] 
    [AllowNesting] [ShowIf("RightLeftNav")] private UINode _left;
    [SerializeField] 
    [AllowNesting] [ShowIf("RightLeftNav")] private UINode _right;

    //Editor Scripts
    public bool CantNavigate { get; set; }

    public bool UpDownNav() 
        => _setNavigation == NavigationType.UpAndDown || _setNavigation == NavigationType.AllDirections;

    public bool RightLeftNav() 
        => _setNavigation == NavigationType.RightAndLeft || _setNavigation == NavigationType.AllDirections;

    //Variables
    private UIBranch _myBranch;
    private UIBranch _activeBranch;
    private UIDataEvents _uiDataEvents = new UIDataEvents();

    //Events
    public static Action<(UIBranch moveFrom, UIBranch moveToo)> onMoveToBranch;
    public static Action<UIBranch> onTurnOffChildBranches;

    //Properties
    protected override bool CanBeHighlighted() => false;
    private void ActiveBranch(UIBranch newBranch) => _activeBranch = newBranch;
    protected override bool CanBePressed() => !(_childBranch is null);
    protected override bool FunctionNotActive() => !CanActivate;
    protected override void SavePointerStatus(bool pointerOver) { }
    public UIBranch Child
    {
        get => _childBranch;
        set => _childBranch = value;
    }

    public void OnAwake(UiActions uiActions, Setting activeFunctions, UIBranch myBranch)
    {
        base.OnAwake(uiActions, activeFunctions);
        CanActivate = (_enabledFunctions & Setting.NavigationAndOnClick) != 0;
        _myBranch = myBranch;
        _uiDataEvents.SubscribeToActiveBranch(ActiveBranch);
    }
    
    public void HandleAsSlider()
    {
        if (_moveDirection == MoveDirection.Left || _moveDirection == MoveDirection.Right)
        {
            Debug.Log("Add Audio");
            //_myNode.Audio.Play(UIEventTypes.Selected);
        }
    }

    public void ProcessMoves()
    {
        if (FunctionNotActive() || _setNavigation == NavigationType.None) return;

        switch (_moveDirection)
        {
            case MoveDirection.Down when _down:
            {
                HandleMove(_down);
                break;
            }
            case MoveDirection.Up when _up:
            {
                HandleMove(_up);
                break;
            }
            case MoveDirection.Left when _left:
            {
                HandleMove(_left);
                break;
            }
            case MoveDirection.Right when _right:
            {
                HandleMove(_right);
                break;
            }
        }
    }

    private void HandleMove(UINode moveTo) => moveTo.CheckIfMoveAllowed(_moveDirection);

    private protected override void ProcessPress()
    {
        if(FunctionNotActive() || !CanBePressed() || _childBranch is null) return;
        
        if (_isSelected)
        {
            onMoveToBranch?.Invoke((_myBranch, _childBranch));
        }
        else
        {
            onTurnOffChildBranches?.Invoke(_childBranch);
        }
    }

    private protected override void ProcessDisabled()
    {
        if(FunctionNotActive()) return;
        
        if (_activeBranch.MyParentBranch == _myBranch)
        {
            _myBranch.MoveToThisBranch();
        }
        onTurnOffChildBranches?.Invoke(_childBranch);
    }

    public void MoveToNextFreeNode()
    {
        UINode nextFree = ReturnNextFreeMoveTarget();
        if(nextFree is null) return;
        EventSystem.current.SetSelectedGameObject(nextFree.gameObject);
        nextFree.SetNodeAsActive();
    }

    private UINode ReturnNextFreeMoveTarget()
    {
        if (_setNavigation == NavigationType.UpAndDown || _setNavigation == NavigationType.AllDirections)
        {
            return _down ? _down : _up;
        }

        if (_setNavigation == NavigationType.RightAndLeft || _setNavigation == NavigationType.AllDirections)
        {
            return _right ? _right : _left;
        }
        return null;
    }
}
