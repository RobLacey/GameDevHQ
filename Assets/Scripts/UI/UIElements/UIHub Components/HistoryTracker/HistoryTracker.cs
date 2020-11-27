using System;
using System.Collections.Generic;
using System.Linq;

public interface ITestList //TODO Remove
{
    INode AddNode { get; }
}

public class HistoryTracker : IHistoryTrack, IEventUser, IServiceUser, 
                              IReturnToHome, ITestList, ICancelHoverOverButton
{
    public HistoryTracker(IHub hub)
    {
        _globalCancelAction = hub.Scheme.GlobalCancelAction;
        SubscribeToService();
        OnEnable();
        //TODO *** Demo of Static and instance Injection plus self injection
        // PopUpHistory has example of ***
        var iEJect = EJect.Class.NoParams<IEJect>();
        HistoryListManagement = EJect.Class.WithParams<IHistoryManagement>(this);
        SelectionProcess = iEJect.WithParams<INewSelectionProcess> (this);
        MoveBackInHistory = iEJect.WithParams<IMoveBackInHistory>(this); 
        PopUpHistory = iEJect.WithParams<IManagePopUpHistory>(this);
    }

    //Variables
    private readonly List<INode> _history = new List<INode>();
    private INode _lastSelected;
    private bool _canStart, _isPaused, _activateOnReturnHome;
    private bool _onHomeScreen = true, _noPopUps = true;
    private IBranch _activeBranch;
    private readonly EscapeKey _globalCancelAction;
    
    //Properties
    private IManagePopUpHistory PopUpHistory { get; }
    private IMoveBackInHistory MoveBackInHistory { get; }
    public IHistoryManagement HistoryListManagement { get; }
    private INewSelectionProcess SelectionProcess { get; }
    public bool ActivateOnReturnHome => _activateOnReturnHome;
    private void SaveOnHomScreen(IOnHomeScreen args) => _onHomeScreen = args.OnHomeScreen;
    private void SaveIsGamePaused(IGameIsPaused args) => _isPaused = args.GameIsPaused;
    private void SaveActiveBranch(IActiveBranch args) => _activeBranch = args.ActiveBranch;
    private void NoPopUps(INoPopUps args) => _noPopUps = args.NoActivePopUps;
    public bool NoHistory => _history.Count == 0;
    public bool IsPaused => _isPaused;

    //Events
    private Action<IReturnToHome> ReturnHome { get; } = EVent.Do.FetchEVent<IReturnToHome>();
    private Action<ICancelHoverOverButton> CancelHoverToActivate { get; }
        = EVent.Do.FetchEVent<ICancelHoverOverButton>();
    
    //TODO Remove Test Rig
    private Action<ITestList> AddANode { get; } = EVent.Do.FetchEVent<ITestList>();
    
    public INode AddNode { get; private set; }
    
    public void AddNodeToTestRunner(INode node)
    {
        AddNode = node;
        AddANode?.Invoke(this);
    }
    
    //Main
    public void ObserveEvents()
    {
        EVent.Do.Subscribe<IOnStart>(SetCanStart);
        EVent.Do.Subscribe<IActiveBranch>(SaveActiveBranch);
        EVent.Do.Subscribe<IOnHomeScreen>(SaveOnHomScreen);
        EVent.Do.Subscribe<IGameIsPaused>(SaveIsGamePaused);
        EVent.Do.Subscribe<INoPopUps>(NoPopUps);
        EVent.Do.Subscribe<IHotKeyPressed>(SetFromHotkey);
        EVent.Do.Subscribe<IDisabledNode>(CloseNodesAfterDisabledNode);
        EVent.Do.Subscribe<ISwitchGroupPressed>(SwitchGroupPressed);
        EVent.Do.Subscribe<IInMenu>(SwitchToGame);
        EVent.Do.Subscribe<ICancelPopUp>(CancelPopUpFromButton);
    }

    public void RemoveFromEvents()
    {
        EVent.Do.Unsubscribe<IOnStart>(SetCanStart);
        EVent.Do.Unsubscribe<IActiveBranch>(SaveActiveBranch);
        EVent.Do.Unsubscribe<IOnHomeScreen>(SaveOnHomScreen);
        EVent.Do.Unsubscribe<IGameIsPaused>(SaveIsGamePaused);
        EVent.Do.Unsubscribe<INoPopUps>(NoPopUps);
        EVent.Do.Unsubscribe<IHotKeyPressed>(SetFromHotkey);
        EVent.Do.Unsubscribe<IDisabledNode>(CloseNodesAfterDisabledNode);
        EVent.Do.Unsubscribe<ISwitchGroupPressed>(SwitchGroupPressed);
        EVent.Do.Unsubscribe<IInMenu>(SwitchToGame);
        EVent.Do.Unsubscribe<ICancelPopUp>(CancelPopUpFromButton);
    }
    
    public void SubscribeToService() => ServiceLocator.Bind<ICancel>(new UICancel(_globalCancelAction));

    public void OnEnable() => ObserveEvents();

    public void OnDisable()
    {
        ServiceLocator.Remove<ICancel>();
        RemoveFromEvents();
    }
    
    private void SetCanStart(IOnStart onStart) => _canStart = true;

    //Main
    public void SetSelected(INode node)
    {
        if(!_canStart) return;
        if(node.DontStoreTheseNodeTypesInHistory) return;
        
        _lastSelected = SelectionProcess.NewNode(node)
                                       .CurrentHistory(_history)
                                       .LastSelectedNode(_lastSelected)
                                       .Run();
    }

    private void CloseNodesAfterDisabledNode(IDisabledNode args)
        => HistoryListManagement.CurrentHistory(_history)
                                 .CloseToThisPoint(args.ThisNodeIsDisabled)
                                 .Run();

    public void BackOneLevel() 
        => _lastSelected = MoveBackInHistory.AddHistory(_history)
                                            .ActiveBranch(_activeBranch)
                                            .IsOnHomeScreen(_onHomeScreen)
                                            .BackOneLevelProcess();

    private void SwitchToGame(IInMenu args)
    {
        if (args.InTheMenu && _canStart)
        {
            BackToHome();
        }
    }

    public void BackToHome() 
        => _lastSelected = MoveBackInHistory.AddHistory(_history)
                                            .ActiveBranch(_activeBranch)
                                            .BackToHomeProcess();

    public void DoCancelHoverToActivate() => CancelHoverToActivate?.Invoke(this);

    private void SwitchGroupPressed(ISwitchGroupPressed args)
    {
        if (!_onHomeScreen && _activeBranch.IsInternalBranch())
        {
            BackOneLevel();
        }
        else if(_onHomeScreen)
        {
            HistoryListManagement.CurrentHistory(_history)
                                 .ClearAllHistory();
        }
    }
    
    private void SetFromHotkey(IHotKeyPressed args)
    {
        if (args.MyBranch.ScreenType != ScreenType.FullScreen && !_onHomeScreen)
        {
            BackToHomeScreen(ActivateNodeOnReturnHome.No);
        }
        
        HistoryListManagement.IgnoreHotKeyParent(args.ParentNode)
                             .CurrentHistory(_history)
                             .ClearAllHistory();

        AddNodeToTestRunner(args.ParentNode);
        
        _history.Add(args.ParentNode);
        _lastSelected = args.ParentNode;
    }

    public void BackToHomeScreen(ActivateNodeOnReturnHome activate)
    {
        _activateOnReturnHome = activate == ActivateNodeOnReturnHome.Yes;
        ReturnHome?.Invoke(this);
    }

    public void MoveToLastBranchInHistory()
    {
        if (!_noPopUps && !_isPaused)
        {
            PopUpHistory.MoveToNextPopUp();
            
            return;
        }
        if (_history.Count == 0 || _isPaused)
        {
            IfPausedOrNoHistory();
        }
        else
        {
            _history.Last().HasChildBranch.MoveToBranchWithoutTween();
        }
    }

    private void IfPausedOrNoHistory()
    {
        if (_isPaused)
        {
            _activeBranch.MoveToBranchWithoutTween();
        }
        else
        {
            BackToHomeScreen(ActivateNodeOnReturnHome.Yes);
        }
    }
    
    private void CancelPopUpFromButton(ICancelPopUp popUpToCancel) => PopUpHistory.HandlePopUps(popUpToCancel.MyBranch);

    public void CheckForPopUpsWhenCancelPressed(Action endOfCancelAction)
    {
        PopUpHistory.IsGamePaused(_isPaused)
                    .NoPopUpAction(endOfCancelAction)
                    .DoPopUpCheckAndHandle();
    }
}
