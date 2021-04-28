using System;
using UnityEngine;


public class BranchBase : IEventUser, IOnHomeScreen, IClearScreen, IEServUser, IBranchBase, IBranchParams,
                                   IEventDispatcher, IAddNewBranch, IRemoveBranch
{
    protected BranchBase(IBranch branch)
    {
        _myBranch = branch.ThisBranch;
        _myCanvas = _myBranch.MyCanvas;
        _canvasOrderCalculator = new CanvasOrderCalculator(_myBranch);
        _myCanvasGroup = _myBranch.MyCanvasGroup;
        MyScreenType = _myBranch.ScreenType;
        _screenData = EJect.Class.WithParams<IScreenData>(this);
        SetCanvas(ActiveCanvas.No);
        SetBlockRaycast(BlockRaycast.No);
    }
    
    //Variables
    protected readonly IBranch _myBranch;
    protected readonly IScreenData _screenData;
    protected bool _inMenu, _canStart, _gameIsPaused, _activeResolvePopUps;
    protected IHistoryTrack _historyTrack;
    protected bool _isTabBranch;
    protected readonly CanvasOrderCalculator _canvasOrderCalculator;
    private readonly Canvas _myCanvas;
    private bool _allowKeys;
    protected readonly CanvasGroup _myCanvasGroup;

    //Events
    private Action<IOnHomeScreen> SetIsOnHomeScreen { get; set; }
    private Action<IClearScreen> DoClearScreen { get; set; }
    private Action<IAddNewBranch> AddThisBranch { get; } = EVent.Do.Fetch<IAddNewBranch>();
    private Action<IRemoveBranch> RemoveThisBranch { get; } = EVent.Do.Fetch<IRemoveBranch>();

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
    public IBranch MyBranch => _myBranch;
    public ScreenType MyScreenType { get; }
    public bool CanAllowKeys => _allowKeys;

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
        EVent.Do.Subscribe<IActivateBranchOnControlsChange>(WhenControlsChange);
    }

    protected virtual void WhenControlsChange(IActivateBranchOnControlsChange args)
    {
        if(args.ActiveBranch.NotEqualTo(_myBranch)) return;
        
        if(_myBranch.CanvasIsEnabled)
            _myBranch.DoNotTween();
        _myBranch.MoveToThisBranch();
    }

    public virtual void UseEServLocator() => _historyTrack = EServ.Locator.Get<IHistoryTrack>(this);

    public void SetUpAsTabBranch() => _isTabBranch = true;

    protected virtual void SetUpBranchesOnStart(ISetUpStartBranches args)
    {
        SetBlockRaycast(BlockRaycast.No);
        SetCanvas(ActiveCanvas.No);
    }

    public virtual bool CanStartBranch() => true;
    public virtual bool CanExitBranch(OutTweenType outTweenType) 
        => _myBranch.GetStayOn() != IsActive.Yes || outTweenType != OutTweenType.MoveToChild;

    public virtual void SetUpBranch(IBranch newParentController = null)
    {
        ActivateChildTabBranches(ActiveCanvas.Yes);
    }

    private void ActivateChildTabBranches(ActiveCanvas activeCanvas)
    {
        if (HasChildTabBranches())
        {
            _myBranch.LastSelected.ToggleData.ReturnTabBranch.SetCanvas(activeCanvas);
        }

        bool HasChildTabBranches()
        {
            return _myBranch.LastSelected.IsToggleGroup && _myBranch.LastSelected.ToggleData.ReturnTabBranch;
        }
    }
    
    public virtual void EndOfBranchStart() => SetBlockRaycast(BlockRaycast.Yes);

    public virtual void StartBranchExit() => SetBlockRaycast(BlockRaycast.No);

    public virtual void EndOfBranchExit()
    {
        SetCanvas(ActiveCanvas.No);
        _canvasOrderCalculator.ResetCanvasOrder();
        ActivateChildTabBranches(ActiveCanvas.No);
    }
    
    public virtual void SetCanvas(ActiveCanvas active)
    {
        if (active == ActiveCanvas.Yes)
        {
            AddThisBranch?.Invoke(this);
        }
        else
        {
            RemoveThisBranch?.Invoke(this);
        }
        
        _myCanvas.enabled = active == ActiveCanvas.Yes;
    }

    public virtual void SetBlockRaycast(BlockRaycast active)
    {
        if(!_canStart  || _activeResolvePopUps) return;
        
        if (_allowKeys)
        {
            _myCanvasGroup.blocksRaycasts = false;
        }
        else
        {
            _myCanvasGroup.blocksRaycasts = active == BlockRaycast.Yes;
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
    
    protected void CanGoToFullscreen_Paused()
    {
        if (MyScreenType != ScreenType.FullScreen) return;
        InvokeDoClearScreen();
    }

    protected virtual void ActivateStoredPosition()
    {
        _screenData.RestoreScreen();
        if (_screenData.WasOnHomeScreen)
            InvokeOnHomeScreen(true);
    }
}