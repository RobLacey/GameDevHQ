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
    }

    //Variables
    private readonly UINode _up, _down, _left, _right;
    private IBranch _myBranch;
    private INode _myNode;
    private readonly INavigationSettings _mySettings;
    private InputScheme _inputScheme;

    //Properties
    private IBranch ChildBranch => _mySettings.ChildBranch;
    private NavigationType SetNavigation => _mySettings.NavType;
    private int NodeGroupSize => _myBranch.ThisGroupsUiNodes.Length;
    protected override bool CanBeHighlighted() => false;
    protected override bool CanBePressed() => !(ChildBranch is null);
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

        DoPresetMove(moveDirection);
    }

    private void DoPresetMove(MoveDirection moveDirection)
    {
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
        if (NodeGroupSize <= 1) return false;

        var index = Array.IndexOf(_myBranch.ThisGroupsUiNodes, _myBranch.LastHighlighted);

        switch (moveDirection)
        {
            case MoveDirection.Down when SetNavigation == NavigationType.AutoUpDown:
                CheckMoveDirection(PositiveInteract, moveDirection, index);
                return true;
            case MoveDirection.Up when SetNavigation == NavigationType.AutoUpDown:
                CheckMoveDirection(NegativeIterate, moveDirection, index);
                return true;
            case MoveDirection.Left when SetNavigation == NavigationType.AutoRightLeft:
                CheckMoveDirection(NegativeIterate, moveDirection, index);
                return true;
            case MoveDirection.Right when SetNavigation == NavigationType.AutoRightLeft:
                CheckMoveDirection(PositiveInteract, moveDirection, index);
                return true;
            case MoveDirection.None:
                return false;
            default:
                return false;
        } 
    }

    private int PositiveInteract(int newIndex) => newIndex.PositiveIterate(NodeGroupSize);
    
    private int NegativeIterate(int newIndex) => newIndex.NegativeIterate(NodeGroupSize);

    private void CheckMoveDirection(Func<int,int> iterateMethod, MoveDirection moveDirection, int index)
    {
        _myBranch.ThisGroupsUiNodes[iterateMethod.Invoke(index)].DoNonMouseMove(moveDirection);
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
