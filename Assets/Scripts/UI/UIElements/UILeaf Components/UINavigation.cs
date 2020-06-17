﻿using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System;

[System.Serializable]
public class UINavigation
{
    [SerializeField] [AllowNesting] [Label("Move To When Clicked")] [HideIf("NotAToggle")] UIBranch _childBranch;
    [SerializeField] NavigationType _setNavigation = NavigationType.UpAndDown;
    [SerializeField] [AllowNesting] [ShowIf("UpDownNav")] UINode up;
    [SerializeField] [AllowNesting] [ShowIf("UpDownNav")] UINode down;
    [SerializeField] [AllowNesting] [ShowIf("RightLeftNav")] UINode left;
    [SerializeField] [AllowNesting] [ShowIf("RightLeftNav")] UINode right;
    public UnityEvent _asButtonEvent;
    public OnToggleEvent _asToggleEvent;

    [System.Serializable]
    public class OnToggleEvent : UnityEvent<bool> { }

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
    Setting _mySettings = Setting.NavigationAndOnClick;
    UINode _myNode;
    UIBranch _myBranch;

    public UIBranch Child { get { return _childBranch; } }

    public void OnAwake(UINode node, UIBranch branch)
    {
        _myNode = node;
        _myBranch = branch;
    }

    public void SetChildsParentBranch()
    {
        if (_myBranch.MyBranchType == BranchType.Independent) return;

        if (_childBranch)
        {
            _childBranch.SetNewParentBranch(_myBranch);
        }
    }


    public void PointerEnter(PointerEventData eventData)
    {
        if (eventData.pointerDrag) return;                  //Enables drag on slider to have pressed colour
        _myNode._audio.Play(UIEventTypes.Highlighted, _myNode._enabledFunctions);
        _myBranch.SaveLastHighlighted(_myNode);
        _myNode.SetAsHighlighted();
    }

    public void PointerExit(PointerEventData eventData)
    {
        if (eventData.pointerDrag) return;                      //Enables drag on slider to have pressed colour
        _myNode.SetNotHighlighted();
    }

    public void PointerDown()
    {
        _myNode.PressedActions();
        if (_myNode.IsDisabled) return;
        if (_myNode.IsCancel) return;

        if (_myNode.IsSelected)
        {
            _myNode._audio.Play(UIEventTypes.Cancelled, _myNode._enabledFunctions);
        }
        else
        {
            _myNode._audio.Play(UIEventTypes.Selected, _myNode._enabledFunctions);
        }
    }

    public void PointerUp()
    {
        if (_myNode.IsDisabled) return;
        if (_myNode.IsCancel) return;

        if (_myNode.GetSlider)
        {
            TriggerEvents();
            _myNode.TurnNodeOnOff();
        }
    }

    public void KeyBoardOrController(AxisEventData eventData)
    {
        if (_myNode.GetSlider)
        {
            if (_myNode.IsSelected)
            {
                if (eventData.moveDir == MoveDirection.Left || eventData.moveDir == MoveDirection.Right)
                {
                    _myNode._audio.Play(UIEventTypes.Selected, _myNode._enabledFunctions);
                }
            }
            else
            {
                ProcessMoves(eventData, _myNode._enabledFunctions);
            }
        }
        else
        {
            ProcessMoves(eventData, _myNode._enabledFunctions);
        }
    }


    private void ProcessMoves(AxisEventData eventData, Setting setting)
    {
        if (_setNavigation == NavigationType.None || (setting & _mySettings) == 0) return;

        if (_setNavigation != NavigationType.RightAndLeft)
        {
            if (eventData.moveDir == MoveDirection.Down)
            {
                if (down)
                {
                    if (down.IsDisabled) { down.OnMove(eventData); return; }
                    down._navigation.NavigateToNextNode();
                }
            }

            if (eventData.moveDir == MoveDirection.Up)
            {
                if (up)
                {
                    if (up.IsDisabled) { up.OnMove(eventData); return; }
                    up._navigation.NavigateToNextNode();
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
                    left._navigation.NavigateToNextNode();
                }
            }

            if (eventData.moveDir == MoveDirection.Right)
            {
                if (right)
                {
                    if (right.IsDisabled) { right.OnMove(eventData); return; }
                    right._navigation.NavigateToNextNode();
                }
            }
        }
    }

    public void OnEnterOrSelected()
    {
        PointerDown();
        if (_myNode.IsDisabled) return;
        if (_myNode.IsCancel) return;

        if (_myNode.GetSlider)        //TODO Need to check this still works properly
        {
            _myNode.GetSlider.interactable = _myNode.IsSelected;
            if (!_myNode.IsSelected)
            {
                TriggerEvents();
            }
        }
    }

    public void NavigateToNextNode()
    {
        if (!_myBranch.AllowKeys) return;
        _myBranch.SaveLastHighlighted(_myNode);
        _myNode._audio.Play(UIEventTypes.Highlighted, _myNode._enabledFunctions);
        _myNode.SetAsHighlighted();
    }

    public void MoveAfterTween()
    {
        if (_childBranch.ScreenType == ScreenType.ToFullScreen)
        {
            _myBranch.OutTweensToChild(() => ToFullScreen_AfterTween());
        }
        else if (_childBranch.ScreenType == ScreenType.Normal)
        {
            if (_childBranch.MyBranchType == BranchType.Internal)
            {
                _childBranch.MoveToNextLevel(_myBranch);
            }
            else
            {
                _myBranch.OutTweensToChild(() => ToChildBranch_AfterTween());
            }
        }
    }

    public void MoveOnClick()
    {
        if (_childBranch.ScreenType == ScreenType.ToFullScreen)
        {
            _myBranch.OutTweensToChild(() => ToChildBranch_OnClick());
        }
        _myNode.ProcessIndieData();
        _childBranch.MoveToNextLevel(_myBranch);
    }

    private void ToFullScreen_AfterTween()
    {
        UIHomeGroup.ClearHomeScreen(_myBranch);
        ToChildBranch_AfterTween();
    }

    private void ToChildBranch_AfterTween()
    {
        if (!_myBranch.DontTurnOff) { _myBranch.MyCanvas.enabled = false; }
        _myNode.ProcessIndieData();
        _childBranch.MoveToNextLevel(_myBranch);
    }

    private void ToChildBranch_OnClick()
    {
        if (!_myBranch.DontTurnOff) { _myBranch.MyCanvas.enabled = false; }
    }

    public void TurnOffChildren()
    {
        if (_childBranch.MyCanvas.enabled == false) return;

        if (_childBranch.WhenToMove == WhenToMove.OnClick)
        {
            _childBranch.OutTweenToParent();
            TurnOff();
        }
        else
        {
            _childBranch.OutTweenToParent(() => TurnOff());
        }
    }

    private void TurnOff()
    {
        _childBranch.LastHighlighted.SetNotHighlighted();
        _childBranch.LastSelected.Deactivate();
    }

    public void TriggerEvents()
    {
        _asButtonEvent?.Invoke();
        _asToggleEvent?.Invoke(_myNode.IsSelected);
    }
}
