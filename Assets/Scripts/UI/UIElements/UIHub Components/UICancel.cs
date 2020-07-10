﻿using System;

/// <summary>
/// Class handles all UI cancel behaviour from cancel type to context sensitive cases
/// </summary>
public class UICancel
{
    private readonly UIHub _uIHub;
    private readonly EscapeKey _globalEscapeSetting;
    private readonly UIBranch[] _homeGroup;

    //Properties
    private bool IsStayOnOrInternalBranch => _uIHub.LastSelected.MyBranch.StayOn 
                                   || _uIHub.LastSelected.ChildBranch.MyBranchType == BranchType.Internal;
    private bool IsANonResolvePopUp => _uIHub.LastHighlighted.MyBranch.IsNonResolvePopUp 
                                                  && !_uIHub.GameIsPaused;
    private bool ActiveResolvePopUps => _uIHub.ActivePopUpsResolve.Count > 0 
                                                && !_uIHub.GameIsPaused;
    private bool IsPausedAndPauseMenu => _uIHub.GameIsPaused 
                                         && _uIHub.ActiveBranch.MyBranchType == BranchType.PauseMenu;
    private bool CanEnterPauseOptionsScreen =>
        (_uIHub.NoActivePopUps && _uIHub.LastSelected.ChildBranch.MyCanvas.enabled == false)
        && _uIHub.PauseOptions == PauseOptionsOnEscape.EnterPauseOrEscapeMenu;


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
            ProcessCancelType(EscapeKey.BackOneLevel);
        }
        else if (CanEnterPauseOptionsScreen)
        {
            _uIHub.PauseOptionMenuPressed();
        }
        else
        {
            ProcessCancelType(_uIHub.LastSelected.ChildBranch.EscapeKeySetting);
        }
    }
    
    public void CancelOrBack(EscapeKey escapeKey)
    {
        if (_uIHub.ActiveBranch.FromHotKey)
        {
            _uIHub.ActiveBranch.FromHotKey = false;
            _uIHub.LastSelected.SetNotSelected_NoEffects();
        }
        ProcessCancelType(escapeKey);
    }


    private void ProcessCancelType(EscapeKey escapeKey)
    {
        if (escapeKey == EscapeKey.GlobalSetting) escapeKey = _globalEscapeSetting;
        
        switch (escapeKey)
        {
            case EscapeKey.BackOneLevel:
                StartCancelProcess(BackOneLevel);
                break;
            case EscapeKey.BackToHome:
                StartCancelProcess(BackToHome);
                break;
        }
    }

    private void StartCancelProcess(Action endAction) 
    {
        _uIHub.LastSelected.Audio.Play(UIEventTypes.Cancelled);

        if ( _uIHub.ActiveBranch.IsAPopUpBranch())
        {
            endAction.Invoke();
            return;
        }
        
        if(!_uIHub.LastSelected.ChildBranch) return; //Stops Tween Error when no child
        
        if (_uIHub.LastSelected.ChildBranch.WhenToMove == WhenToMove.AfterEndOfTween)
        {
            _uIHub.LastSelected.ChildBranch.StartOutTween(endAction.Invoke);
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

        if (lastSelected.ChildBranch && IsStayOnOrInternalBranch) lastSelected.MyBranch.TweenOnChange = false;

        if (IsPausedAndPauseMenu)
        {
            lastSelected.MyBranch.TweenOnChange = true;
            _uIHub.PauseOptionMenuPressed();
            return;
        }

        if (ActiveResolvePopUps)
        {
            HandleRemovingPopUps_Resolve();
        }
        else if (IsANonResolvePopUp)
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

    private void BackToHome()
    {
        int index = _uIHub.HomeGroupIndex;
        if (_uIHub.OnHomeScreen) _homeGroup[index].TweenOnChange = false;
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
}
