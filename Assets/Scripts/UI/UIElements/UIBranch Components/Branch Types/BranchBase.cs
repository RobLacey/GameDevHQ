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
    protected bool InMenu => _myDataHub.InMenu;
    protected bool CanStart => _myDataHub.SceneStarted;
    protected bool GameIsPaused => _myDataHub.GamePaused;
    protected bool NoResolvePopUps => _myDataHub.NoResolvePopUp;
    public bool OnHomeScreen => _myDataHub.OnHomeScreen;
    public IBranch IgnoreThisBranch => _myBranch;
    public IBranch MyBranch => _myBranch;
    public ScreenType MyScreenType { get; }
    protected bool CanAllowKeys => _myDataHub.AllowKeys;


    protected void InvokeOnHomeScreen(bool onHome)
    {
       _myDataHub.SetOnHomeScreen(onHome);
        SetIsOnHomeScreen?.Invoke(this);
    }
    
    private void InvokeDoClearScreen() => DoClearScreen?.Invoke(this);
    protected virtual void SaveInMenu(IInMenu args) { }
    protected virtual void SaveIfOnHomeScreen(IOnHomeScreen args) { }

    private void AllowKeys(IAllowKeys args)
    {
        if (!CanStart && CanAllowKeys)
        {
            _myCanvasGroup.blocksRaycasts = false;
            return;
        }
        var blockRaycast = CanAllowKeys ? BlockRaycast.No : BlockRaycast.Yes;
        SetBlockRaycast(blockRaycast);
    }

    //Main
    public virtual void OnAwake()
    {
        _canvasOrderCalculator = EZInject.Class.WithParams<ICanvasOrderCalculator>(this);
        _screenData = EZInject.Class.WithParams<IScreenData>(this);
        _myCanvas.enabled = false;
        _myCanvasGroup.blocksRaycasts = false;
    }

    public virtual void OnEnable()
    {
        FetchEvents();
        ObserveEvents();
        UseEZServiceLocator();
        _screenData.OnEnable();
        _canvasOrderCalculator.OnEnable();
        LateStartUp();
    }

    private void LateStartUp()
    {
        if (_myDataHub.IsNull()) return;

        if (_myDataHub.SceneStarted)
        {
            _canvasOrderCalculator.OnStart();
        }
    }

    public virtual void UseEZServiceLocator()
    {
        _myDataHub = EZService.Locator.Get<IDataHub>(this);
        _historyTrack = EZService.Locator.Get<IHistoryTrack>(this);
    }
    
    public virtual void OnStart()
    {
        if(_myDataHub.SceneStarted) return;
        _canvasOrderCalculator.OnStart();
    }

    public virtual void FetchEvents()
    {
        SetIsOnHomeScreen = HistoryEvents.Do.Fetch<IOnHomeScreen>();
        DoClearScreen = BranchEvent.Do.Fetch<IClearScreen>();
    }

    public virtual void ObserveEvents()
    {
        BranchEvent.Do.Subscribe<ISetUpStartBranches>(SetUpBranchesOnStart);
        HistoryEvents.Do.Subscribe<IOnHomeScreen>(SaveIfOnHomeScreen);
        BranchEvent.Do.Subscribe<IClearScreen>(ClearBranchForFullscreen);
        InputEvents.Do.Subscribe<IAllowKeys>(AllowKeys);
    }

    public virtual void UnObserveEvents()
    {
        BranchEvent.Do.Unsubscribe<ISetUpStartBranches>(SetUpBranchesOnStart);
        HistoryEvents.Do.Unsubscribe<IOnHomeScreen>(SaveIfOnHomeScreen);
        BranchEvent.Do.Unsubscribe<IClearScreen>(ClearBranchForFullscreen);
        InputEvents.Do.Unsubscribe<IAllowKeys>(AllowKeys);
        _screenData.OnDisable();
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

    public virtual void SetUpGOUIBranch(IGOUIModule module) { }
    public virtual IGOUIModule ReturnGOUIModule() => null;

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
    
    public virtual void EndOfBranchStart()
    {
        if(!CanStart) return;
        SetBlockRaycast(BlockRaycast.Yes);
    }

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