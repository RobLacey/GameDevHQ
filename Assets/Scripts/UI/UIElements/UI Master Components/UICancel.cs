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
        if (_myUIHub.GameIsPaused || _myUIHub.ActiveBranch.IsAPopUpBranch())
        {
            _myUIHub.LastSelected.IAudio.Play(UIEventTypes.Cancelled);
            endAction.Invoke();
            return;
        }

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

    private static void BackToHome() //TODO Review with Pause and Pop UP as may fall over
    {
        int index = _myUIHub.GroupIndex;

        if (_myUIHub.OnHomeScreen)
        {
            _homeGroup[index].TweenOnChange = false;
        }

        _homeGroup[index].LastSelected.SetNotSelected_NoEffects();
        _homeGroup[index].MoveToNextLevel();
        _homeGroup[index].LastSelected.MyBranch.SaveLastSelected(_homeGroup[index].LastSelected);
    }

    private static void BackOneLevel()
    {
        UINode lastSelected = _myUIHub.LastSelected;

        if (lastSelected.ChildBranch)
        {
            if (lastSelected.MyBranch.DontTurnOff || lastSelected.ChildBranch.MyBranchType == BranchType.Internal)
            {
                lastSelected.MyBranch.TweenOnChange = false;
            }
        }
        if (_myUIHub.GameIsPaused && _myUIHub.ActiveBranch.MyBranchType == BranchType.PauseMenu)
        {
            _myUIHub.PauseMenu();
        }
        else if (_myUIHub.ActivePopUps_Resolve.Count > 0 && !_myUIHub.GameIsPaused)
        {
            HandleRemovingPopUps_Resolve();
        }
        else if(_myUIHub.LastHighlighted.MyBranch.IsNonResolvePopUp && !_myUIHub.GameIsPaused)
        {
            _myUIHub.LastHighlighted.MyBranch.PopUpClass.RemoveFromActiveList_NonResolve();
        }
        else
        {
            lastSelected.SetNotSelected_NoEffects();
            lastSelected.MyBranch.SaveLastSelected(lastSelected.MyBranch.MyParentBranch.LastSelected);
            lastSelected.MyBranch.MoveToNextLevel();
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

    private static void HandleRemovingPopUps_Resolve()
    {
        if (_myUIHub.LastHighlighted.MyBranch.IsResolvePopUp)
        {
            _myUIHub.LastHighlighted.MyBranch.PopUpClass.RemoveFromActiveList_Resolve();
        }
        else
        {
            int lastIndexItem = _myUIHub.ActivePopUps_Resolve.Count - 1;
            _myUIHub.ActivePopUps_Resolve[lastIndexItem].PopUpClass.RemoveFromActiveList_Resolve();
        }
    }
}
