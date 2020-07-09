using System;
using System.Collections.Generic;
using UnityEngine;

public class UICancel
{
    private readonly UIHub _uIHub;
    private readonly EscapeKey _globalEscapeSetting;
    private readonly UIBranch[] _homeGroup;

    private bool DontAllowTween => _uIHub.LastSelected.MyBranch.StayOn 
                                   || _uIHub.LastSelected.ChildBranch.MyBranchType == BranchType.Internal;
    private bool NotPausedAndIsNonResolvePopUp => _uIHub.LastHighlighted.MyBranch.IsNonResolvePopUp 
                                                  && !_uIHub.GameIsPaused;
    private bool NotPausedAndNoResolvePopUps => _uIHub.ActivePopUpsResolve.Count > 0 
                                                && !_uIHub.GameIsPaused;
    private bool IsPausedAndPauseMenu => _uIHub.GameIsPaused 
                                         && _uIHub.ActiveBranch.MyBranchType == BranchType.PauseMenu;

    public UICancel(UIHub uIHub, EscapeKey globalSetting, UIBranch[] homeBranches)
    {
        _uIHub = uIHub;
        _globalEscapeSetting = globalSetting;
        _homeGroup = homeBranches;
    }
    public void CancelPressed()
    {
        if(_uIHub.ActiveBranch.IsResolvePopUp) return;
        
        if (_uIHub.ActiveBranch.FromHotKey)
        {
            CancelOrBack(EscapeKey.BackToHome);
        }
        else if (_uIHub.GameIsPaused || _uIHub.ActiveBranch.IsNonResolvePopUp)
        {
            OnCancel(EscapeKey.BackOneLevel);
        }
        else if (CanEnterPauseOptionsScreen())
        {
            _uIHub.PauseOptionMenuPressed();
        }
        else
        {
            OnCancel(_uIHub.LastSelected.ChildBranch.EscapeKeySetting);
        }
    }
    
    public void CancelOrBack(EscapeKey escapeKey)
    {
        if (_uIHub.ActiveBranch.FromHotKey)
        {
            _uIHub.ActiveBranch.FromHotKey = false;
            _uIHub.LastSelected.SetNotSelected_NoEffects();
        }
        OnCancel(escapeKey);
    }


    private void OnCancel(EscapeKey escapeKey)
    {
        if (escapeKey == EscapeKey.GlobalSetting) escapeKey = _globalEscapeSetting;
        
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
            _uIHub.PauseOptionMenuPressed();
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
                    lastSelected.MyBranch.MoveToThisBranch();
                }
             }
        }
    }

    private void BackToHome()
    {
        int index = _uIHub.HomeGroupIndex;
        //List<UIBranch> homeGroup = _uIHub.HomeGroupBranches;
        if (_uIHub.OnHomeScreen) 
            _homeGroup[index].TweenOnChange = false;
        //homeGroup[index].LastSelected.SetNotSelected_NoEffects();
        _homeGroup[index].LastSelected.Deactivate();
        _homeGroup[index].LastSelected.MyBranch.SaveLastSelected(_homeGroup[index].LastSelected);
        _homeGroup[index].MoveToThisBranch();
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
