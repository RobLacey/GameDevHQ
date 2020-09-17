using NaughtyAttributes;
using UnityEngine;
using UnityEngine.EventSystems;
using System;

[Serializable]
public class UINavigation : NodeFunctionBase
{
[SerializeField] [AllowNesting] [Label("Move To When Clicked")] [HideIf("NotAToggle")]
private UIBranch _childBranch;
    [SerializeField] private NavigationType _setNavigation = NavigationType.UpAndDown;
    [SerializeField] [AllowNesting] [ShowIf("UpDownNav")]
    private UINode _up;
    [SerializeField] [AllowNesting] [ShowIf("UpDownNav")]
    private UINode _down;
    [SerializeField] [AllowNesting] [ShowIf("RightLeftNav")]
    private UINode _left;
    [SerializeField] [AllowNesting] [ShowIf("RightLeftNav")]
    private UINode _right;

    //Editor Scripts
    public bool NotAToggle { get; set; }

    public bool UpDownNav() 
        => _setNavigation == NavigationType.UpAndDown || _setNavigation == NavigationType.AllDirections;

    public bool RightLeftNav() 
        => _setNavigation == NavigationType.RightAndLeft || _setNavigation == NavigationType.AllDirections;

    //Variables
    private UIBranch _myBranch;

    //Properties
    protected override bool CanBeHighlighted() => false;
    protected override bool CanBePressed() => _childBranch;
    protected override bool FunctionNotActive() => !CanActivate;
    protected override void SavePointerStatus(bool pointerOver) { }
    public UIBranch Child => _childBranch;


    public override void OnAwake(UINode node, UiActions uiActions)
    {
        base.OnAwake(node, uiActions);
        CanActivate = (_enabledFunctions & Setting.NavigationAndOnClick) != 0;
        _myBranch = node.MyBranch;
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
        RightAndLeftMove();
        UpAndDownMove();
    }

    private void RightAndLeftMove()
    {
        if (_setNavigation == NavigationType.RightAndLeft) return;
        
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
        }
    }

    private void UpAndDownMove()
    {
        if (_setNavigation == NavigationType.UpAndDown) return;
        
        switch (_moveDirection)
        {
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

    private void HandleMove(UINode moveTo)
    {
        if (moveTo.IsDisabled)
        {
            moveTo.DoMove(_moveDirection);
        }
        else
        {
            moveTo.OnPointerEnter(new PointerEventData(EventSystem.current));
        }
    }

    public void StartMoveToChild()
    {
        if (_childBranch.MyBranchType == BranchType.Internal)
        {
            ToChildBranchProcess();
        }
        else
        {
            _myBranch.StartOutTweenProcess(OutTweenType.MoveToChild, ToChildBranchProcess);
        }

        void ToChildBranchProcess() => _childBranch.MoveToThisBranch(_myBranch);
    }

    private protected override void ProcessPress()
    {
        if(FunctionNotActive() && !CanBePressed()) return;
        if (_isSelected)
        {
            StartMoveToChild();
        }
        else
        {
            TurnOffChildrenBranch();
        }
    }

    public void TurnOffChildrenBranch() 
        => _childBranch.StartOutTweenProcess(OutTweenType.Cancel, _childBranch.LastHighlighted.DeactivateAndCancelChildren);

    private protected override void ProcessDisabled(bool isDisabled) => TurnOffChildrenBranch();
}
