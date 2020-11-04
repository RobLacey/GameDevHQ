using UnityEngine.EventSystems;

public class UINavigation : NodeFunctionBase, IServiceUser
{
    public UINavigation(INavigationSettings settings, UiActions uiActions)
    {
        _childBranch = settings.ChildBranch;
        _setNavigation = settings.NavType;
        _up = settings.Up;
        _down = settings.Down;
        _left = settings.Left;
        _right = settings.Right;
        CanActivate = true;
        OnAwake(uiActions);
    }

    //Variables
    private UIBranch _childBranch;
    private readonly NavigationType _setNavigation;
    private readonly UINode _up;
    private readonly UINode _down;
    private readonly UINode _left;
    private readonly UINode _right;
    private UIBranch _myBranch;
    private UINode _myNode;
    private IHistoryTrack _uiHistoryTrack;

    //Properties
    protected override bool CanBeHighlighted() => false;
    protected override bool CanBePressed() => !(_childBranch is null);
    protected override bool FunctionNotActive() => !CanActivate;
    protected override void SavePointerStatus(bool pointerOver) { }

    protected sealed override void OnAwake(UiActions uiActions)
    {
        base.OnAwake(uiActions);
        _myNode = uiActions.Node;
        _myBranch = _myNode.MyBranch;
        SubscribeToService();
    }
    
    public void SubscribeToService() => _uiHistoryTrack = ServiceLocator.GetNewService<IHistoryTrack>(this);

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
                HandleMove(_down, moveDirection);
                break;
            }
            case MoveDirection.Up when _up:
            {
                HandleMove(_up, moveDirection);
                break;
            }
            case MoveDirection.Left when _left:
            {
                HandleMove(_left, moveDirection);
                break;
            }
            case MoveDirection.Right when _right:
            {
                HandleMove(_right, moveDirection);
                break;
            }
        }
    }

    private static void HandleMove(UINode moveTo, MoveDirection moveDirection) 
        => moveTo.CheckIfMoveAllowed(moveDirection);

    private protected override void ProcessPress()
    {
        if(FunctionNotActive() || !CanBePressed() || _childBranch is null) return;

        if (!_isSelected) return;
        _myBranch.NavigateToChildBranch(_childBranch);
    }
    
    private protected override void ProcessDisabled()
    {
        if(FunctionNotActive()) return;
        _myBranch.MoveToBranchWithoutTween();
        _uiHistoryTrack.CloseAllChildNodesAfterPoint(_myBranch.LastSelected.ReturnNode);
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
