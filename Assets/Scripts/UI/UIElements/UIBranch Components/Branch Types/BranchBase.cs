using System;
using EZ.Events;
using EZ.Service;
using UnityEngine;


public class BranchBase : IEZEventUser, IOnHomeScreen, IClearScreen, IServiceUser, IBranchBase, IBranchParams,
                          IEZEventDispatcher, ICanInteractWithBranch, ICannotInteractWithBranch, ICanvasCalcParms, ISetPositionParms
{
    protected BranchBase(IBranch branch)
    {
        _myBranch = branch.ThisBranch;
        _myCanvas = _myBranch.MyCanvas;
        _myCanvasGroup = _myBranch.MyCanvasGroup;
        MyScreenType = _myBranch.ScreenType;
    }
    
    //Variables
    protected readonly IBranch _myBranch;
    protected IScreenData _screenData;
    protected bool _inMenu, _canStart, _gameIsPaused, _activeResolvePopUps;
    protected IHistoryTrack _historyTrack;
    protected bool _isTabBranch;
    protected ICanvasOrderCalculator _canvasOrderCalculator;
    protected readonly Canvas _myCanvas;
    protected readonly CanvasGroup _myCanvasGroup;
    protected IDataHub _myDataHub;

    //Events
    private Action<IOnHomeScreen> SetIsOnHomeScreen { get; set; }
    private Action<IClearScreen> DoClearScreen { get; set; }
    private Action<ICanInteractWithBranch> AddThisBranch { get; } = BranchEvent.Do.Fetch<ICanInteractWithBranch>();
    private Action<ICannotInteractWithBranch> RemoveThisBranch { get; } = BranchEvent.Do.Fetch<ICannotInteractWithBranch>();

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
        CanAllowKeys = args.CanAllowKeys;
        if (!_canStart && CanAllowKeys)
        {
            _myCanvasGroup.blocksRaycasts = false;
        }
        var blockRaycast = CanAllowKeys ? BlockRaycast.No : BlockRaycast.Yes;
        SetBlockRaycast(blockRaycast);
    }
    public bool OnHomeScreen { get; private set; } = true;
    public IBranch IgnoreThisBranch => _myBranch;
    public IBranch MyBranch => _myBranch;
    public ScreenType MyScreenType { get; }
    protected bool CanAllowKeys { get; private set; }

    //Main
    public virtual void OnAwake()
    {
        _canvasOrderCalculator = EZInject.Class.WithParams<ICanvasOrderCalculator>(this);
        _screenData = EZInject.Class.WithParams<IScreenData>(this);
        SetCanvas(ActiveCanvas.No);
        SetBlockRaycast(BlockRaycast.No);
    }

    public virtual void OnEnable()
    {
        FetchEvents();
        ObserveEvents();
        UseEZServiceLocator();
        _screenData.OnEnable();
        _canvasOrderCalculator.OnEnable();
        LateStartSetUp();
    }

    private void LateStartSetUp()
    {
        if(_myDataHub.IsNull()) return;

        if (_myDataHub.SceneAlreadyStarted)
        {
            _activeResolvePopUps = _myDataHub.NoResolvePopUp;
            _gameIsPaused = _myDataHub.GamePaused;
            _canStart = _myDataHub.SceneAlreadyStarted;
            OnHomeScreen = _myDataHub.OnHomeScreen;
            CanAllowKeys = _myDataHub.AllowKeys;
            _inMenu = _myDataHub.InMenu;
        }
    }

    public virtual void UseEZServiceLocator()
    {
        _myDataHub = EZService.Locator.Get<IDataHub>(this);
        _historyTrack = EZService.Locator.Get<IHistoryTrack>(this);
    }
    
    public virtual void OnStart() => _canvasOrderCalculator.OnStart();

    public virtual void FetchEvents()
    {
        SetIsOnHomeScreen = HistoryEvents.Do.Fetch<IOnHomeScreen>();
        DoClearScreen = BranchEvent.Do.Fetch<IClearScreen>();
    }

    public virtual void ObserveEvents()
    {
        PopUpEvents.Do.Subscribe<INoResolvePopUp>(SaveResolvePopUps);
        HistoryEvents.Do.Subscribe<IGameIsPaused>(SaveIfGamePaused);
        BranchEvent.Do.Subscribe<ISetUpStartBranches>(SetUpBranchesOnStart);
        HistoryEvents.Do.Subscribe<IOnStart>(SaveOnStart);
        HistoryEvents.Do.Subscribe<IOnHomeScreen>(SaveIfOnHomeScreen);
        HistoryEvents.Do.Subscribe<IInMenu>(SaveInMenu);
        BranchEvent.Do.Subscribe<IClearScreen>(ClearBranchForFullscreen);
        InputEvents.Do.Subscribe<IAllowKeys>(AllowKeys);
        InputEvents.Do.Subscribe<IActivateBranchOnControlsChange>(WhenControlsChange);
    }

    protected virtual void UnObserveEvents()
    {
        PopUpEvents.Do.Unsubscribe<INoResolvePopUp>(SaveResolvePopUps);
        HistoryEvents.Do.Unsubscribe<IGameIsPaused>(SaveIfGamePaused);
        BranchEvent.Do.Unsubscribe<ISetUpStartBranches>(SetUpBranchesOnStart);
        HistoryEvents.Do.Unsubscribe<IOnStart>(SaveOnStart);
        HistoryEvents.Do.Unsubscribe<IOnHomeScreen>(SaveIfOnHomeScreen);
        HistoryEvents.Do.Unsubscribe<IInMenu>(SaveInMenu);
        BranchEvent.Do.Unsubscribe<IClearScreen>(ClearBranchForFullscreen);
        InputEvents.Do.Unsubscribe<IAllowKeys>(AllowKeys);
        InputEvents.Do.Unsubscribe<IActivateBranchOnControlsChange>(WhenControlsChange);
        _screenData.OnDisable();
        _canvasOrderCalculator.OnDisable();
    }

    public virtual void OnDisable()
    {
        UnObserveEvents();
        SetIsOnHomeScreen = null;
        DoClearScreen = null;
    }
    
    public virtual void OnDestroy()
    {
        UnObserveEvents();
        _myDataHub = null;
        _historyTrack = null;
    }

    protected virtual void WhenControlsChange(IActivateBranchOnControlsChange args)
    {
        if(args.ActiveBranch.NotEqualTo(_myBranch)) return;
        
        if(_myBranch.CanvasIsEnabled)
            _myBranch.DoNotTween();
        _myBranch.MoveToThisBranch();
    }

    public virtual void SetUpGOUIBranch(IGOUIModule module) { }

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
        _myCanvas.enabled = active == ActiveCanvas.Yes;
        
        if (active == ActiveCanvas.Yes)
        {
            AddThisBranch?.Invoke(this);
        }
        else
        {
            RemoveThisBranch?.Invoke(this);
        }
    }

    public virtual void SetBlockRaycast(BlockRaycast active)
    {
        if(!_canStart) return;
        
        if (CanAllowKeys)
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

    protected void ActivateStoredPosition()
    {
        _screenData.RestoreScreen();
        if (_screenData.WasOnHomeScreen)
            InvokeOnHomeScreen(true);
    }

}