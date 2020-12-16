﻿using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public interface IBranchBase : IParameters
{
    void OnEnable();
    void SetUpAsTabBranch();
    void SetUpBranch(IBranch newParentController = null);
    void SetCanvas(ActiveCanvas active);
    void SetBlockRaycast(BlockRaycast active);
    void ActivateStoredPosition();
}

public interface IBranchParams
{
    ScreenType MyScreenType { get; }
}

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
    
    protected readonly IBranch _myBranch;
    protected readonly IScreenData _screenData;
    protected bool _inMenu, _canStart, _gameIsPaused, _resolvePopUps;
    protected IHistoryTrack _historyTrack;
    private readonly Canvas _myCanvas;
    private readonly CanvasGroup _myCanvasGroup;
    protected bool _isTabBranch;

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
    private void SaveResolvePopUps(INoResolvePopUp args) => _resolvePopUps = args.ActiveResolvePopUps;
    private void SaveIfGamePaused(IGameIsPaused args) => _gameIsPaused = args.GameIsPaused;
    protected virtual void SaveInMenu(IInMenu args) => _inMenu = args.InTheMenu;
    protected virtual void SaveIfOnHomeScreen(IOnHomeScreen args) => OnHomeScreen = args.OnHomeScreen;
    protected virtual void SaveOnStart(IOnStart args) => _canStart = true;
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
    }

    public void UseEServLocator() => _historyTrack = EServ.Locator.Get<IHistoryTrack>(this);

    public void SetUpAsTabBranch() => _isTabBranch = true;

    protected virtual void SetUpBranchesOnStart(ISetUpStartBranches args)
    {
        SetBlockRaycast(BlockRaycast.No);
        SetCanvas(ActiveCanvas.No);
    }

    public abstract void SetUpBranch(IBranch newParentController = null);
    
    public virtual void SetCanvas(ActiveCanvas active) => _myCanvas.enabled = active == ActiveCanvas.Yes;

    public virtual void SetBlockRaycast(BlockRaycast active)
    {
        if(!_canStart) return;
        Debug.Log(_myBranch + " : " + active);
        _myCanvasGroup.blocksRaycasts = active == BlockRaycast.Yes && _inMenu;
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
        _screenData.WasOnHomeScreen = true;
        InvokeDoClearScreen();
        InvokeOnHomeScreen(false);
    }
    
    public void ActivateStoredPosition()
    {
        _screenData.RestoreScreen();
        if (_screenData.WasOnHomeScreen)
            InvokeOnHomeScreen(true);
    }
}