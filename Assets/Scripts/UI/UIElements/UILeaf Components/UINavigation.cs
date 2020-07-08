using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System;

[System.Serializable]
public class UINavigation : IUINavigation
{
    [SerializeField] [AllowNesting] [Label("Move To When Clicked")] [HideIf("NotAToggle")] UIBranch _childBranch;
    [SerializeField] NavigationType _setNavigation = NavigationType.UpAndDown;
    [SerializeField] [AllowNesting] [ShowIf("UpDownNav")] UINode up;
    [SerializeField] [AllowNesting] [ShowIf("UpDownNav")] UINode down;
    [SerializeField] [AllowNesting] [ShowIf("RightLeftNav")] UINode left;
    [SerializeField] [AllowNesting] [ShowIf("RightLeftNav")] UINode right;

    #region Editor Scripts & Internal Classes
    public bool NotAToggle { get; set; }

    public bool UpDownNav()
    {
        if (_setNavigation == NavigationType.UpAndDown || _setNavigation == NavigationType.AllDirections)
        {
            return true;
        }
        return false;
    }

    public bool RightLeftNav()
    {
        if (_setNavigation == NavigationType.RightAndLeft || _setNavigation == NavigationType.AllDirections)
        {
            return true;
        }
        return false;
    }

    #endregion

    //Variables
    UINode _myNode;
    UIBranch _myBranch;
    public bool CanNaviagte { get; private set; }

    public UIBranch Child { get { return _childBranch; } }

    public void OnAwake(UINode node, UIBranch branch, Setting setting)
    {
        _myNode = node;
        _myBranch = branch;
        CanNaviagte = (setting & Setting.NavigationAndOnClick) != 0;
    }

    public void SetChildsParentBranch()
    {
        if (_myBranch.IsAPopUpBranch()) return;

        if (_childBranch)
        {
            _childBranch.SetNewParentBranch(_myBranch);
        }
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
            _myBranch.SaveLastHighlighted(_myNode);
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
        if (_setNavigation == NavigationType.None || !CanNaviagte) return;

        if (_setNavigation != NavigationType.RightAndLeft)
        {
            if (eventData.moveDir == MoveDirection.Down)
            {
                if (down)
                {
                    _myNode.TriggerExitEvent();
                    if (down.IsDisabled) { down.OnMove(eventData); return; }
                    down.Navigation.NavigateToNextNode();
                }
            }

            if (eventData.moveDir == MoveDirection.Up)
            {
                if (up)
                {
                    _myNode.TriggerExitEvent();
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
                    _myNode.TriggerExitEvent();
                    if (left.IsDisabled) { left.OnMove(eventData); return; }
                    left.Navigation.NavigateToNextNode();

                }
            }

            if (eventData.moveDir == MoveDirection.Right)
            {
                if (right)
                {
                    _myNode.TriggerExitEvent();
                    if (right.IsDisabled) { right.OnMove(eventData); return; }
                    right.Navigation.NavigateToNextNode();

                }
            }
        }
    }

    public void NavigateToNextNode()
    {
        if(!_myBranch.AllowKeys) return;
        if (_myNode.Function == ButtonFunction.HoverToActivate)
        {
            _myNode.PressedActions();
        }
        else
        {
            _myNode.Audio.Play(UIEventTypes.Highlighted);
        }
        _myNode.TriggerEnterEvent();
        _myBranch.SaveLastHighlighted(_myNode);
        _myNode.SetAsHighlighted();
    }

    public void MoveAfterTween()
    {
        if (_childBranch.ScreenType == ScreenType.ToFullScreen)
        {
            _myBranch.StartOutTween(() => ToFullScreen_AfterTween());
        }
        else if (_childBranch.ScreenType == ScreenType.Normal)
        {
            if (_childBranch.MyBranchType == BranchType.Internal)
            {
                _childBranch.MoveToNextLevel(_myBranch);
            }
            else
            {
                _myBranch.StartOutTween(() => ToChildBranch_AfterTween());
            }
        }
    }

    public void MoveOnClick()
    {
        if (_childBranch.ScreenType == ScreenType.ToFullScreen)
        {
            _myBranch.StartOutTween();
        }
        ToChildBranch_OnClick();
    }

    private void ToFullScreen_AfterTween()
    {
        ToChildBranch_AfterTween();
    }

    private void ToChildBranch_AfterTween()
    {
        if (!_myBranch.StayOn) { _myBranch.MyCanvas.enabled = false; }
        _childBranch.MoveToNextLevel(_myBranch);
    }

    private void ToChildBranch_OnClick()
    {
        if (!_myBranch.StayOn) { _myBranch.MyCanvas.enabled = false; }
        _childBranch.MoveToNextLevel(_myBranch);
    }

    public void TurnOffChildren()
    {
        if (_childBranch.WhenToMove == WhenToMove.Immediately)
        {
            _childBranch.StartOutTween();
            _childBranch.LastSelected.Deactivate();
        }
        else
        {
            _childBranch.StartOutTween(() => _childBranch.LastSelected.Deactivate());
        }
    }
}
