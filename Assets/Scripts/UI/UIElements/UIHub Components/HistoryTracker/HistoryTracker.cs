using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public interface ITestList //TODO Remove
{
    INode AddNode { get; }
}

public class HistoryTracker : IHistoryTrack, IEventUser, IEServUser, 
                              IReturnToHome, ITestList, ICancelHoverOverButton, IEventDispatcher
{
    public HistoryTracker(IHub hub)
    {
        _globalCancelAction = hub.Scheme.GlobalCancelAction;
        UseEServLocator();
        HistoryListManagement = EJect.Class.WithParams<IHistoryManagement>(this);
        SelectionProcess = EJect.Class.WithParams<INewSelectionProcess> (this);
        MoveBackInHistory = EJect.Class.WithParams<IMoveBackInHistory>(this); 
        PopUpHistory = EJect.Class.WithParams<IManagePopUpHistory>(this);
    }

    //Variables
    private readonly List<INode> _history = new List<INode>();
    private INode _lastSelected;
    private bool _canStart, _isPaused, _activateOnReturnHome;
    private bool _onHomeScreen = true, _noPopUps = true;
    private IBranch _activeBranch;
    private readonly EscapeKey _globalCancelAction;
    private ICancel _cancel;
    
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
    private Action<IReturnToHome> ReturnHome { get; set; }
    private Action<ICancelHoverOverButton> CancelHoverToActivate { get; set; }
    
    //TODO Remove Test Rig
    private Action<ITestList> DoAddANode { get; set; }
    
    public INode AddNode { get; private set; }
    
    public void AddNodeToTestRunner(INode node)
    {
        AddNode = node;
        DoAddANode?.Invoke(this);
    }
    
    //Main
    public void OnEnable()
    {
        FetchEvents();
        ObserveEvents();
        _cancel.OnEnable();
        PopUpHistory.OnEnable();
    }

    public void OnDisable()
    {
        RemoveEvents();
        _cancel.OnDisable();
        PopUpHistory.OnDisable();
    }
    
    public void FetchEvents()
    {
        ReturnHome = EVent.Do.Fetch<IReturnToHome>();
        CancelHoverToActivate = EVent.Do.Fetch<ICancelHoverOverButton>();
        DoAddANode = EVent.Do.Fetch<ITestList>();
    }


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

    public void RemoveEvents()
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

    public void UseEServLocator()
    {
        _cancel = new UICancel(_globalCancelAction);
        EServ.Locator.AddNew(_cancel);
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
    {
        if(_history.Count == 0) return;
        _lastSelected = MoveBackInHistory.AddHistory(_history)
                                         .ActiveBranch(_activeBranch)
                                         .BackToHomeProcess();
    }
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
