using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public interface ITestList
{
    UINode AddNode { get; }
}

public class HistoryTracker : IHistoryTrack, IEventUser, IReturnToHome, ITestList
{
    public HistoryTracker() => OnEnable();

    //Variables
    private readonly List<UINode> _history = new List<UINode>();
    private UINode _lastSelected;
    private bool _canStart, _isPaused;
    private bool _onHomScreen = true, _noPopUps = true;
    private UIBranch _activeBranch;
    private IPopUpController _popUpController;

    public UINode AddNode { get; private set; }
    private UIBranch PopUpToRemove { get; set; }
    public bool ActivateBranchOnReturnHome { get; private set; }

    private void SaveOnHomScreen(IOnHomeScreen args) => _onHomScreen = args.OnHomeScreen;
    private void SaveIsGamePaused(IGameIsPaused args) => _isPaused = args.GameIsPaused;
    private void SaveActiveBranch(IActiveBranch args) => _activeBranch = args.ActiveBranch;
    private void NoPopUps(INoPopUps args) => _noPopUps = args.NoActivePopUps;
    
    //Events
    private static CustomEvent<IReturnToHome> ReturnedToHome { get; } = new CustomEvent<IReturnToHome>();
    private static CustomEvent<ITestList> AddANode { get; } = new CustomEvent<ITestList>();
    
    public void ObserveEvents()
    {
        EventLocator.Subscribe<IOnStart>(SetCanStart, this);
        EventLocator.Subscribe<IActiveBranch>(SaveActiveBranch, this);
        EventLocator.Subscribe<IOnHomeScreen>(SaveOnHomScreen, this);
        EventLocator.Subscribe<IGameIsPaused>(SaveIsGamePaused, this);
        EventLocator.Subscribe<INoPopUps>(NoPopUps, this);
    }

    public void RemoveFromEvents()
    {
        EventLocator.Unsubscribe<IOnStart>(SetCanStart);
        EventLocator.Unsubscribe<IActiveBranch>(SaveActiveBranch);
        EventLocator.Unsubscribe<IOnHomeScreen>(SaveOnHomScreen);
        EventLocator.Unsubscribe<IGameIsPaused>(SaveIsGamePaused);
        EventLocator.Unsubscribe<INoPopUps>(NoPopUps);
    }

    public void OnEnable()
    {
        _popUpController = new PopUpController();
        ObserveEvents();
    }

    public void OnDisable()
    {
        ServiceLocator.RemoveService<IPopUpController>();
        RemoveFromEvents();
    }
    
    private void SetCanStart(IOnStart onStart) => _canStart = true;

    //Main
    public void SetSelected(INode node)
    {
        if(!_canStart) return;
        if(node.ReturnNode.DontStoreTheseNodeTypesInHistory) return;
        SetNullLastSelected(node);
        
        if (_history.Contains(node.ReturnNode))
        {
            CloseAllChildNodesAfterPoint(node);
            SetLastSelectedWhenNoHistory(node);
        }
        else
        {
            if(!_isPaused)
                SelectedNodeInDifferentBranch(node);
            
            AddNode = node.ReturnNode;
            AddANode?.RaiseEvent(this);
            
            _history.Add(node.ReturnNode);
            _lastSelected = node.ReturnNode;
        }
    }

    private void SelectedNodeInDifferentBranch(INode node)
    {
        if (_lastSelected.HasChildBranch != node.ReturnNode.MyBranch)
            ReverseAndClearHistory();
    }

    private void SetNullLastSelected(INode node)
    {
        if (_lastSelected is null)
            _lastSelected = node.ReturnNode;
    }

    public void CloseAllChildNodesAfterPoint(INode newNode)
    {
        if (!_history.Contains(newNode.ReturnNode)) return;

        for (int i = _history.Count -1; i > 0; i--)
        {
            if (_history[i] == newNode.ReturnNode) break;
            CloseThisLevel(_history[i]);
        }
        CloseThisLevel(newNode);
    }

    private void CloseThisLevel(INode node)
    {
        node.HasChildBranch.StartBranchExitProcess(OutTweenType.Cancel);
        node.DeactivateNode();
        
        AddNode = node.ReturnNode;
        AddANode?.RaiseEvent(this);

        _history.Remove(node.ReturnNode);
    }

