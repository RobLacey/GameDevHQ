﻿using System;

public abstract class BranchBase
{
    protected BranchBase(UIBranch branch)
    {
        _myBranch = branch;
        _isHomeScreenBranch = _myBranch.MyBranchType == BranchType.HomeScreen; 
        OnEnable();
    }
    
    protected readonly UIBranch _myBranch;
    protected readonly UIDataEvents _uiDataEvents = new UIDataEvents();
    protected readonly UIPopUpEvents _uiPopUpEvents = new UIPopUpEvents();
    protected readonly UIControlsEvents _uiControlsEvents = new UIControlsEvents();
    protected readonly ScreenData _screenData = new ScreenData();
    protected readonly bool _isHomeScreenBranch;
    protected bool _onHomeScreen = true;
    protected bool _inMenu, _canStart, _gameIsPaused;
    protected bool _noResolvePopUps = true;
    private bool _noActivePopUps = true;

    //Events
    public static event Action<bool> SetIsOnHomeScreen; // Subscribe To track if on Home Screen
    public static event Action<UIBranch> DoClearScreen; // Subscribe To track if on Home Screen

    //Properties
    protected void InvokeOnHomeScreen(bool onHome) => SetIsOnHomeScreen?.Invoke(onHome);
    protected void InvokeDoClearScreen(UIBranch ignoreThisBranch) => DoClearScreen?.Invoke(ignoreThisBranch);
    private void SaveNoResolvePopUps(bool activeResolvePopUps) => _noResolvePopUps = activeResolvePopUps;
    private void SaveNoActivePopUps(bool noActivePopUps) => _noActivePopUps = noActivePopUps;
    private void SaveIfGamePaused(bool paused) => _gameIsPaused = paused;
    protected virtual void SaveInMenu(bool isInMenu) => _inMenu = isInMenu;
    protected virtual void SaveIfOnHomeScreen(bool currentlyOnHomeScreen) => _onHomeScreen = currentlyOnHomeScreen;
    protected virtual void SaveOnStart() => _canStart = true;
    private bool ReturnToGame() => !_screenData._wasInTheMenu && _noActivePopUps;

    private void OnEnable()
    {
        _uiDataEvents.SubscribeToOnHomeScreen(SaveIfOnHomeScreen);
        _uiDataEvents.SubscribeToInMenu(SaveInMenu);
        _uiDataEvents.SubscribeToOnStart(SaveOnStart);
        _uiDataEvents.SubscribeToBackOneLevel(MoveBackToThisBranch);
        _uiDataEvents.SubscribeToGameIsPaused(SaveIfGamePaused);
        _uiDataEvents.SubscribeSetUpBranchesAtStart(SetUpBranchesOnStart);
        _uiPopUpEvents.SubscribeNoResolvePopUps(SaveNoResolvePopUps);
        _uiPopUpEvents.SubscribeNoPopUps(SaveNoActivePopUps);
        DoClearScreen += ClearBranchForFullscreen;
    }

    protected virtual void SetUpBranchesOnStart(UIBranch startBranch)
    {
        _myBranch.MyCanvasGroup.blocksRaycasts = false;
        _myBranch.MyCanvas.enabled = false;
    }

    public abstract void SetUpBranch(UIBranch newParentController = null);

    protected virtual void MoveBackToThisBranch(UIBranch lastBranch)
    {
        _myBranch.LastSelected.SetNotSelected_NoEffects(); //TODO Review
        _myBranch.MyParentBranch.LastSelected.ThisNodeIsSelected();
    }
    
    public void ActivateBranch()
    {
        if(!_canStart) return; 
        _myBranch.MyCanvasGroup.blocksRaycasts = _inMenu;
        _myBranch.MyCanvas.enabled = true;
    }

    protected virtual void ClearBranchForFullscreen(UIBranch ignoreThisBranch = null)
    {
        if (ignoreThisBranch == _myBranch || !_myBranch.CanvasIsEnabled) return;

        _myBranch.MyCanvas.enabled = false;
        _myBranch.MyCanvasGroup.blocksRaycasts = false;
    }

    protected virtual void CanGoToFullscreen()
    {
        if (_myBranch.ScreenType != ScreenType.FullScreen) return;

        if (!_onHomeScreen) return;
        InvokeDoClearScreen(_myBranch);
        InvokeOnHomeScreen(_isHomeScreenBranch);
    }
    
    protected void ReturnToMenuOrGame((UIBranch nextPopUp, UIBranch currentPopUp) data)
    {
        if (ReturnToGame())
        {
            data.nextPopUp.LastHighlighted.ThisNodeIsHighLighted();
        }
        else
        {
            data.nextPopUp.MoveToBranchWithoutTween();
        }
    }
}