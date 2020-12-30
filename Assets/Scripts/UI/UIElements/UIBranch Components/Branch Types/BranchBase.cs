using System;
using UnityEngine;


public abstract class BranchBase : IEventUser, IOnHomeScreen, IClearScreen, IEServUser, IBranchBase, IBranchParams,
                                   IEventDispatcher
{
    protected BranchBase(IBranch branch)
    {
        _myBranch = branch.ThisBranch;
        _myCanvas = _myBranch.MyCanvas;
        _myCanvasGroup = _myBranch.MyCanvasGroup;
        MyScreenType = _myBranch.ScreenType;
        _screenData = EJect.Class.WithParams<IScreenData>(this);
    }
    
    //Variables
    protected readonly IBranch _myBranch;
    protected readonly IScreenData _screenData;
    protected bool _inMenu, _canStart, _gameIsPaused, _activeResolvePopUps;
    protected IHistoryTrack _historyTrack;
    private readonly Canvas _myCanvas;
    private readonly CanvasGroup _myCanvasGroup;
    protected bool _isTabBranch;
    private bool _allowKeys;

    //Events
    private Action<IOnHomeScreen> SetIsOnHomeScreen { get; set; }
    private Action<IClearScreen> DoClearScreen { get; set; }

    //Properties & Set/Getters
    protected void InvokeOnHomeScreen(bool onHome)
    {
        OnHomeScreen = onHome;
        SetIsOnHomeScreen?.Invoke(this);
    }
    private void InvokeDoClearScreen() => DoClearScreen?.Invoke(this);
    private void SaveResolvePopUps(INoResolvePopUp args) => _activeResolvePopUps = args.ActiveResolvePopUps;
    private void SaveIfGamePaused(IGameIsPaused args) => _gameIsPaused = args.GameIsPaused;
    protected virtual void SaveInMenu(IInMenu args) => _inMenu = args.InTheMenu;
    protected virtual void SaveIfOnHomeScreen(IOnHomeScreen args) => OnHomeScreen = args.OnHomeScreen;
    protected virtual void SaveOnStart(IOnStart args) => _canStart = true;

    private void AllowKeys(IAllowKeys args)
    {
        _allowKeys = args.CanAllowKeys;
        if (!_canStart && _allowKeys)
        {
            _myCanvasGroup.blocksRaycasts = false;
        }
        var blockRaycast = _allowKeys ? BlockRaycast.No : BlockRaycast.Yes;
        SetBlockRaycast(blockRaycast);
    }
    public bool OnHomeScreen { get; private set; } = true;
    public IBranch IgnoreThisBranch => _myBranch;
    public ScreenType MyScreenType { get; }

    //Main
    public void OnEnable()
    {
        FetchEvents();
        ObserveEvents();
        UseEServLocator();
        _screenData.OnEnable();
    }
    
    public virtual void FetchEvents()
    {
        SetIsOnHomeScreen = EVent.Do.Fetch<IOnHomeScreen>();
        DoClearScreen = EVent.Do.Fetch<IClearScreen>();
    }

    public virtual void ObserveEvents()
    {
        EVent.Do.Subscribe<INoResolvePopUp>(SaveResolvePopUps);
        EVent.Do.Subscribe<IGameIsPaused>(SaveIfGamePaused);
        EVent.Do.Subscribe<ISetUpStartBranches>(SetUpBranchesOnStart);
        EVent.Do.Subscribe<IOnStart>(SaveOnStart);
        EVent.Do.Subscribe<IOnHomeScreen>(SaveIfOnHomeScreen);
        EVent.Do.Subscribe<IInMenu>(SaveInMenu);
        EVent.Do.Subscribe<IClearScreen>(ClearBranchForFullscreen);
        EVent.Do.Subscribe<IAllowKeys>(AllowKeys);
    }

    public void UseEServLocator() => _historyTrack = EServ.Locator.Get<IHistoryTrack>(this);

    public void SetUpAsTabBranch() => _isTabBranch = true;

    protected virtual void SetUpBranchesOnStart(ISetUpStartBranches args)
    {
        SetBlockRaycast(BlockRaycast.No);
        SetCanvas(ActiveCanvas.No);
    }
    
    public virtual void OnStart() { }

    public abstract void SetUpBranch(IBranch newParentController = null);
    
    public virtual void SetCanvas(ActiveCanvas active) => _myCanvas.enabled = active == ActiveCanvas.Yes;

    public virtual void SetBlockRaycast(BlockRaycast active)
    {
        if(!_canStart) return;
        if (_allowKeys)
        {
            _myCanvasGroup.blocksRaycasts = false;
        }
        else
        {
            _myCanvasGroup.blocksRaycasts = active == BlockRaycast.Yes && _inMenu;
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
        if (MyScreenType != ScreenType.FullScreen || !OnHomeScreen) return;
        InvokeDoClearScreen();
        InvokeOnHomeScreen(false);
    }
    
    public virtual void ActivateStoredPosition()
    {
        _screenData.RestoreScreen();
        if (_screenData.WasOnHomeScreen)
            InvokeOnHomeScreen(true);
    }
}