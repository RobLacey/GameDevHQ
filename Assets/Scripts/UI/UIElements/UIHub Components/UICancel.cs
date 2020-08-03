using System;
using UnityEngine;

/// <summary>
/// Class handles all UI cancel behaviour from cancel type to context sensitive cases
/// </summary>
public class UICancel : INodeData, IBranchData, IHUbData, IMono
{
    private readonly UIHub _uIHub;
    private readonly PopUpController _popUpController;
    private readonly EscapeKey _globalEscapeSetting;
    private readonly UIBranch[] _homeGroup;
    private bool _noActiveResolvePopUps = true;
    private bool _noActiveNonResolvePopUps = true;

    public UICancel(UIHub uIHub, EscapeKey globalSetting, UIBranch[] homeBranches, PopUpController popUpController)
    {
        _uIHub = uIHub;
        _popUpController = popUpController;
        _globalEscapeSetting = globalSetting;
        _homeGroup = homeBranches;
        OnEnable();
    }

    //Properties
    private bool IsStayOnOrInternalBranch => LastSelected.MyBranch.StayOn; 
    private bool CanEnterPauseOptionsScreen =>
        (NoActivePopUps && LastSelected.HasChildBranch.MyCanvas.enabled == false)
        && _uIHub.PauseOptions == PauseOptionsOnEscape.EnterPauseOrEscapeMenu;
    
     public UINode LastHighlighted { get; private set; }
     public UINode LastSelected { get; private set; }
     public UIBranch ActiveBranch { get; private set; }
     public bool GameIsPaused { get; private set; }
     private bool NoActivePopUps => _noActiveResolvePopUps && _noActiveNonResolvePopUps;
     private void SetResolveCount(bool activeResolvePopUps) => _noActiveResolvePopUps = activeResolvePopUps;
     private void SetNonResolveCount(bool activeNonResolvePopUps) => _noActiveNonResolvePopUps = activeNonResolvePopUps;
     public void SaveHighlighted(UINode newNode) => LastHighlighted = newNode;
     public void SaveSelected(UINode newNode) => LastSelected = newNode;
     public void SaveActiveBranch(UIBranch newBranch) => ActiveBranch = newBranch;
     public void IsGamePaused(bool paused) => GameIsPaused = paused;
     
    public void OnEnable()
    {
        UINode.DoHighlighted += SaveHighlighted;
        UINode.DoSelected += SaveSelected;
        UIBranch.DoActiveBranch += SaveActiveBranch;
        UIHub.GamePaused += IsGamePaused;
        PopUpController.NoResolvePopUps += SetResolveCount;
        PopUpController.NoNonResolvePopUps += SetNonResolveCount;
    }

    public void OnDisable()
    {
        UINode.DoHighlighted -= SaveHighlighted;
        UINode.DoSelected -= SaveSelected;
        UIBranch.DoActiveBranch -= SaveActiveBranch;
        UIHub.GamePaused -= IsGamePaused;
        PopUpController.NoResolvePopUps -= SetResolveCount;
        PopUpController.NoNonResolvePopUps -= SetNonResolveCount;
    }

    public void CancelPressed()
    {
        if(ActiveBranch.IsResolvePopUp) return; //Can't just cancel a resolve PopUp
        
        if (ActiveBranch.FromHotKey)
        {
            CancelOrBack(EscapeKey.BackToHome);
        }
        else if (GameIsPaused || ActiveBranch.IsNonResolvePopUp)
        {
            ProcessCancelType(EscapeKey.BackOneLevel);
        }
        else if (CanEnterPauseOptionsScreen)
        {
            _uIHub.PauseOptionMenuPressed();
        }
        else
        {
            ProcessCancelType(LastSelected.HasChildBranch.EscapeKeySetting);
        }
    }
    
    public void CancelOrBack(EscapeKey escapeKey)
    {
        if (ActiveBranch.FromHotKey)
        {
            ActiveBranch.FromHotKey = false;
            LastSelected.SetNotSelected_NoEffects();
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
        LastSelected.Audio.Play(UIEventTypes.Cancelled);

        if (ActiveBranch.IsAPopUpBranch() || ActiveBranch.IsPauseMenuBranch())
        {
            endAction.Invoke();
            return;
        }
        
        if(ActiveBranch == ActiveBranch.MyParentBranch) return; //Stops Tween Error when no child
        
        if (ActiveBranch.WhenToMove == WhenToMove.AfterEndOfTween)
        {
            ActiveBranch.StartOutTween(endAction.Invoke);
        }
        else
        {
            ActiveBranch.StartOutTween();
            endAction.Invoke();
        }
    }

    private void BackOneLevel()
    {
        if (LastSelected.HasChildBranch && IsStayOnOrInternalBranch) LastSelected.MyBranch.TweenOnChange = false;

        if (GameIsPaused)
        {
            _uIHub.PauseOptionMenuPressed();
        }
        else if (!_noActiveResolvePopUps)
        {
            _popUpController.RemoveFromActiveList_Resolve();
        }
        else if (!_noActiveNonResolvePopUps)
        {
            _popUpController.RemoveFromActiveList_NonResolve();
        }
        else
        {
            LastSelected.SetNotSelected_NoEffects();
            LastSelected.SetAsSelected();
            LastSelected.MyBranch.MoveToThisBranch();
        }
    }

    private void BackToHome()
    {
        int index = _uIHub.HomeGroupIndex;
        if (_uIHub.OnHomeScreen) _homeGroup[index].TweenOnChange = false;
        _homeGroup[index].LastSelected.Deactivate();
        _homeGroup[index].LastSelected.SetAsSelected();
        _homeGroup[index].MoveToThisBranch();
    }
}
