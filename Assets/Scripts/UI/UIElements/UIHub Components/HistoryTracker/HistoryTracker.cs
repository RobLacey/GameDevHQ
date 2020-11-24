using System;
using System.Collections.Generic;
using System.Linq;

public interface ITestList
{
    INode AddNode { get; }
}

public class HistoryTracker : IHistoryTrack, IEventUser, IServiceUser, IReturnToHome, ITestList, ICancelHoverOverButton
{
    public HistoryTracker(EscapeKey globalCancelAction)
    {
        _globalCancelAction = globalCancelAction;
        SubscribeToService();
        OnEnable();
        HistoryListManagement = new HistoryListManagement(this);
        SelectionProcess = new NewSelectionProcess(this, HistoryListManagement);
        MoveBackInHistory = new MoveBackInHistory(this, HistoryListManagement);
        PopUpHistory = new ManagePopUpHistory(this, new PopUpController());
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
    private IHistoryManagement HistoryListManagement { get; }
    private INewSelectionProcess SelectionProcess { get; }
    public bool ActivateOnReturnHome => _activateOnReturnHome;
    private void SaveOnHomScreen(IOnHomeScreen args) => _onHomeScreen = args.OnHomeScreen;
    private void SaveIsGamePaused(IGameIsPaused args) => _isPaused = args.GameIsPaused;
    private void SaveActiveBranch(IActiveBranch args) => _activeBranch = args.ActiveBranch;
    private void NoPopUps(INoPopUps args) => _noPopUps = args.NoActivePopUps;
    public bool NoHistory => _history.Count == 0;
    public bool IsPaused => _isPaused;

    //Events
    private static CustomEvent<IReturnToHome> ReturnedToHome { get; } = new CustomEvent<IReturnToHome>();
    private static CustomEvent<ICancelHoverOverButton> CancelHoverToActivate { get; } 
        = new CustomEvent<ICancelHoverOverButton>();
    
    //TODO Remove Test Rig
    private static CustomEvent<ITestList> AddANode { get; } = new CustomEvent<ITestList>();
    public INode AddNode { get; private set; }
    
    public void AddNodeToTestRunner(INode node)
    {
        AddNode = node;
        AddANode?.RaiseEvent(this);
    }
    
    //Main
    public void ObserveEvents()
    {
        EventLocator.Subscribe<IOnStart>(SetCanStart, this);
        EventLocator.Subscribe<IActiveBranch>(SaveActiveBranch, this);
        EventLocator.Subscribe<IOnHomeScreen>(SaveOnHomScreen, this);
        EventLocator.Subscribe<IGameIsPaused>(SaveIsGamePaused, this);
        EventLocator.Subscribe<INoPopUps>(NoPopUps, this);
        EventLocator.Subscribe<IHotKeyPressed>(SetFromHotkey, this);
        EventLocator.Subscribe<IDisabledNode>(CloseNodesAfterDisabledNode, this);
        EventLocator.Subscribe<ISwitchGroupPressed>(SwitchGroupPressed, this);
        EventLocator.Subscribe<IInMenu>(SwitchToGame, this);
        EventLocator.Subscribe<ICancelPopUp>(CancelPopUpFromButton, this);
    }

    public void RemoveFromEvents()
    {
        EventLocator.Unsubscribe<IOnStart>(SetCanStart);
        EventLocator.Unsubscribe<IActiveBranch>(SaveActiveBranch);
        EventLocator.Unsubscribe<IOnHomeScreen>(SaveOnHomScreen);
        EventLocator.Unsubscribe<IGameIsPaused>(SaveIsGamePaused);
        EventLocator.Unsubscribe<INoPopUps>(NoPopUps);
        EventLocator.Unsubscribe<IHotKeyPressed>(SetFromHotkey);
        EventLocator.Unsubscribe<IDisabledNode>(CloseNodesAfterDisabledNode);
        EventLocator.Unsubscribe<ISwitchGroupPressed>(SwitchGroupPressed);
        EventLocator.Unsubscribe<IInMenu>(SwitchToGame);
        EventLocator.Unsubscribe<ICancelPopUp>(CancelPopUpFromButton);
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

    public void DoCancelHoverToActivate() => CancelHoverToActivate?.RaiseEvent(this);

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
        ReturnedToHome?.RaiseEvent(this);
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
