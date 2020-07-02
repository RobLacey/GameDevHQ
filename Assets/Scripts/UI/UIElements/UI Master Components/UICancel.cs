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
    public void CancelPressed()
    {
        if (_hubData.ActiveBranch.FromHotkey)
        {
            CancelOrBack(EscapeKey.BackToHome);
        }
        else if (_hubData.GameIsPaused || _hubData.ActiveBranch.IsAPopUpBranch())
        {
            OnCancel(EscapeKey.BackOneLevel);
        }
        else if (!CanCancel())
        {
            if (_hubData.PauseOptions == PauseOptionsOnEscape.EnterPauseOrEscape)
            {
                _hubData.PauseOptionMenu();
            }
        }
        else
        {
            OnCancel(_hubData.LastSelected.ChildBranch.EscapeKeySetting);
        }
    }
    
    public void CancelOrBack(EscapeKey escapeKey)
    {
        if (_hubData.ActiveBranch.FromHotkey)
        {
            _hubData.ActiveBranch.FromHotkey = false;
        }
        OnCancel(escapeKey);
    }


    private void OnCancel(EscapeKey escapeKey)
    {
        if (escapeKey == EscapeKey.GlobalSetting) escapeKey = _hubData.GlobalEscape;
        
        switch (escapeKey)
        {
            case EscapeKey.BackOneLevel:
                EscapeButtonProcess(BackOneLevel);
                break;
            case EscapeKey.BackToHome:
                EscapeButtonProcess(BackToHome);
                break;
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
        List<UIBranch> homeGroup = _hubData.HomeGroupBranches;

        if (_hubData.OnHomeScreen)
        {
            homeGroup[index].TweenOnChange = false;
        }
        homeGroup[index].LastSelected.SetNotSelected_NoEffects();
        homeGroup[index].LastSelected.MyBranch.SaveLastSelected(homeGroup[index].LastSelected);
        homeGroup[index].MoveToNextLevel();
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
        return _hubData.LastSelected.ChildBranch.MyCanvas.enabled;
    }
}
