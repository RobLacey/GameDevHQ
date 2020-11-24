using UnityEngine;

public interface IBranchBase
{
    void OnDisable();
    void SetUpAsTabBranch();
    void SetUpBranchesOnStart(ISetUpStartBranches args);
    void SetUpBranch(IBranch newParentController = null);
    void SetCanvas(ActiveCanvas active);
    void SetBlockRaycast(BlockRaycast active);
    void ActivateStoredPosition();
}

public abstract class BranchBase : IEventUser, IOnHomeScreen, IClearScreen, IServiceUser, IBranchBase
{
    protected BranchBase(IBranch branch)
    {
        _myBranch = branch.ThisBranch;
        _myCanvas = _myBranch.MyCanvas;
        _myCanvasGroup = _myBranch.MyCanvasGroup;
        _screenData = new ScreenData(_myBranch.ScreenType);
        OnEnable();
    }
    
    protected readonly IBranch _myBranch;
    protected readonly ScreenData _screenData;
    protected bool _inMenu, _canStart, _gameIsPaused, _resolvePopUps;
    protected IHistoryTrack _historyTrack;
    private readonly Canvas _myCanvas;
    private readonly CanvasGroup _myCanvasGroup;
    protected bool _isTabBranch;

    //Events
    private static CustomEvent<IOnHomeScreen> SetIsOnHomeScreen { get; } = new CustomEvent<IOnHomeScreen>();
    private static CustomEvent<IClearScreen> DoClearScreen { get; } = new CustomEvent<IClearScreen>();

    //Properties
    protected void InvokeOnHomeScreen(bool onHome)
    {
        OnHomeScreen = onHome;
        SetIsOnHomeScreen?.RaiseEvent(this);
    }

    private void InvokeDoClearScreen() => DoClearScreen?.RaiseEvent(this);
    private void SaveResolvePopUps(INoResolvePopUp args) => _resolvePopUps = args.ActiveResolvePopUps;
    private void SaveIfGamePaused(IGameIsPaused args) => _gameIsPaused = args.GameIsPaused;
    protected virtual void SaveInMenu(IInMenu args) => _inMenu = args.InTheMenu;
    protected virtual void SaveIfOnHomeScreen(IOnHomeScreen args) => OnHomeScreen = args.OnHomeScreen;
    protected virtual void SaveOnStart(IOnStart args) => _canStart = true;
    public bool OnHomeScreen { get; private set; } = true;
    public IBranch IgnoreThisBranch => _myBranch;

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
    
    public void SubscribeToService() => _historyTrack = ServiceLocator.Get<IHistoryTrack>(this);

    public void SetUpAsTabBranch() => _isTabBranch = true;

    public virtual void SetUpBranchesOnStart(ISetUpStartBranches args)
    {
        SetBlockRaycast(BlockRaycast.No);
        SetCanvas(ActiveCanvas.No);
    }

    public abstract void SetUpBranch(IBranch newParentController = null);
    
    public void SetCanvas(ActiveCanvas active) => _myCanvas.enabled = active == ActiveCanvas.Yes;

    public virtual void SetBlockRaycast(BlockRaycast active)
    {
        if(!_canStart) return;
        if (active == BlockRaycast.Yes)
        {
            _myCanvasGroup.blocksRaycasts = _inMenu;
        }
        else
        {
            _myCanvasGroup.blocksRaycasts = false;
        }
    }

    protected virtual void ClearBranchForFullscreen(IClearScreen args)
    {
        if (args.IgnoreThisBranch == _myBranch || !_myBranch.CanvasIsEnabled) return;
        SetCanvas(ActiveCanvas.No);
        SetBlockRaycast(BlockRaycast.No);
    }

    protected void CanGoToFullscreen()
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