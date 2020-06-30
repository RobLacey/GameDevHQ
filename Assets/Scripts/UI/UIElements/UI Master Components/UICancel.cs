using System;
using System.Collections.Generic;
using UnityEngine;

public class UICancel : ICancel
{
    private IHubData _hubData;

    public UICancel(IHubData hubData)
    {
        _hubData = hubData;
    }

    public void OnCancel(EscapeKey escapeKey)
    {
        //_hubData.LastSelected.SetNotSelected_NoEffects();
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
            OnCancel(_hubData.GlobalEscape);
        }
    }

    private void EscapeButtonProcess(Action endAction) 
    {
        _hubData.LastSelected.IAudio.Play(UIEventTypes.Cancelled);

        if ( _hubData.ActiveBranch.IsAPopUpBranch())
        {
            endAction.Invoke();
            return;
        }

        if (_hubData.LastSelected.ChildBranch.WhenToMove == WhenToMove.AtTweenEnd)
        {
            _hubData.LastSelected.ChildBranch.StartOutTween(() => endAction.Invoke());
        }
        else
        {
            _hubData.LastSelected.ChildBranch.StartOutTween();
            endAction.Invoke();
        }
    }

    private void BackOneLevel()
    {
        UINode lastSelected = _hubData.LastSelected;

        if (lastSelected.ChildBranch)
        {
            if (lastSelected.MyBranch.DontTurnOff || lastSelected.ChildBranch.MyBranchType == BranchType.Internal)
            {
                lastSelected.MyBranch.TweenOnChange = false;
            }
        }

        if (_hubData.GameIsPaused && _hubData.ActiveBranch.MyBranchType == BranchType.PauseMenu)
        {
            lastSelected.MyBranch.TweenOnChange = true;
            _hubData.PauseOptionMenu();
        }
        else if (_hubData.ActivePopUps_Resolve.Count > 0 && !_hubData.GameIsPaused)
        {
            HandleRemovingPopUps_Resolve();
        }
        else if (_hubData.LastHighlighted.MyBranch.IsNonResolvePopUp && !_hubData.GameIsPaused)
        {
            Debug.Log("Here");
            _hubData.LastHighlighted.MyBranch.PopUpClass.RemoveFromActiveList_NonResolve();
        }
        else
        {
            lastSelected.SetNotSelected_NoEffects();
            lastSelected.MyBranch.SaveLastSelected(lastSelected.MyBranch.MyParentBranch.LastSelected);
            lastSelected.MyBranch.MoveToNextLevel();
        }
    }

    private void BackToHome()
    {
        int index = _hubData.GroupIndex;
        List<UIBranch> _homeGroup = _hubData.HomeGroupBranches;

        if (_hubData.OnHomeScreen)
        {
            _homeGroup[index].TweenOnChange = false;
        }
        _homeGroup[index].LastSelected.SetNotSelected_NoEffects();
        _homeGroup[index].LastSelected.MyBranch.SaveLastSelected(_homeGroup[index].LastSelected);
        _homeGroup[index].MoveToNextLevel();
    }

    private void HandleRemovingPopUps_Resolve()
    {
        if (_hubData.LastHighlighted.MyBranch.IsResolvePopUp)
        {
            _hubData.LastHighlighted.MyBranch.PopUpClass.RemoveFromActiveList_Resolve();
        }
        else
        {
            int lastIndexItem = _hubData.ActivePopUps_Resolve.Count - 1;
            _hubData.ActivePopUps_Resolve[lastIndexItem].PopUpClass.RemoveFromActiveList_Resolve();
        }
    }

    public bool CanCancel()
    {
        if (_hubData.LastSelected.ChildBranch != null
            && _hubData.LastSelected.ChildBranch.MyCanvas.enabled != false)
        {
            return true;
        }
        return false;
    }
}
