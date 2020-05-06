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
    [AllowNesting] [ShowIf("UpDownNav")] public UILeaf up;
    [AllowNesting] [ShowIf("UpDownNav")] public UILeaf down;
    [AllowNesting] [ShowIf("RightLeftNav")] public UILeaf left;
    [AllowNesting] [ShowIf("RightLeftNav")] public UILeaf right;
    public UnityEvent _asButtonEvent;
    public OnToggleEvent _asToggleEvent;

    [System.Serializable]
    public class OnToggleEvent : UnityEvent<bool> { }

    public enum NavigationType { RightAndLeft, UpAndDown, AllDirections, None }

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

    public void ProcessMoves(AxisEventData eventData)
    {
        if (_setNavigation == NavigationType.None) return;

        if (_setNavigation != NavigationType.RightAndLeft)
        {
            if (eventData.moveDir == MoveDirection.Down)
            {
                if (down) down.MoveToNext();
            }

            if (eventData.moveDir == MoveDirection.Up)
            {
                if (up) up.MoveToNext();
            }
        }

        if (_setNavigation != NavigationType.UpAndDown)
        {
            if (eventData.moveDir == MoveDirection.Left)
            {
                if (left) left.MoveToNext();
            }

            if (eventData.moveDir == MoveDirection.Right)
            {
                if (right) right.MoveToNext();
            }
        }
    }

    public void DrawNavLines(RectTransform myRect = null)
    {
        if (myRect = null) return;
        Vector3[] array = new Vector3[4];
        myRect.GetWorldCorners(array);
        Vector3 start = ((array[0] - array[1]) / 2);
        Gizmos.color = Color.red;
        Gizmos.DrawCube(array[0] - start, new Vector3(25,25,25));
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(array[0] - start, down.transform.position);
        Gizmos.DrawSphere(down.transform.position, 15);
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(array[0] - start, up.transform.position);
        Gizmos.DrawSphere(up.transform.position, 15);
    }
}
