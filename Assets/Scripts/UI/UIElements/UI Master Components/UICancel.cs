using System;
using System.Collections.Generic;
using UnityEngine;

public class UICancel
{
    private readonly UIHub _uIHub;
    private bool DontAllowTween => _uIHub.LastSelected.MyBranch.StayOn 
                                   || _uIHub.LastSelected.ChildBranch.MyBranchType == BranchType.Internal;
    private bool NotPausedAndIsNonResolvePopUp => _uIHub.LastHighlighted.MyBranch.IsNonResolvePopUp 
                                                  && !_uIHub.GameIsPaused;
    private bool NotPausedAndNoResolvePopUps => _uIHub.ActivePopUpsResolve.Count > 0 
                                                && !_uIHub.GameIsPaused;
    private bool IsPausedAndPauseMenu => _uIHub.GameIsPaused 
                                         && _uIHub.ActiveBranch.MyBranchType == BranchType.PauseMenu;

    public UICancel(UIHub uIHub)
    {
        _uIHub = uIHub;
    }
    public void CancelPressed()
    {
        if(_uIHub.ActiveBranch.IsResolvePopUp) return;
        
        if (_uIHub.ActiveBranch.FromHotkey)
        {
            CancelOrBack(EscapeKey.BackToHome);
        }
        else if (_uIHub.GameIsPaused || _uIHub.ActiveBranch.IsNonResolvePopUp)
        {
            OnCancel(EscapeKey.BackOneLevel);
        }
        else if (CanEnterPauseOptionsScreen())
        {
            _uIHub.PauseOptionMenu();
        }
        else
        {
            OnCancel(_uIHub.LastSelected.ChildBranch.EscapeKeySetting);
        }
    }
    
    public void CancelOrBack(EscapeKey escapeKey)
    {
        if (_uIHub.ActiveBranch.FromHotkey)
        {
            _uIHub.ActiveBranch.FromHotkey = false;
            _uIHub.LastSelected.SetNotSelected_NoEffects();
        }
        OnCancel(escapeKey);
    }


    private void OnCancel(EscapeKey escapeKey)
    {
        if (escapeKey == EscapeKey.GlobalSetting) escapeKey = _uIHub.GlobalEscape;
        
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
        _uIHub.LastSelected.Audio.Play(UIEventTypes.Cancelled);

        if ( _uIHub.ActiveBranch.IsAPopUpBranch())
        {
            endAction.Invoke();
            return;
        }

        // if (_uIHub.GameIsPaused)
        // {
        //     _uIHub.SetLastSelected(_uIHub.ActiveBranch.LastSelected);
        //     Debug.Log("Paused");
        // }
        
        if(!_uIHub.LastSelected.ChildBranch) return; //Stops Tween Error when no child
        
        if (_uIHub.LastSelected.ChildBranch.WhenToMove == WhenToMove.AfterEndOfTween)
        {
            _uIHub.LastSelected.ChildBranch.StartOutTween(() => endAction.Invoke());
        }
        else
        {
            _uIHub.LastSelected.ChildBranch.StartOutTween();
            endAction.Invoke();
        }
    }

    private void BackOneLevel()
    {
        var lastSelected = _uIHub.LastSelected;

        if (lastSelected.ChildBranch)
        {
            if (DontAllowTween) 
                lastSelected.MyBranch.TweenOnChange = false;
        }

        if (IsPausedAndPauseMenu)
        {
            lastSelected.MyBranch.TweenOnChange = true;
            _uIHub.PauseOptionMenu();
        }
        else
        {
             if (NotPausedAndNoResolvePopUps)
             {
                 HandleRemovingPopUps_Resolve();
             }
             else
             {
                if (NotPausedAndIsNonResolvePopUp)
                {
                    _uIHub.LastHighlighted.MyBranch.PopUpClass.RemoveFromActiveList_NonResolve();
                }
                else
                {
                    lastSelected.SetNotSelected_NoEffects();
                    lastSelected.MyBranch.SaveLastSelected(lastSelected.MyBranch.MyParentBranch.LastSelected);
                    lastSelected.MyBranch.MoveToNextLevel();
                }
             }
        }
    }

    private void BackToHome()
    {
        int index = _uIHub.GroupIndex;
        List<UIBranch> homeGroup = _uIHub.HomeGroupBranches;
        if (_uIHub.OnHomeScreen) 
            homeGroup[index].TweenOnChange = false;
        //homeGroup[index].LastSelected.SetNotSelected_NoEffects();
        homeGroup[index].LastSelected.Deactivate();
        homeGroup[index].LastSelected.MyBranch.SaveLastSelected(homeGroup[index].LastSelected);
        homeGroup[index].MoveToNextLevel();
    }

    private void HandleRemovingPopUps_Resolve()
    {
        if (_uIHub.LastHighlighted.MyBranch.IsResolvePopUp)
        {
            _uIHub.LastHighlighted.MyBranch.PopUpClass.RemoveFromActiveList_Resolve();
        }
        else
        {
            int lastIndexItem = _uIHub.ActivePopUpsResolve.Count - 1;
            _uIHub.ActivePopUpsResolve[lastIndexItem].PopUpClass.RemoveFromActiveList_Resolve();
        }
    }

    private bool CanEnterPauseOptionsScreen()
    {
        return (_uIHub.NoActivePopUps && _uIHub.LastSelected.ChildBranch.MyCanvas.enabled == false) 
               && _uIHub.PauseOptions == PauseOptionsOnEscape.EnterPauseOrEscapeMenu;
    }
}
