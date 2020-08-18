using System;
using System.Collections.Generic;
using UnityEngine;

//Todo Stop Optional popups appearing when in fullscreen. Maybe new popups altogether and maybe cache them 
public class UIPopUp : IPopUp
{
    public UIPopUp(UIBranch branch, UIBranch[] branchList)
    {
        _myBranch = branch;
        _allBranches = branchList;
        OnEnable();
    }

    //Variables
    private readonly UIBranch _myBranch;
    private readonly UIBranch[] _allBranches;
    private bool _noActivePopUps = true;
    private bool _isInMenu;
    private readonly UIDataEvents _uiDataEvents = new UIDataEvents();
    private readonly UIControlsEvents _uiControlsEvents = new UIControlsEvents();
    private readonly UIPopUpEvents _uiPopUpEvents = new UIPopUpEvents();
    private readonly List<UIBranch> _clearedBranches = new List<UIBranch>();
    private bool _gameIsPaused;
    private bool _inGameBeforePopUp;
    private bool _onHomeScreen = true;

    //Properties
    private void SaveNoActivePopUps(bool noActivePopUps) => _noActivePopUps = noActivePopUps;
    private void SaveIfGamePaused(bool paused) => _gameIsPaused = paused;
    private void SaveIfInMenu(bool inMenu) => _isInMenu = inMenu;
    private void SaveOnHomeScreen(bool onHomeScreen) => _onHomeScreen = onHomeScreen;
    
    //Events
    public static event Action<UIBranch> AddResolvePopUp;
    public static event Action<UIBranch> AddOptionalPopUp;

    private void OnEnable()
    {
        _uiControlsEvents.SubscribeToGameIsPaused(SaveIfGamePaused);
        _uiDataEvents.SubscribeToInMenu(SaveIfInMenu);
        _uiDataEvents.SubscribeToOnHomeScreen(SaveOnHomeScreen);
        _uiPopUpEvents.SubscribeNoPopUps(SaveNoActivePopUps);
    }

    public void StartPopUp()
    {
        if (_gameIsPaused) return; //TODO add to buffer goes here for when paused. trigger from SaveOnHome?
        
        if (!_myBranch.CanvasIsEnabled)
            SetUpPopUp();
    }
    
    private void SetUpPopUp()
    {
        if (!_isInMenu && _noActivePopUps)
            _inGameBeforePopUp = true;

        if (_myBranch.IsResolvePopUp)
        {
            AddResolvePopUp?.Invoke(_myBranch);
            PopUpStartProcess();
        }

        if (_myBranch.IsOptionalPopUp && _onHomeScreen) //TODO add to buffer goes here fro not on home.trigger from SaveOnHome?
        {
            AddOptionalPopUp?.Invoke(_myBranch);
            PopUpStartProcess();
        }        
    }

    private void PopUpStartProcess()
    {
        _clearedBranches.Clear();
        
        if (_myBranch.IsResolvePopUp || _myBranch.IsPauseMenuBranch())
            ClearAndStoreActiveBranches();
        
        ActivatePopUp();
    }

    private void ClearAndStoreActiveBranches()
    {
        foreach (var branchToClear in _allBranches)
        {
            if (branchToClear.CanvasIsEnabled && branchToClear != _myBranch)
                _clearedBranches.Add(branchToClear);
        }
    }

    private void ActivatePopUp()
    {
        _myBranch.LastSelected.Audio.Play(UIEventTypes.Selected);
        _myBranch.MoveToBranchFromPopUp();
    }
    
    public void MoveToNextPopUp(UIBranch lastBranch)
    {
        EndOfTweenActions(lastBranch);
    }

    private void EndOfTweenActions(UIBranch lastBranch)
    {
        DoRestoreScreen();
        
        if (_noActivePopUps && _inGameBeforePopUp)
        {
            ReturnToGame(lastBranch);
        }
        else
        {
            ToLastActiveBranch(lastBranch);
        }
    }

    private void ReturnToGame(UIBranch lastBranch)
    {
        lastBranch.LastHighlighted.ThisNodeIsHighLighted();
        _inGameBeforePopUp = false;
    }

    private static void ToLastActiveBranch(UIBranch lastActiveBranch) 
        => lastActiveBranch.MoveToBranchWithoutTween();

    private void DoRestoreScreen()
    {
        foreach (var branch in _clearedBranches)
        {
            branch.ActivateBranch();
        }
    }
}

