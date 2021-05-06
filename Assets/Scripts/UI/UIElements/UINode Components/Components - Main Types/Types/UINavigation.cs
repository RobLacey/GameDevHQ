using UnityEngine;
using UnityEngine.EventSystems;

public class UINavigation : NodeFunctionBase, IEServUser
{
    public UINavigation(INavigationSettings settings, IUiEvents uiEvents)
    {
        _mySettings = settings;
        //_childBranch = settings.ChildBranch;
        _setNavigation = settings.NavType;
        _up = settings.Up;
        _down = settings.Down;
        _left = settings.Left;
        _right = settings.Right;
        CanActivate = true;
        OnAwake(uiEvents);
    }

    //Variables
   // private readonly IBranch _childBranch;
    private readonly NavigationType _setNavigation;
    private readonly UINode _up;
    private readonly UINode _down;
    private readonly UINode _left;
    private readonly UINode _right;
    private IBranch _myBranch;
    private INode _myNode;
    private INavigationSettings _mySettings;
    private InputScheme _inputScheme;

    //Properties
    private IBranch ChildBranch => _mySettings.ChildBranch;
    protected override bool CanBeHighlighted() => false;
    //protected override bool CanBePressed() => !(_childBranch is null);
    protected override bool CanBePressed() => !(ChildBranch is null);
    protected override bool FunctionNotActive() => !CanActivate;
    protected override void SavePointerStatus(bool pointerOver) { }
    private bool MultiSelectAllowed => _inputScheme.MultiSelectPressed() &&
                                       _myNode.MultiSelectSettings.OpenChildBranch == IsActive.No
                                        && _myNode.MultiSelectSettings.AllowMultiSelect == IsActive.Yes ;

    //Main
    protected sealed override void OnAwake(IUiEvents events)
    {
        base.OnAwake(events);
        _myNode = events.ReturnMasterNode;
        _myBranch = _myNode.MyBranch;
        UseEServLocator();
    }
    
    public void UseEServLocator() => _inputScheme = EServ.Locator.Get<InputScheme>(this);

    protected override void AxisMoveDirection(MoveDirection moveDirection)
    {
        base.AxisMoveDirection(moveDirection);
        ProcessMoves(moveDirection);
    }

    private void ProcessMoves(MoveDirection moveDirection)
    {
        if (FunctionNotActive() || _setNavigation == NavigationType.None) return;

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