    private void SetLastSelectedWhenNoHistory(INode node) 
        => _lastSelected = _history.Count > 0 ? _history.Last() : node.ReturnNode;

    public void BackOneLevel()
    {
        DoMoveBackOneLevel(_history.Last());
        IfLastSelectedIsOnHomeScreen(_history.Last());
        if(_history.Count > 0) 
            _lastSelected = _history.Last();
    }

    private static void DoMoveBackOneLevel(INode lastNode)
    {
        lastNode.SetNodeAsNotSelected_NoEffects();
        lastNode.MyBranch.Branch.MoveBackToThisBranch(lastNode.MyBranch);
    }

    private void IfLastSelectedIsOnHomeScreen(UINode lastNode)
    {
        if (lastNode.MyBranch.IsHomeScreenBranch())
        {
            BackToHome();
        }
        else
        {
            AddNode = lastNode;
            AddANode?.RaiseEvent(this);

            _history.Remove(lastNode);
        }
    }

    public void BackToHome()
    {
        if (_history.Count <= 0) return;
        ReverseAndClearHistory();
        ActivateBranchOnReturnHome = true;
        ReturnedToHome.RaiseEvent(this);
    }

    public void ReverseAndClearHistory() => HistoryProcessed(stopWhenInternalBranchReacted: true);

    private void HistoryProcessed(bool stopWhenInternalBranchReacted)
    {
        if (_history.Count == 0) return;
        _history.Reverse();
        
        foreach (var uiNode in _history)
        {
            uiNode.HasChildBranch.StartBranchExitProcess(OutTweenType.Cancel);
            uiNode.DeactivateNode();
            if (IfNodeIsInternalBranch(uiNode, stopWhenInternalBranchReacted)) return;
        }
        AddNode = null;
        AddANode?.RaiseEvent(this);

        _history.Clear();
    }
    
    private bool IfNodeIsInternalBranch(UINode uiNode, bool saveInternals)
    {
        if (!uiNode.HasChildBranch.IsInternalBranch() || !saveInternals) return false;

        AddNode = uiNode;
        AddANode?.RaiseEvent(this);

        _history.Remove(uiNode);
        _history.Reverse();
        return true;
    }

    public void SetFromHotkey(UIBranch branch, INode parentNode)
    {
        HomeScreenHotKey(branch);
        HistoryProcessed(stopWhenInternalBranchReacted: false);
        AddNode = FindHomeScreenRoot(branch);
        AddANode?.RaiseEvent(this);

        _history.Add(FindHomeScreenRoot(branch));
    }

    private void HomeScreenHotKey(UIBranch branch)
    {
        if (branch.ScreenType == ScreenType.FullScreen || _onHomScreen) return;
        ActivateBranchOnReturnHome = false;
        ReturnedToHome.RaiseEvent(this);
    }

    private UINode FindHomeScreenRoot(UIBranch branch)
    {
        while (!branch.IsHomeScreenBranch())
        {
            branch = branch.MyParentBranch;
        }
        _lastSelected = branch.LastSelected.ReturnNode;
        return _lastSelected;
    }

    public void MoveToLastBranchInHistory()
    {
        if (!_noPopUps && !_isPaused)
        {
            _popUpController.NextPopUp().MoveToBranchWithoutTween();
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
            ActivateBranchOnReturnHome = true;
            ReturnedToHome?.RaiseEvent(this);
        }
    }

    public void CancelMove(Action endOfCancelAction)
    {
        if (_noPopUps || _isPaused)
        {
            _activeBranch.StartBranchExitProcess(OutTweenType.Cancel, endOfCancelAction);
            _lastSelected.PlayCancelAudio();
            return;
        }
        
        if(!_noPopUps && !_isPaused)
        {
            PopUpToRemove = _popUpController.NextPopUp();
            PopUpToRemove.LastSelected.PlayCancelAudio();
            _popUpController.RemoveNextPopUp(PopUpToRemove);
            PopUpToRemove.StartBranchExitProcess(OutTweenType.Cancel, EndOfTweenCallback);
        }
    }

    private void EndOfTweenCallback()
    {
        if (_noPopUps)
        {
            MoveToLastBranchInHistory();
        }
        else
        {
            _popUpController.NextPopUp().MoveToBranchWithoutTween();
        }
    }
}
