using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

[System.Serializable]
public class UINavigation
{
    [AllowNesting] [Label("Move To When Clicked")] public UIBranch _childBranch;
    public NavigationType _setNavigation = NavigationType.UpAndDown;
    [AllowNesting] [ShowIf("UpDownNav")] public UINode up;
    [AllowNesting] [ShowIf("UpDownNav")] public UINode down;
    [AllowNesting] [ShowIf("RightLeftNav")] public UINode left;
    [AllowNesting] [ShowIf("RightLeftNav")] public UINode right;
    public UnityEvent _asButtonEvent;
    public OnToggleEvent _asToggleEvent;

    Setting _mySettings = Setting.NavigationAndOnClick;

    [System.Serializable]
    public class OnToggleEvent : UnityEvent<bool> { }

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

    public void ProcessMoves(AxisEventData eventData, Setting setting)
    {
        if (_setNavigation == NavigationType.None || (setting & _mySettings) == 0) return;

        if (_setNavigation != NavigationType.RightAndLeft)
        {
            if (eventData.moveDir == MoveDirection.Down)
            {
                if (down)
                {
                    if (down.Disabled) { down.OnMove(eventData); return; }
                    down.MoveToNext();
                }
            }

            if (eventData.moveDir == MoveDirection.Up)
            {
                if (up)
                {
                    if (up.Disabled) { up.OnMove(eventData); return; }
                    up.MoveToNext();
                }
            }
        }

        if (_setNavigation != NavigationType.UpAndDown)
        {
            if (eventData.moveDir == MoveDirection.Left)
            {
                if (left)
                {
                    if (left.Disabled) { left.OnMove(eventData); return; }
                    left.MoveToNext();
                }
            }

            if (eventData.moveDir == MoveDirection.Right)
            {
                if (right)
                {
                    if (right.Disabled) { right.OnMove(eventData); return; }
                    right.MoveToNext();
                }
            }
        }
    }
}
