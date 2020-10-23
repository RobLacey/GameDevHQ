using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HistoryTracker : IHistoryTrack, IEventUser, IReturnToHome
{
    public HistoryTracker() => OnEnable();

    //Variables
    private readonly List<UINode> _history = new List<UINode>();
    private UINode _lastSelected;
    private bool _canStart;
    private bool _onHomScreen = true;

    private void SaveOnHomScreen(IOnHomeScreen args) => _onHomScreen = args.OnHomeScreen;
    
    //Events
    private static CustomEvent<IReturnToHome> ReturnedToHome { get; } = new CustomEvent<IReturnToHome>();
    
    public void ObserveEvents()
    {
        EventLocator.Subscribe<IOnStart>(SetCanStart, this);
        EventLocator.Subscribe<IOnHomeScreen>(SaveOnHomScreen, this);
    }

    public void RemoveFromEvents()
    {
        EventLocator.Unsubscribe<IOnStart>(SetCanStart);
        EventLocator.Unsubscribe<IOnHomeScreen>(SaveOnHomScreen);
    }

    public void OnEnable() => ObserveEvents();

    public void OnDisable() => RemoveFromEvents();

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
            SelectedNodeInDifferentBranch(node);
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
            _history.Remove(lastNode);
        }
    }

    public void BackToHome()
    {
        if (_history.Count <= 0) return;
        var lastHomeNode = _history.First();
        ReverseAndClearHistory();
        lastHomeNode.MyBranch.MoveToBranchWithoutTween();
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
        _history.Clear();
    }
    
    private bool IfNodeIsInternalBranch(UINode uiNode, bool saveInternals)
    {
        if (!uiNode.HasChildBranch.IsInternalBranch() || !saveInternals) return false;
        _history.Remove(uiNode);
        _history.Reverse();
        return true;
    }

    public void SetFromHotkey(UIBranch branch, INode parentNode)
    {
        HomeScreenHotKey(branch);
        HistoryProcessed(stopWhenInternalBranchReacted: false);
        _history.Add(FindHomeScreenRoot(branch));
    }

    private void HomeScreenHotKey(UIBranch branch)
    {
        if(branch.ScreenType != ScreenType.FullScreen && !_onHomScreen)
        {
            ReturnedToHome.RaiseEvent(this);
        }    
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
}
