using System;
using UnityEngine;

public abstract class BranchBase
{
    protected BranchBase(UIBranch branch)
    {
        _myBranch = branch;
        _isHomeScreenBranch = _myBranch._branchType == BranchType.HomeScreen; 
        _uiDataEvents.SubscribeToOnHomeScreen(SaveIfOnHomeScreen);
        _uiDataEvents.SubscribeToInMenu(SaveInMenu);
        _uiDataEvents.SubscribeToOnStart(SaveOnStart);
        _uiPopUpEvents.SubscribeNoResolvePopUps(SaveNoResolvePopUps);
        _uiControlsEvents.SubscribeToGameIsPaused(SaveIfGamePaused);
        _uiPopUpEvents.SubscribeNoResolvePopUps(SaveNoResolvePopUps);

        UIHub.SetUpBranchesAtStart += SetUpBranchesOnStart;
        DoClearScreen += ClearBranch;
    }
    
    protected readonly UIBranch _myBranch;
    protected readonly bool _isHomeScreenBranch;
    protected bool _onHomeScreen = true;
    protected readonly UIDataEvents _uiDataEvents = new UIDataEvents();
    private readonly UIPopUpEvents _uiPopUpEvents = new UIPopUpEvents();
    private readonly UIControlsEvents _uiControlsEvents = new UIControlsEvents();
    protected bool _inMenu;
    protected bool _canStart;
    protected bool _noActivePopUps = true;
    protected bool _gameIsPaused;
    protected bool _noResolvePopUps = true;


    //Events
    public static event Action<bool> SetIsOnHomeScreen; // Subscribe To track if on Home Screen
    public static event Action<UIBranch> DoClearScreen; // Subscribe To track if on Home Screen
    public Action OnStartPopUp;
    public Action<UIBranch> ActivateNextPopUp;
    protected readonly ScreenData _screenData = new ScreenData();

    //Properties
    protected void InvokeOnHomeScreen(bool onHome) => SetIsOnHomeScreen?.Invoke(onHome);
    private void InvokeDoClearScreen(UIBranch ignoreThisBranch) => DoClearScreen?.Invoke(ignoreThisBranch);
    private void SaveNoResolvePopUps(bool activeResolvePopUps) => _noResolvePopUps = activeResolvePopUps;
    private void SaveNoActivePopUps(bool noActivePopUps) => _noActivePopUps = noActivePopUps;
    private void SaveIfGamePaused(bool paused) => _gameIsPaused = paused;


    //Properties
    protected virtual void SaveInMenu(bool isInMenu) => _inMenu = isInMenu;
    protected virtual void SaveIfOnHomeScreen(bool currentlyOnHomeScreen) => _onHomeScreen = currentlyOnHomeScreen;
    protected virtual void SaveOnStart() => _canStart = true;


    protected virtual void SetUpBranchesOnStart(UIBranch startBranch)
    {
        _myBranch._myCanvasGroup.blocksRaycasts = false;
        _myBranch._myCanvas.enabled = false;
    }

    public abstract void SetUpBranch(UIBranch newParentController = null);
    
    public void ActivateBranch()
    {
        if(!_canStart) return; 
        _myBranch._myCanvasGroup.blocksRaycasts = _inMenu;
        _myBranch._myCanvas.enabled = true;
    }

    protected void ClearBranch(UIBranch ignoreThisBranch = null)
    {
        if (ignoreThisBranch == _myBranch || !_myBranch.CanvasIsEnabled) return;
        _myBranch._myCanvas.enabled = false;
        _myBranch._myCanvasGroup.blocksRaycasts = false;
    }

    protected void CanClearOrRestoreScreen()
    {
        if (_myBranch._screenType != ScreenType.FullScreen) return;
        
        if (_onHomeScreen)
        {
            InvokeOnHomeScreen(_isHomeScreenBranch);
        }
        else
        {
            InvokeDoClearScreen(_myBranch);
        }
    }
}