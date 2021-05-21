using System;
using System.Collections.Generic;
using System.Linq;
using EZ.Events;
using EZ.Service;
using UIElements.Input_System;
using UnityEngine;

public class HistoryTracker : IHistoryTrack, IEZEventUser, IReturnToHome, IHistoryData, 
                              IEZEventDispatcher, IReturnHomeGroupIndex
{
    public HistoryTracker()
    {
        HistoryListManagement = EZInject.Class.WithParams<IHistoryManagement>(this);
        SelectionProcess = EZInject.Class.WithParams<INewSelectionProcess> (this);
        MoveBackInHistory = EZInject.Class.WithParams<IMoveBackInHistory>(this);
        PopUpHistory = EZInject.Class.WithParams<IManagePopUpHistory>(this);
        _multiSelectSystem = EZInject.Class.WithParams<IMultiSelect>(this);
    }

    //Variables
    private readonly List<INode> _history = new List<INode>();
    private INode _lastSelected;
    private bool _canStart, _isPaused;
    private bool _onHomeScreen = true, _noPopUps = true;
    private IBranch _activeBranch;
    private ICancel _cancel;
    private IMultiSelect _multiSelectSystem;

    //Properties
    private IManagePopUpHistory PopUpHistory { get; }
    private IMoveBackInHistory MoveBackInHistory { get; }
    public IHistoryManagement HistoryListManagement { get; }
    private INewSelectionProcess SelectionProcess { get; }
    private void SaveOnHomScreen(IOnHomeScreen args) => _onHomeScreen = args.OnHomeScreen;
    private void SaveIsGamePaused(IGameIsPaused args) => _isPaused = args.GameIsPaused;
    private void SaveActiveBranch(IActiveBranch args) => _activeBranch = args.ActiveBranch;
    private void NoPopUps(INoPopUps args) => _noPopUps = args.NoActivePopUps;
    public INode NodeToUpdate { get; private set; }
    public bool NoHistory => _history.Count == 0 && !_multiSelectSystem.MultiSelectActive;
    public bool IsPaused => _isPaused;
    public INode TargetNode { get; set; }


    //Events
    private Action<IReturnToHome> ReturnHome { get; set; }
    private Action<IReturnHomeGroupIndex> ReturnHomeGroupBranch { get; set; }
    
    //TODO Remove Test Rig
    private Action<IHistoryData> DoAddANode { get; set; }
    
    public void UpdateHistoryData(INode node)
    {
        NodeToUpdate = node;
        DoAddANode?.Invoke(this);
    }
    
    //Main
    public void OnEnable()
    {
        AddService();
        FetchEvents();
        ObserveEvents();
        PopUpHistory.OnEnable();
        _multiSelectSystem.OnEnable();
    }

    public void AddService() => EZService.Locator.AddNew<IHistoryTrack>(this);

    public void OnRemoveService() { }
   
    public void FetchEvents()
    {
        ReturnHome = HistoryEvents.Do.Fetch<IReturnToHome>();
        ReturnHomeGroupBranch = HistoryEvents.Do.Fetch<IReturnHomeGroupIndex>();
        DoAddANode = HistoryEvents.Do.Fetch<IHistoryData>();
    }

    public void ObserveEvents()
    {
        HistoryEvents.Do.Subscribe<IOnStart>(SetCanStart);
        HistoryEvents.Do.Subscribe<IActiveBranch>(SaveActiveBranch);
        HistoryEvents.Do.Subscribe<IOnHomeScreen>(SaveOnHomScreen);
        HistoryEvents.Do.Subscribe<IGameIsPaused>(SaveIsGamePaused);
        HistoryEvents.Do.Subscribe<ISelectedNode>(SetSelected);
        HistoryEvents.Do.Subscribe<IDisabledNode>(CloseNodesAfterDisabledNode);
        HistoryEvents.Do.Subscribe<IInMenu>(SwitchToGame);
        PopUpEvents.Do.Subscribe<INoPopUps>(NoPopUps);
        InputEvents.Do.Subscribe<IHotKeyPressed>(SetFromHotkey);
        InputEvents.Do.Subscribe<ISwitchGroupPressed>(SwitchGroupPressed);
        CancelEvents.Do.Subscribe<ICancelPopUp>(CancelPopUpFromButton);
    }

    public void GOUIBranchHasClosed(IBranch branchToClose, IGOUIModule nextModule)
    {
        if (_activeBranch == branchToClose || _activeBranch.MyParentBranch == branchToClose)
        {
            Debug.Log("GOUI Object");
            nextModule.SwitchEnter();
        }
        
        if(!_history.Contains(branchToClose.LastSelected)) return;

        if (_multiSelectSystem.MultiSelectActive)
        {
            Debug.Log($"MultiSelect : {branchToClose} Removed");
            _multiSelectSystem.RemoveFromMultiSelect(_history, branchToClose.LastSelected);
            branchToClose.LastSelected.DeactivateNode();
        }
        else
        {
            Debug.Log($"History Tracker : {branchToClose} Removed");
            CheckListsForBranch(branchToClose.LastSelected);
        }
        
        if(_history.Count > 0)
        {
            _history.Last().MyBranch.MoveToThisBranch();
        }
        else
        {
            ReturnHomeGroupBranch?.Invoke(this);
            TargetNode.MyBranch.MoveToThisBranch();
        }
    }

    private void CheckListsForBranch(INode targetBranchLastSelected)
    {
        HistoryListManagement.CurrentHistory(_history)
                             .CloseThisLevel(targetBranchLastSelected);
        targetBranchLastSelected.DeactivateNode();
    }

    private void SetCanStart(IOnStart onStart) => _canStart = true;

    //Main
    private void SetSelected(ISelectedNode newNode)
    {
        if(!_canStart) return;
        if(newNode.UINode.CanNotStoreNodeInHistory) return;
        
        if (IfMultiSelectPressed(newNode)) return;
       
        if(_multiSelectSystem.MultiSelectActive)
        {
           ClearAllHistory();
        }        
        AddNewSelectedNode(newNode);
    }

    private bool IfMultiSelectPressed(ISelectedNode newNode)
    {
        if (_multiSelectSystem.MultiSelectPressed(_history, newNode.UINode))
        {
            _lastSelected = newNode.UINode;
            return true;
        }
        return false;
    }

    private void AddNewSelectedNode(ISelectedNode newNode)
    {
        _lastSelected = SelectionProcess.NewNode(newNode.UINode)
                                        .CurrentHistory(_history)
                                        .LastSelectedNode(_lastSelected)
                                        .Run();
    }

    private void CloseNodesAfterDisabledNode(IDisabledNode args)
        => HistoryListManagement.CurrentHistory(_history)
                                .CloseToThisPoint(args.ThisIsTheDisabledNode)
                                .Run();

    public void BackOneLevel()
    {
        if (_multiSelectSystem.MultiSelectActive)
        {
            _lastSelected = _history.First();
            ClearAllHistory();
        }
        else
        {
            _lastSelected = MoveBackInHistory.AddHistory(_history)
                                             .ActiveBranch(_activeBranch)
                                             .IsOnHomeScreen(_onHomeScreen)
                                             .BackOneLevelProcess();
        }
    }

    private void SwitchToGame(IInMenu args)
    {
        if (!args.InTheMenu && _canStart)
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

    private void SwitchGroupPressed(ISwitchGroupPressed args)
    {
        if (!_onHomeScreen && _activeBranch.IsInternalBranch())
        {
            BackOneLevel();
        }
        else
        {
            ClearAllHistory();
        }
    }

    private void ClearAllHistory()
    {
        _multiSelectSystem.ClearMultiSelect();

        HistoryListManagement.CurrentHistory(_history)
                             .ClearAllHistory();
    }
    
    private void SetFromHotkey(IHotKeyPressed args)
    {
        HotKeyReturnsToHomeScreen(args.MyBranch.ScreenType);
        
        ClearAllHistory();
        _lastSelected = args.ParentNode;
    }
    
    private void HotKeyReturnsToHomeScreen(ScreenType hotKeyScreenType)
    {
        if (hotKeyScreenType != ScreenType.FullScreen && !_onHomeScreen)
        {
            BackToHomeScreen();
        }
    }

    public void BackToHomeScreen() => ReturnHome?.Invoke(this);

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
            BackToHomeScreen();
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
