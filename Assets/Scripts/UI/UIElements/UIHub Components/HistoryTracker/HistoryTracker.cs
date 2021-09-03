using System;
using System.Collections.Generic;
using System.Linq;
using EZ.Events;
using EZ.Service;
using UIElements.Input_System;
using UnityEngine;

public class HistoryTracker : IHistoryTrack, IEZEventUser, IReturnToHome, IStoreNodeHistoryData, 
                              IEZEventDispatcher, IReturnHomeGroupIndex, IServiceUser
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
    private ICancel _cancel;
    private IMultiSelect _multiSelectSystem;
    private IDataHub _myDataHub;

    //Properties
    private IManagePopUpHistory PopUpHistory { get; }
    private IMoveBackInHistory MoveBackInHistory { get; }
    public IHistoryManagement HistoryListManagement { get; }
    private INewSelectionProcess SelectionProcess { get; }
    public INode NodeToUpdate { get; private set; }
    public bool NoHistory => _history.Count == 0 && !_multiSelectSystem.MultiSelectActive;
    public bool IsPaused => _myDataHub.GamePaused;
    private bool CanStart => _myDataHub.SceneStarted;
    private bool OnHomeScreen => _myDataHub.OnHomeScreen;
    private bool NoPopUps => _myDataHub.NoPopups;
    private IBranch ActiveBranch => _myDataHub.ActiveBranch;
    public INode TargetNode { get; set; }


    //Events
    private Action<IReturnToHome> ReturnHome { get; set; }
    private Action<IReturnHomeGroupIndex> ReturnHomeGroupBranch { get; set; }
    
    //TODO Remove Test Rig
    private Action<IStoreNodeHistoryData> DoAddANode { get; set; }
    
    public void UpdateHistoryData(INode node)
    {
        NodeToUpdate = node;
        DoAddANode?.Invoke(this);
    }
    
    //Main
    public void OnEnable()
    {
        UseEZServiceLocator();
        AddService();
        FetchEvents();
        ObserveEvents();
        PopUpHistory.OnEnable();
        _multiSelectSystem.OnEnable();
    }

    
    public void UseEZServiceLocator() => _myDataHub = EZService.Locator.Get<IDataHub>(this);

    public void AddService() => EZService.Locator.AddNew<IHistoryTrack>(this);

    public void OnRemoveService() { }
   
    public void FetchEvents()
    {
        ReturnHome = HistoryEvents.Do.Fetch<IReturnToHome>();
        ReturnHomeGroupBranch = HistoryEvents.Do.Fetch<IReturnHomeGroupIndex>();
        DoAddANode = HistoryEvents.Do.Fetch<IStoreNodeHistoryData>();
    }

    public void ObserveEvents()
    {
        HistoryEvents.Do.Subscribe<ISelectedNode>(SetSelected);
        HistoryEvents.Do.Subscribe<IDisabledNode>(CloseNodesAfterDisabledNode);
        HistoryEvents.Do.Subscribe<IInMenu>(SwitchToGame);
        InputEvents.Do.Subscribe<IHotKeyPressed>(SetFromHotkey);
        InputEvents.Do.Subscribe<ISwitchGroupPressed>(SwitchGroupPressed);
        CancelEvents.Do.Subscribe<ICancelPopUp>(CancelPopUpFromButton);
    }

    public void UnObserveEvents() { }

    public void GOUIBranchHasClosed(IBranch branchToClose, bool noGOUILeft)
    {
        if(!branchToClose.IsInGameBranch()) return;
        
        NoGOUIAndWasActiveBranch(noGOUILeft, ActiveBranch == branchToClose);
        
        if(!_history.Contains(branchToClose.LastSelected)) return;

        CheckListsAndRemove(branchToClose);
        
        if(_history.Count > 0)
            _history.Last().MyBranch.MoveToThisBranch();
    }

    private void NoGOUIAndWasActiveBranch(bool noGOUILeft, bool activeBranch)
    {
        if (!noGOUILeft || !activeBranch) return;
        ReturnToNextHomeGroup();
    }

    public void ReturnToNextHomeGroup()
    {
        ReturnHomeGroupBranch?.Invoke(this);
        TargetNode.MyBranch.MoveToThisBranch();
    }

    private void CheckListsAndRemove(IBranch branchToClose)
    {
        if (_multiSelectSystem.MultiSelectActive)
        {
            _multiSelectSystem.RemoveFromMultiSelect(_history, branchToClose.LastSelected);
            branchToClose.LastSelected.DeactivateNode();
        }
        else
        {
            HistoryListManagement.CurrentHistory(_history)
                                 .ClearGOUIBranchFromHistory(branchToClose.LastSelected);
        }
    }

    //Main
    private void SetSelected(ISelectedNode newNode)
    {
        if(newNode.UINode.IsNull() || !CanStart) return;
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
                                             .ActiveBranch(ActiveBranch)
                                             .IsOnHomeScreen(OnHomeScreen)
                                             .BackOneLevelProcess();
        }
    }

    private void SwitchToGame(IInMenu args)
    {
        if (!args.InTheMenu && CanStart)
        {
            BackToHome();
        }
    }

    public void BackToHome()
    {
        if(_history.Count == 0) return;
        _lastSelected = MoveBackInHistory.AddHistory(_history)
                                         .ActiveBranch(ActiveBranch)
                                         .BackToHomeProcess();
    }

    private void SwitchGroupPressed(ISwitchGroupPressed args)
    {
        if (!OnHomeScreen && ActiveBranch.IsInternalBranch())
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
        if (hotKeyScreenType != ScreenType.FullScreen && !OnHomeScreen)
        {
            BackToHomeScreen();
        }
    }

    public void BackToHomeScreen() => ReturnHome?.Invoke(this);

    public void MoveToLastBranchInHistory()
    {
        if (!NoPopUps && !IsPaused)
        {
            PopUpHistory.MoveToNextPopUp();
            return;
        }
        
        if (_history.Count == 0 || IsPaused)
        {
            BackToHomeScreen();
        }
        else
        {
            if(_history.Last().HasChildBranch.IsNotNull())
            {
                _history.Last().HasChildBranch.DoNotTween();
                _history.Last().HasChildBranch.MoveToThisBranch();
            }
            else
            {
                _history.Last().MyBranch.DoNotTween();
                _history.Last().MyBranch.MoveToThisBranch();
            }
        }
    }
    
    private void CancelPopUpFromButton(ICancelPopUp popUpToCancel) => PopUpHistory.HandlePopUps(popUpToCancel.MyBranch);

    public void CheckForPopUpsWhenCancelPressed(Action endOfCancelAction)
    {
        PopUpHistory.NoPopUpAction(endOfCancelAction)
                    .DoPopUpCheckAndHandle();
    }
}
