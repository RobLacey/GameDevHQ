using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public interface ITestList //TODO Remove
{
    INode AddNode { get; }
}

public class HistoryTracker : IHistoryTrack, IEventUser, 
                              IReturnToHome, ITestList, ICancelHoverOverButton, IEventDispatcher
{
    public HistoryTracker()
    {
        HistoryListManagement = EJect.Class.WithParams<IHistoryManagement>(this);
        SelectionProcess = EJect.Class.WithParams<INewSelectionProcess> (this);
        MoveBackInHistory = EJect.Class.WithParams<IMoveBackInHistory>(this);
        PopUpHistory = EJect.Class.WithParams<IManagePopUpHistory>(this);
    }

    //Variables
    private readonly List<INode> _history = new List<INode>();
    private INode _lastSelected;
    private bool _canStart, _isPaused;
    private bool _onHomeScreen = true, _noPopUps = true;
    private IBranch _activeBranch;
    private ICancel _cancel;

    //Properties
    private IManagePopUpHistory PopUpHistory { get; }
    private IMoveBackInHistory MoveBackInHistory { get; }
    public IHistoryManagement HistoryListManagement { get; }
    private INewSelectionProcess SelectionProcess { get; }
    public ActivateNodeOnReturnHome ActivateOnReturnHome { get; private set; }
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
        PopUpHistory.OnEnable();
    }
    
    public void OnDisable() { }
   
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
        EVent.Do.Subscribe<ISelectedNode>(SetSelected);
        EVent.Do.Subscribe<IClearAll>(ClearAll);
    }

    private void SetCanStart(IOnStart onStart) => _canStart = true;

    //Main
    private void SetSelected(ISelectedNode newNode)
    {
        if(!_canStart) return;
        if(newNode.UINode.CanStoreNodeInHistory) return;
        
        _lastSelected = SelectionProcess.NewNode(newNode.UINode)
                                       .CurrentHistory(_history)
                                       .LastSelectedNode(_lastSelected)
                                       .Run();
    }

    private void CloseNodesAfterDisabledNode(IDisabledNode args)
        => HistoryListManagement.CurrentHistory(_history)
                                 .CloseToThisPoint(args.ToThisDisabledNode)
                                 .Run();

    public void BackOneLevel() 
        => _lastSelected = MoveBackInHistory.AddHistory(_history)
                                            .ActiveBranch(_activeBranch)
                                            .IsOnHomeScreen(_onHomeScreen)
                                            .BackOneLevelProcess();

    private void SwitchToGame(IInMenu args)
    {
        if (!args.InTheMenu && _canStart)
        {
            BackToHome();
        }
    }

    public void ClearAll(IClearAll args) => BackToHome();

    public void BackToHome()
    {
        if(_history.Count == 0) return;
        _lastSelected = MoveBackInHistory.AddHistory(_history)
                                         .ActiveBranch(_activeBranch)
                                         .BackToHomeProcess();
    }

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
        HotKeyReturnsToHomeScreen(args.MyBranch.ScreenType);
        
        HistoryListManagement.CurrentHistory(_history)
                             .ClearAllHistory();
        _lastSelected = args.ParentNode;
    }

    private void HotKeyReturnsToHomeScreen(ScreenType hotKeyScreenType)
    {
        if (hotKeyScreenType != ScreenType.FullScreen && !_onHomeScreen)
        {
            BackToHomeScreen(ActivateNodeOnReturnHome.No);
        }
    }

    public void BackToHomeScreen(ActivateNodeOnReturnHome activate)
    {
        ActivateOnReturnHome = activate;
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
            _history.Last().HasChildBranch.DoNotTween();
            _history.Last().HasChildBranch.MoveToThisBranch();
        }
    }

    private void IfPausedOrNoHistory()
    {
        if (_isPaused)
        {
            _activeBranch.MoveToThisBranch();
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
