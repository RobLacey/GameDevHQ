using UnityEngine;
using UnityEngine.EventSystems;

public class UINavigation : NodeFunctionBase, IServiceUser
{
    public UINavigation(InavigationSettings settings, UiActions uiActions)
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
        if (_myNode.IsToggleGroup || _myNode.IsToggleNotLinked) 
            _childBranch = null;
        _myBranch = _myNode.MyBranch;
        SubscribeToService();
    }
    
    public void SubscribeToService()
    {
        _uiHistoryTrack = ServiceLocator.GetNewService<IHistoryTrack>(this);
        //return _uiHistoryTrack is null;
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

        if (!_isSelected) return;
        StopReturnFlashFromFullScreen();
        _myBranch.MoveToAChildBranch(_childBranch);
    }
    
    private void StopReturnFlashFromFullScreen()
    {
        if (_childBranch.ScreenType == ScreenType.FullScreen)
            _myNode.SetNodeAsNotSelected_NoEffects();
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
