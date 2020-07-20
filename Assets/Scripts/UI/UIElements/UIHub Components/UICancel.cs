using System;
using UnityEngine;

/// <summary>
/// Class handles all UI cancel behaviour from cancel type to context sensitive cases
/// </summary>
public class UICancel : INodeData, IBranchData, IHUbData, IMono
{
    private readonly UIHub _uIHub;
    private readonly EscapeKey _globalEscapeSetting;
    private readonly UIBranch[] _homeGroup;

    //Properties
    private bool IsStayOnOrInternalBranch => LastSelected.MyBranch.StayOn 
                                   || LastSelected.HasChildBranch.MyBranchType == BranchType.Internal;
    private bool IsANonResolvePopUp => LastHighlighted.MyBranch.IsNonResolvePopUp 
                                                  && !/*_uIHub.*/GameIsPaused;
    private bool ActiveResolvePopUps => _uIHub.ActivePopUpsResolve.Count > 0 
                                                && !/*_uIHub.*/GameIsPaused;
    private bool IsPausedAndPauseMenu => /*_uIHub.*/GameIsPaused 
                                         && _uIHub.ActiveBranch.MyBranchType == BranchType.PauseMenu;
    private bool CanEnterPauseOptionsScreen =>
        (_uIHub.NoActivePopUps && LastSelected.HasChildBranch.MyCanvas.enabled == false)
        && _uIHub.PauseOptions == PauseOptionsOnEscape.EnterPauseOrEscapeMenu;
    
     public UINode LastHighlighted { get; private set; }
     public UINode LastSelected { get; private set; }
     public UIBranch ActiveBranch { get; private set; }
     public bool GameIsPaused { get; private set; }


     public UICancel(UIHub uIHub, EscapeKey globalSetting, UIBranch[] homeBranches)
    {
        _uIHub = uIHub;
        _globalEscapeSetting = globalSetting;
        _homeGroup = homeBranches;
        OnEnable();
    }
    
    public void OnEnable()
    {
        UINode.DoHighlighted += SaveHighlighted;
        UINode.DoSelected += SaveSelected;
        UIBranch.DoActiveBranch += SaveActiveBranch;
        UIHub.GamePaused += IsGamePaused;
    }

    public void OnDisable()
    {
        UINode.DoHighlighted -= SaveHighlighted;
        UINode.DoSelected -= SaveSelected;
        UIBranch.DoActiveBranch -= SaveActiveBranch;
        UIHub.GamePaused -= IsGamePaused;

    }

    public void CancelPressed()
    {
        if(/*_uIHub.*/ActiveBranch.IsResolvePopUp) return; //Can't just cancel a resolve PopUp
        
        if (/*_uIHub.*/ActiveBranch.FromHotKey)
        {
            CancelOrBack(EscapeKey.BackToHome);
        }
        else if (/*_uIHub.*/GameIsPaused || /*_uIHub.*/ActiveBranch.IsNonResolvePopUp)
        {
            ProcessCancelType(EscapeKey.BackOneLevel);
        }
        else if (CanEnterPauseOptionsScreen)
        {
            _uIHub.PauseOptionMenuPressed();
        }
        else
        {
            ProcessCancelType(/*_uIHub.*/LastSelected.HasChildBranch.EscapeKeySetting);
        }
    }
    
    public void CancelOrBack(EscapeKey escapeKey)
    {
        if (/*_uIHub.*/ActiveBranch.FromHotKey)
        {
            /*_uIHub.*/ActiveBranch.FromHotKey = false;
            /*_uIHub.*/LastSelected.SetNotSelected_NoEffects();
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
        /*_uIHub.*/LastSelected.Audio.Play(UIEventTypes.Cancelled);

        if ( /*_uIHub.*/ActiveBranch.IsAPopUpBranch() || /*_uIHub.*/ActiveBranch.IsPause())
        {
            endAction.Invoke();
            return;
        }
        
        if(/*_uIHub.*/ActiveBranch == /*_uIHub.*/ActiveBranch.MyParentBranch) return; //Stops Tween Error when no child
        
        if (/*_uIHub.*/ActiveBranch.WhenToMove == WhenToMove.AfterEndOfTween)
        {
            /*_uIHub.*/ActiveBranch.StartOutTween(endAction.Invoke);
        }
        else
        {
            /*_uIHub.*/ActiveBranch.StartOutTween();
            endAction.Invoke();
        }
        // if(!_uIHub.LastSelected.HasChildBranch) return; //Stops Tween Error when no child
        //
        // if (_uIHub.LastSelected.HasChildBranch.WhenToMove == WhenToMove.AfterEndOfTween)
        // {
        //     _uIHub.LastSelected.HasChildBranch.StartOutTween(endAction.Invoke);
        // }
        // else
        // {
        //     _uIHub.LastSelected.HasChildBranch.StartOutTween();
        //     endAction.Invoke();
        // }
    }

    private void BackOneLevel()
    {
       // var lastSelected = _uIHub.LastSelected;

        if (/*lastSelected.*/ LastSelected.HasChildBranch && IsStayOnOrInternalBranch) /*lastSelected.*/ LastSelected.MyBranch.TweenOnChange = false;

        if (IsPausedAndPauseMenu)
        {
            //lastSelected.MyBranch.TweenOnChange = true;
            _uIHub.PauseOptionMenuPressed();
            return;
        }

        if (ActiveResolvePopUps)
        {
            _uIHub.RemoveFromActiveList_Resolve();

            //HandleRemovingPopUps_Resolve();
        }
        else if (IsANonResolvePopUp)
        {
            //*_uIHub.*/LastHighlighted.MyBranch.PopUpClass.RemoveFromActiveList_NonResolve();
            /*_uIHub.*/ _uIHub.RemoveFromActiveList_NonResolve(/*LastHighlighted.MyBranch*/);
        }
        else
        {
            /*lastSelected*/LastSelected.SetNotSelected_NoEffects();
            //lastSelected.MyBranch.SaveLastSelected(lastSelected);
            /*lastSelected*/LastSelected.SetAsSelected();
            /*lastSelected*/LastSelected.MyBranch.MoveToThisBranch();
        }
    }

    private void BackToHome()
    {
        int index = _uIHub.HomeGroupIndex;
        if (_uIHub.OnHomeScreen) _homeGroup[index].TweenOnChange = false;
        _homeGroup[index].LastSelected.Deactivate();
        //_homeGroup[index].LastSelected.MyBranch.SaveLastSelected(_homeGroup[index].LastSelected);
        _homeGroup[index].LastSelected.SetAsSelected();
        _homeGroup[index].MoveToThisBranch();
    }
    
    public void SaveHighlighted(UINode newNode) => LastHighlighted = newNode;

    public void SaveSelected(UINode newNode) => LastSelected = newNode;

    public void SaveActiveBranch(UIBranch newBranch) => ActiveBranch = newBranch;
    public void IsGamePaused(bool paused) => GameIsPaused = paused;
}
