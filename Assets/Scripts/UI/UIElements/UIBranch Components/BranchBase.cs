

using UnityEngine;

public abstract class BranchBase : IEventUser, IOnHomeScreen, IClearScreen, IServiceUser
{
    protected BranchBase(UIBranch branch)
    {
        _myBranch = branch;
        OnEnable();
    }
    
    protected readonly UIBranch _myBranch;
    protected readonly ScreenData _screenData = new ScreenData();
    protected bool _inMenu, _canStart, _gameIsPaused, _resolvePopUps;
    protected IHistoryTrack _historyTrack;

    //Events
    private static CustomEvent<IOnHomeScreen> SetIsOnHomeScreen { get; } = new CustomEvent<IOnHomeScreen>();
    private static CustomEvent<IClearScreen> DoClearScreen { get; } = new CustomEvent<IClearScreen>();

    //Properties
    protected void InvokeOnHomeScreen(bool onHome)
    {
        OnHomeScreen = onHome;
        SetIsOnHomeScreen?.RaiseEvent(this);
    }
    protected void InvokeDoClearScreen() => DoClearScreen?.RaiseEvent(this);
    private void SaveResolvePopUps(INoResolvePopUp args) => _resolvePopUps = args.ActiveResolvePopUps;
    private void SaveIfGamePaused(IGameIsPaused args) => _gameIsPaused = args.GameIsPaused;
    protected virtual void SaveInMenu(IInMenu args) => _inMenu = args.InTheMenu;

    protected virtual void SaveIfOnHomeScreen(IOnHomeScreen args)
    {
        OnHomeScreen = args.OnHomeScreen;
        if (_myBranch.IsHomeScreenBranch() && OnHomeScreen && !_myBranch.CanvasIsEnabled)
        {
            ActivateBranchCanvas();
            ActivateBlockRaycast();
        }
    }
    protected virtual void SaveOnStart(IOnStart args) => _canStart = true;
    public bool OnHomeScreen { get; private set; } = true;
    public UIBranch IgnoreThisBranch => _myBranch;

    //Main
    private void OnEnable()
    {
        ObserveEvents();
        SubscribeToService();
    }

    public virtual void OnDisable()
    {
        RemoveFromEvents();
        _screenData.RemoveFromEvents();
    }
    
    public virtual void ObserveEvents()
    {
        EventLocator.Subscribe<IGameIsPaused>(SaveIfGamePaused, this);
        EventLocator.Subscribe<ISetUpStartBranches>(SetUpBranchesOnStart, this);
        EventLocator.Subscribe<IOnStart>(SaveOnStart, this);
        EventLocator.Subscribe<IOnHomeScreen>(SaveIfOnHomeScreen, this);
        EventLocator.Subscribe<IInMenu>(SaveInMenu, this);
        EventLocator.Subscribe<INoResolvePopUp>(SaveResolvePopUps, this);
        EventLocator.Subscribe<IClearScreen>(ClearBranchForFullscreen, this);
    }

    public virtual void RemoveFromEvents()
    {
        EventLocator.Unsubscribe<IGameIsPaused>(SaveIfGamePaused);
        EventLocator.Unsubscribe<ISetUpStartBranches>(SetUpBranchesOnStart);
        EventLocator.Unsubscribe<IOnStart>(SaveOnStart);
        EventLocator.Unsubscribe<IOnHomeScreen>(SaveIfOnHomeScreen);
        EventLocator.Unsubscribe<IInMenu>(SaveInMenu);
        EventLocator.Unsubscribe<INoResolvePopUp>(SaveResolvePopUps);
        EventLocator.Unsubscribe<IClearScreen>(ClearBranchForFullscreen);
    }
    
    public void SubscribeToService()
    {
        _historyTrack = ServiceLocator.GetNewService<IHistoryTrack>(this);
    }


    protected virtual void SetUpBranchesOnStart(ISetUpStartBranches args)
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
    
    public void ActivateBranchCanvas() => _myBranch.MyCanvas.enabled = true;

    public virtual void ActivateBlockRaycast()
    {
        if(!_canStart) return;
        _myBranch.MyCanvasGroup.blocksRaycasts = _inMenu;
    }

    protected virtual void ClearBranchForFullscreen(IClearScreen args)
    {
        if (args.IgnoreThisBranch == _myBranch || !_myBranch.CanvasIsEnabled) return;
        _myBranch.MyCanvas.enabled = false;
        _myBranch.MyCanvasGroup.blocksRaycasts = false;
    }

    protected virtual void CanGoToFullscreen()
    {
        if (_myBranch.ScreenType != ScreenType.FullScreen) return;
        InvokeDoClearScreen();
        InvokeOnHomeScreen(_myBranch.IsHomeScreenBranch());
    }
    
    public void ActivateStoredPosition()
    {
        _screenData.RestoreScreen();
        
        if (_screenData._wasOnHomeScreen)
            InvokeOnHomeScreen(true);
        
        _screenData._locked = false;
    }
}