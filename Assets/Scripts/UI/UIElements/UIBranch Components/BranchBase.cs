
using UnityEngine;

public abstract class BranchBase : IEventUser
{
    protected BranchBase(UIBranch branch)
    {
        _myBranch = branch;
        OnEnable();
    }
    
    protected readonly UIBranch _myBranch;
    protected readonly ScreenData _screenData = new ScreenData();
    protected bool _onHomeScreen = true, _noResolvePopUps = true;
    protected bool _inMenu, _canStart, _gameIsPaused;

    //Events
    private static CustomEvent<IOnHomeScreen, bool> SetIsOnHomeScreen { get; }
        = new CustomEvent<IOnHomeScreen, bool>();
    private static CustomEvent<IClearScreen, UIBranch> DoClearScreen { get; }
        = new CustomEvent<IClearScreen, UIBranch>();

    //Properties
    protected static void InvokeOnHomeScreen(bool onHome) => SetIsOnHomeScreen?.RaiseEvent(onHome);
    protected static void InvokeDoClearScreen(UIBranch ignoreThisBranch) => DoClearScreen?.RaiseEvent(ignoreThisBranch);
    private void SaveNoResolvePopUps(bool activeResolvePopUps) => _noResolvePopUps = activeResolvePopUps;
    private void SaveIfGamePaused(bool paused) => _gameIsPaused = paused;
    protected virtual void SaveInMenu(bool isInMenu) => _inMenu = isInMenu;
    protected virtual void SaveIfOnHomeScreen(bool currentlyOnHomeScreen)
    {
        _onHomeScreen = currentlyOnHomeScreen;
        if (_myBranch.IsHomeScreenBranch() && _onHomeScreen && !_myBranch.CanvasIsEnabled)
        {
            ActivateBranchCanvas();
            ActivateBlockRaycast();
        }
    }
    protected virtual void SaveOnStart() => _canStart = true;
    
    private void OnEnable() => ObserveEvents();

    public virtual void OnDisable()
    {
        RemoveFromEvents();
        _screenData.RemoveFromEvents();
    }
    
    public virtual void ObserveEvents()
    {
        EventLocator.SubscribeToEvent<IGameIsPaused, bool>(SaveIfGamePaused, this);
        EventLocator.SubscribeToEvent<ISetUpStartBranches, UIBranch>(SetUpBranchesOnStart, this);
        EventLocator.SubscribeToEvent<IOnStart>(SaveOnStart, this);
        EventLocator.SubscribeToEvent<IBackOrCancel, UIBranch>(MoveBackToThisBranch, this);
        EventLocator.SubscribeToEvent<IOnHomeScreen, bool>(SaveIfOnHomeScreen, this);
        EventLocator.SubscribeToEvent<IInMenu, bool>(SaveInMenu, this);
        EventLocator.SubscribeToEvent<INoResolvePopUp, bool>(SaveNoResolvePopUps, this);
        EventLocator.SubscribeToEvent<IClearScreen, UIBranch>(ClearBranchForFullscreen, this);
    }

    public virtual void RemoveFromEvents()
    {
        EventLocator.UnsubscribeFromEvent<IGameIsPaused, bool>(SaveIfGamePaused);
        EventLocator.UnsubscribeFromEvent<ISetUpStartBranches, UIBranch>(SetUpBranchesOnStart);
        EventLocator.UnsubscribeFromEvent<IOnStart>(SaveOnStart);
        EventLocator.UnsubscribeFromEvent<IBackOrCancel, UIBranch>(MoveBackToThisBranch);
        EventLocator.UnsubscribeFromEvent<IOnHomeScreen, bool>(SaveIfOnHomeScreen);
        EventLocator.UnsubscribeFromEvent<IInMenu, bool>(SaveInMenu);
        EventLocator.UnsubscribeFromEvent<INoResolvePopUp, bool>(SaveNoResolvePopUps);
        EventLocator.UnsubscribeFromEvent<IClearScreen, UIBranch>(ClearBranchForFullscreen);
    }

    protected virtual void SetUpBranchesOnStart(UIBranch startBranch)
    {
        _myBranch.MyCanvasGroup.blocksRaycasts = false;
        _myBranch.MyCanvas.enabled = false;
    }

    public abstract void SetUpBranch(UIBranch newParentController = null);

    public virtual void MoveBackToThisBranch(UIBranch lastBranch)
    {
        if(lastBranch == _myBranch)
        {
            _myBranch.MyParentBranch.LastSelected.ThisNodeIsSelected();
        }    
    }
    
    public void ActivateBranchCanvas()
    {
        _myBranch.MyCanvas.enabled = true;
    }

    public virtual void ActivateBlockRaycast()
    {
        if(!_canStart) return;
        _myBranch.MyCanvasGroup.blocksRaycasts = _inMenu;
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
        InvokeDoClearScreen(_myBranch);
        InvokeOnHomeScreen(_myBranch.IsHomeScreenBranch());
    }
    
    protected void ActivateStoredPosition()
    {
        _screenData.RestoreScreen();
        _screenData._activeBranch.MoveToBranchWithoutTween();
        
        if (_screenData._wasOnHomeScreen)
            InvokeOnHomeScreen(true);
        
        _screenData._locked = false;
    }

    protected static void GoToNextPopUp((UIBranch nextPopUp, UIBranch currentPopUp) data) 
        => data.nextPopUp.MoveToBranchWithoutTween();

}