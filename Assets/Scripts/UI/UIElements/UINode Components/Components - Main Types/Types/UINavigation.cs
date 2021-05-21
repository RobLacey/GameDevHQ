using System;
using EZ.Service;
using UnityEngine;
using UnityEngine.EventSystems;

public class UINavigation : NodeFunctionBase
{
    public UINavigation(INavigationSettings settings, IUiEvents uiEvents): base(uiEvents)
    {
        _mySettings = settings;
        _up = settings.Up;
        _down = settings.Down;
        _left = settings.Left;
        _right = settings.Right;
        CanActivate = true;
    }

    //Variables
    private readonly UINode _up;
    private readonly UINode _down;
    private readonly UINode _left;
    private readonly UINode _right;
    private IBranch _myBranch;
    private INode _myNode;
    private readonly INavigationSettings _mySettings;
    private InputScheme _inputScheme;

    //Properties
    private IBranch ChildBranch => _mySettings.ChildBranch;
    private NavigationType SetNavigation => _mySettings.NavType;
    protected override bool CanBeHighlighted() => false;
    protected override bool CanBePressed() => !(ChildBranch is null);
    protected override bool FunctionNotActive() => !CanActivate;
    protected override void SavePointerStatus(bool pointerOver) { }
    private bool MultiSelectAllowed => _inputScheme.MultiSelectPressed() &&
                                       _myNode.MultiSelectSettings.OpenChildBranch == IsActive.No
                                        && _myNode.MultiSelectSettings.AllowMultiSelect == IsActive.Yes ;

    //Main
    public override void OnAwake()
    {
        base.OnAwake();
        _myNode = _uiEvents.ReturnMasterNode;
        _myBranch = _myNode.MyBranch;
    }
    
    public override void UseEZServiceLocator()
    {
        base.UseEZServiceLocator();
        _inputScheme = EZService.Locator.Get<InputScheme>(this);
    }

    public override void OnDisable()
    {
        base.OnDisable();
        _inputScheme = null;
    }

    protected override void AxisMoveDirection(MoveDirection moveDirection)
    {
        base.AxisMoveDirection(moveDirection);
        ProcessMoves(moveDirection);
    }

    private void ProcessMoves(MoveDirection moveDirection)
    {
        if (FunctionNotActive() || SetNavigation == NavigationType.None) return;
        
        if (DoAutoMove(moveDirection)) return;

        switch (moveDirection)
        {
            case MoveDirection.Down when _down:
            {
                _down.DoNonMouseMove(moveDirection);
                break;
            }
            case MoveDirection.Up when _up:
            {
                _up.DoNonMouseMove(moveDirection);
                break;
            }
            case MoveDirection.Left when _left:
            {
                _left.DoNonMouseMove(moveDirection);
                break;
            }
            case MoveDirection.Right when _right:
            {
                _right.DoNonMouseMove(moveDirection);
                break;
            }
        }
    }

    private bool DoAutoMove(MoveDirection moveDirection)
    {
        Debug.Log("Make it show when moved onto branch and be able to remove nodes safely");
        if (SetNavigation != NavigationType.AutoUpDown 
            && SetNavigation != NavigationType.AutoRightLeft) return false;
        
        int index = Array.IndexOf(_myBranch.ThisGroupsUiNodes, _myBranch.LastHighlighted);
        int size = _myBranch.ThisGroupsUiNodes.Length;

        if(SetNavigation == NavigationType.AutoUpDown)
        {
            if (moveDirection == MoveDirection.Up)
                index = index.PositiveIterate(size);
            else if (moveDirection == MoveDirection.Down)
                index = index.NegativeIterate(size);
            _myBranch.ThisGroupsUiNodes[index].DoNonMouseMove(moveDirection);
        } 
        
        if(SetNavigation == NavigationType.AutoRightLeft)
        {
            if (moveDirection == MoveDirection.Right)
                index = index.PositiveIterate(size);
            else if (moveDirection == MoveDirection.Left)
                index = index.NegativeIterate(size);
        _myBranch.ThisGroupsUiNodes[index].DoNonMouseMove(moveDirection);
        }
        
        return true;
    }

    private protected override void ProcessPress()
    {
        if(FunctionNotActive() || !CanBePressed()) return;

        if (!_isSelected) return;
        NavigateToChildBranch(ChildBranch);
    }

    private void NavigateToChildBranch(IBranch moveToo)
    {
        if(MultiSelectAllowed) return;
        
        if (moveToo.IsInternalBranch())
        {
            ToChildBranchProcess();
        }
        else
        {
            _myBranch.StartBranchExitProcess(OutTweenType.MoveToChild, ToChildBranchProcess);
        }
        
        void ToChildBranchProcess() => moveToo.MoveToThisBranch(_myBranch);
    }
    
    public void MoveToNextFreeNode()
    {
        var nextFree = ReturnNextFreeMoveTarget();
        if(nextFree is null) return;
        EventSystem.current.SetSelectedGameObject(nextFree.gameObject);
        nextFree.SetNodeAsActive();
    }

    private UINode ReturnNextFreeMoveTarget()
    {
        if (SetNavigation == NavigationType.UpAndDown || SetNavigation == NavigationType.AllDirections)
        {
            return _down ? _down : _up;
        }

        if (SetNavigation == NavigationType.RightAndLeft || SetNavigation == NavigationType.AllDirections)
        {
            return _right ? _right : _left;
        }
        return null;
    }
}
