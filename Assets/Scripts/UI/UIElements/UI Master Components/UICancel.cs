using System;
using System.Collections.Generic;
using UnityEngine;

public static class UICancel
{
    private static List<UIBranch> homeGroup;
    private static UINode lastSelected;
    private static UIMasterController myMaster;
    private static int groupIndex;

    public static void Cancel(UIMasterController master, UINode lastNode, List<UIBranch> group, int index) //Cancel Class
    {
        lastSelected = lastNode;
        groupIndex = index;
        homeGroup = group;
        myMaster = master;

        if (lastSelected._navigation._childBranch != null
            && lastSelected._navigation._childBranch.MyCanvas.enabled != false)
        {
            OnCancel(lastSelected._navigation._childBranch.EscapeKeySetting);
        }
        else
        {
            OnCancel(lastSelected.MyBranch.EscapeKeySetting);
        }
    }

    public static void CancelOrBackButton(EscapeKey escapeKey, UIMasterController master, UINode lastNode, List<UIBranch> group, int index)
    {
        lastSelected = lastNode;
        groupIndex = index;
        homeGroup = group;
        myMaster = master;

        OnCancel(escapeKey);
    }

    private static void OnCancel(EscapeKey escapeKey)
    {
        if (escapeKey == EscapeKey.BackOneLevel)
        {
            if (lastSelected._navigation._childBranch == null)
            {
                myMaster.LastSelected = lastSelected.MyBranch.MyParentBranch.LastSelected;
            }
            EscapeButtonProcess(() => BackOneLevel());
        }
        else if (escapeKey == EscapeKey.BackToHome)
        {
            EscapeButtonProcess(() => BackToHome());
        }

        else if (escapeKey == EscapeKey.GlobalSetting)
        {
            OnCancel(myMaster.GlobalEscape);
        }
    }

    private static void EscapeButtonProcess(Action endAction) 
    {
        if (lastSelected._navigation._childBranch.WhenToMove == WhenToMove.AtTweenEnd)
        {
            lastSelected._navigation._childBranch.OutTweenToParent(() => endAction.Invoke());
            lastSelected._audio.Play(UIEventTypes.Cancelled, lastSelected._enabledFunctions);
        }
        else
        {
            lastSelected._navigation._childBranch.OutTweenToParent();
            lastSelected._audio.Play(UIEventTypes.Cancelled, lastSelected._enabledFunctions);
            endAction.Invoke();
        }
    }

    private static void BackToHome()
    {
        homeGroup[groupIndex].LastHighlighted.SetNotHighlighted();

        if (homeGroup[groupIndex].LastSelected)
        {
            homeGroup[groupIndex].LastSelected.Deactivate();
        }

        myMaster.LastSelected = homeGroup[groupIndex].LastSelected;

        if (!homeGroup[groupIndex].LastSelected.MyBranch.TweenOnHome) 
            homeGroup[groupIndex].LastSelected.MyBranch.TweenOnChange = false;

        homeGroup[groupIndex].MoveToNextLevel();
    }

    private static void BackOneLevel()
    {
        if (lastSelected.IsSelected != false)
        {
            lastSelected.PressedActions();
            lastSelected.SetNotHighlighted();
            if (lastSelected.MyBranch.DontTurnOff ||
                lastSelected._navigation._childBranch.MyBranchType == BranchType.Internal ||
                !lastSelected.MyBranch.TweenOnHome)
            {

                lastSelected.MyBranch.TweenOnChange = false;
            }
            lastSelected.MyBranch.MoveToNextLevel();
            myMaster.LastSelected = lastSelected.MyBranch.MyParentBranch.LastSelected;
        }
    }

}
