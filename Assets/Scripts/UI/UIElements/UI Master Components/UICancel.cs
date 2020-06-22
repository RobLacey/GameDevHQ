using System;
using System.Collections.Generic;
using UnityEngine;

public static class UICancel
{
    public static List<UIBranch> _homeGroup;
    public static UIHub _myUIHub;


    public static void Cancel()
    {
        if (_myUIHub.LastSelected.ChildBranch != null
            && _myUIHub.LastSelected.ChildBranch.MyCanvas.enabled != false)
        {
            OnCancel(_myUIHub.LastSelected.ChildBranch.EscapeKeySetting);
        }
    }

    public static void CancelOrBackButton(EscapeKey escapeKey)
    {
        OnCancel(escapeKey);
    }

    private static void OnCancel(EscapeKey escapeKey)
    {
        if (escapeKey == EscapeKey.BackOneLevel)
        {
            EscapeButtonProcess(() => BackOneLevel());
        }
        else if (escapeKey == EscapeKey.BackToHome)
        {
            EscapeButtonProcess(() => BackToHome());
        }

        else if (escapeKey == EscapeKey.GlobalSetting)
        {
            OnCancel(_myUIHub.GlobalEscape);
        }
    }

    private static void EscapeButtonProcess(Action endAction) 
    {
        if (_myUIHub.LastSelected.ChildBranch.WhenToMove == WhenToMove.AtTweenEnd)
        {
            _myUIHub.LastSelected.ChildBranch.OutTweenToParent(() => endAction.Invoke());
            _myUIHub.LastSelected.IAudio.Play(UIEventTypes.Cancelled);
        }
        else
        {
            _myUIHub.LastSelected.ChildBranch.OutTweenToParent();
            _myUIHub.LastSelected.IAudio.Play(UIEventTypes.Cancelled);
            endAction.Invoke();
        }
    }

    private static void BackToHome()
    {
        int index = _myUIHub.GroupIndex;

        if (_myUIHub.OnHomeScreen)
        {
            _homeGroup[index].TweenOnChange = false;
        }

        _homeGroup[index].LastSelected.Deactivate();
        _homeGroup[index].LastSelected.SetNotHighlighted();
        _homeGroup[index].MoveToNextLevel();
    }

    private static void BackOneLevel()
    {
        UINode lastSelected = _myUIHub.LastSelected;

        if (lastSelected.IsSelected != false)
        {
            if (lastSelected.MyBranch.DontTurnOff || lastSelected.ChildBranch.MyBranchType == BranchType.Internal)
            {
                lastSelected.MyBranch.TweenOnChange = false;
            }
            lastSelected.Deactivate();
            lastSelected.SetNotHighlighted();
            lastSelected.MyBranch.MoveToNextLevel();
            _myUIHub.LastSelected = lastSelected.MyBranch.MyParentBranch.LastSelected;
        }
    }

    public static void ResetHierachy()
    {
        UINode thisNode = _myUIHub.LastSelected;

        while (thisNode.IsSelected == true)
        {
            thisNode.SetNotSelected_NoEffects();
            thisNode = thisNode.MyBranch.MyParentBranch.LastSelected;
        }
    }


}
