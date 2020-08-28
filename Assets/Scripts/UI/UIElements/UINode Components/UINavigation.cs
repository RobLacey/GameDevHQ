using NaughtyAttributes;
using UnityEngine;
using UnityEngine.EventSystems;
using System;

[Serializable]
public class UINavigation
{
[SerializeField] [AllowNesting] [Label("Move To When Clicked")] [HideIf("NotAToggle")] UIBranch _childBranch;
    [SerializeField] NavigationType _setNavigation = NavigationType.UpAndDown;
    [SerializeField] [AllowNesting] [ShowIf("UpDownNav")] UINode up;
    [SerializeField] [AllowNesting] [ShowIf("UpDownNav")] UINode down;
    [SerializeField] [AllowNesting] [ShowIf("RightLeftNav")] UINode left;
    [SerializeField] [AllowNesting] [ShowIf("RightLeftNav")] UINode right;

    //Editor Scripts
    public bool NotAToggle { get; set; }

    public bool UpDownNav() 
        => _setNavigation == NavigationType.UpAndDown || _setNavigation == NavigationType.AllDirections;

    public bool RightLeftNav() 
        => _setNavigation == NavigationType.RightAndLeft || _setNavigation == NavigationType.AllDirections;

    //Variables
    UINode _myNode;
    UIBranch _myBranch;
    private bool _allowKeys;
    public bool CanNavigate { get; private set; }

    public UIBranch Child { get { return _childBranch; } }

    public void OnAwake(UINode node, UIBranch branch, Setting setting)
    {
        _myNode = node;
        _myBranch = branch;
        CanNavigate = (setting & Setting.NavigationAndOnClick) != 0;
        ChangeControl.DoAllowKeys += SaveAllowKeys;
    }

    private void SaveAllowKeys(bool allow)
    {
        _allowKeys = allow;
    }

    public void PointerEnter(PointerEventData eventData)
    {
        if (eventData.pointerDrag) return;                  //Enables drag on slider to have pressed colour
        if (_myNode.Function == ButtonFunction.HoverToActivate & !_myNode.IsSelected)
        {
            _myNode.PressedActions();
        }
        else
        {
            _myNode.Audio.Play(UIEventTypes.Highlighted);
            _myNode.ThisNodeIsHighLighted();
            _myNode.SetAsHighlighted();
        }
    }

    public void PointerExit(PointerEventData eventData)
    {
        if (eventData.pointerDrag) return;                      //Enables drag on slider to have pressed colour
        _myNode.SetNotHighlighted();
    }

    public void KeyBoardOrController(AxisEventData eventData)
    {
        if (_myNode.AmSlider)
        {
            if (_myNode.IsSelected)
            {
                if (eventData.moveDir == MoveDirection.Left || eventData.moveDir == MoveDirection.Right)
                {
                    _myNode.Audio.Play(UIEventTypes.Selected);
                }
            }
            else
            {
                ProcessMoves(eventData);
            }
        }
        else
        {
            ProcessMoves(eventData);
        }
    }


    private void ProcessMoves(AxisEventData eventData)
    {
        if (_setNavigation == NavigationType.None || !CanNavigate) return;

        if (_setNavigation != NavigationType.RightAndLeft)
        {
            if (eventData.moveDir == MoveDirection.Down)
            {
                if (down)
                {
                    if (down.IsDisabled) { down.OnMove(eventData); return; }
                    down.Navigation.NavigateToNextNode();
                }
            }

            if (eventData.moveDir == MoveDirection.Up)
            {
                if (up)
                {
                    if (up.IsDisabled) { up.OnMove(eventData); return; }
                    up.Navigation.NavigateToNextNode();
                }
            }
        }

        if (_setNavigation != NavigationType.UpAndDown)
        {
            if (eventData.moveDir == MoveDirection.Left)
            {
                if (left)
                {
                    if (left.IsDisabled) { left.OnMove(eventData); return; }
                    left.Navigation.NavigateToNextNode();
                }
            }

            if (eventData.moveDir == MoveDirection.Right)
            {
                if (right)
                {
                    if (right.IsDisabled) { right.OnMove(eventData); return; }
                    right.Navigation.NavigateToNextNode();
                }
            }
        }
        _myNode.SetNotHighlighted();
    }

    public void NavigateToNextNode()
    {
        if(!_allowKeys) return;
        if (_myNode.Function == ButtonFunction.HoverToActivate)
        {
            _myNode.PressedActions();
        }
        else
        {
            _myNode.Audio.Play(UIEventTypes.Highlighted);
        }
        //_myNode.TriggerEnterEvent();
        _myNode.ThisNodeIsHighLighted();
        _myNode.SetAsHighlighted();
    }

    public void StartMoveToChild()
    {
        if (_childBranch.MyBranchType == BranchType.Internal)
        {
            _childBranch.MoveToThisBranch(_myBranch);
            return;
        }

        _myBranch.StartOutTweenProcess(OutTweenType.MoveToChild, ToChildBranchProcess);

        void ToChildBranchProcess() => _childBranch.MoveToThisBranch(_myBranch);
    }
    
    public void TurnOffChildren()
    {
        _childBranch.StartOutTweenProcess(OutTweenType.Cancel, _childBranch.LastHighlighted.Deactivate);
    }
}
