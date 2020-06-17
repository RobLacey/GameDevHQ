using System;
using System.Collections.Generic;
using UnityEngine;

public static class UICancel
{
    public static List<UIBranch> homeGroup;
    public static UIMasterController myMaster;

    public static void Cancel() //Cancel Class
    {
        if (myMaster.LastSelected.ChildBranch != null
            && myMaster.LastSelected.ChildBranch.MyCanvas.enabled != false)
        {
            OnCancel(myMaster.LastSelected.ChildBranch.EscapeKeySetting);
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
            if (myMaster.LastSelected.ChildBranch == null)
            {
                myMaster.LastSelected = myMaster.LastSelected.MyBranch.MyParentBranch.LastSelected;
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
        if (myMaster.LastSelected.ChildBranch.WhenToMove == WhenToMove.AtTweenEnd)
        {
            myMaster.LastSelected.ChildBranch.OutTweenToParent(() => endAction.Invoke());
            myMaster.LastSelected._audio.Play(UIEventTypes.Cancelled, myMaster.LastSelected._enabledFunctions);
        }
        else
        {
            myMaster.LastSelected.ChildBranch.OutTweenToParent();
            myMaster.LastSelected._audio.Play(UIEventTypes.Cancelled, myMaster.LastSelected._enabledFunctions);
            endAction.Invoke();
        }
    }

    private static void BackToHome()
    {
        int index = myMaster.GroupIndex;

        homeGroup[index].LastHighlighted.SetNotHighlighted();

        if (homeGroup[index].LastSelected)
        {
            homeGroup[index].LastSelected.Deactivate();
        }

        myMaster.LastSelected = homeGroup[index].LastSelected;
        homeGroup[index].MoveToNextLevel();
    }

    private static void BackOneLevel()
    {
        UINode lastSelected = myMaster.LastSelected;

        if (lastSelected.IsSelected != false)
        {
            lastSelected.PressedActions();
            lastSelected.SetNotHighlighted();
            if (lastSelected.MyBranch.DontTurnOff || lastSelected.ChildBranch.MyBranchType == BranchType.Internal)
            {
                lastSelected.MyBranch.TweenOnChange = false;
            }
            lastSelected.MyBranch.MoveToNextLevel();
            myMaster.LastSelected = lastSelected.MyBranch.MyParentBranch.LastSelected;
        }
    }

}
