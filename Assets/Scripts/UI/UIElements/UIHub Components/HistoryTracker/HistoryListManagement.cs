using System;
using System.Collections.Generic;
using System.Linq;

public interface IHistoryManagement
{
    IHistoryManagement CloseToThisPoint(INode node);
    IHistoryManagement CurrentHistory(List<UINode> history);
    void Run();
    void ClearAllHistory();
    void ClearHistoryStopAtInternalBranch();
    IHistoryManagement IgnoreHotKeyParent(INode node);
}

public class HistoryListManagement : IHistoryManagement
{
    public HistoryListManagement(HistoryTracker historyTracker)
    {
        _historyTracker = historyTracker;
    }

    //Variables
    private readonly HistoryTracker _historyTracker;
    private INode _targetNode, _hotKeyParent;
    private bool _hasHistory;
    private List<UINode> _history = new List<UINode>();

    public IHistoryManagement CloseToThisPoint(INode node)
    {
        _targetNode = node;
        return this;
    }
    public IHistoryManagement IgnoreHotKeyParent(INode node)
    {
        _hotKeyParent = node;
        return this;
    }
    
    public IHistoryManagement CurrentHistory(List<UINode> history)
    {
        _history = history;
        _hasHistory = true;
        return this;
    }

    public void Run()
    {
        CheckForExceptions();
        CloseAllChildNodesAfterPoint();
    }

    private void CheckForExceptions()
    {
        if(_targetNode is null) throw new Exception("Missing Target Node");
        CheckForMissingHistory();
        _hasHistory = false;
    }

    private void CheckForMissingHistory()
    {
        if (!_hasHistory) throw new Exception("Missing Current History");
    }

    private void CloseAllChildNodesAfterPoint()
    {
        if (!_history.Contains(_targetNode.ReturnNode)) return;
        
        for (int i = _history.Count -1; i > 0; i--)
        {
            if (_history[i] == _targetNode.ReturnNode) break;
            CloseThisLevel(_history[i]);
        }
        CloseThisLevel(_targetNode);
    }

    private void CloseThisLevel(INode node)
    {
        _history.Remove(node.ReturnNode);
        node.HasChildBranch.LastSelected.DeactivateNode();
        node.HasChildBranch.StartBranchExitProcess(OutTweenType.Cancel, EndOfTweenActions);

        _historyTracker.AddNodeToTestRunner(node.ReturnNode);

        void EndOfTweenActions() => node.MyBranch.MoveToBranchWithoutTween();
    }
    
    public void ClearAllHistory()
    {
        CheckForMissingHistory();
        HistoryProcessed(false);
        _hotKeyParent = null;
    }
    
    public void ClearHistoryStopAtInternalBranch()
    {
        CheckForMissingHistory();
        HistoryProcessed(stopAtInternalBranch: true);
        _hotKeyParent = null;
    }

    private void HistoryProcessed(bool stopAtInternalBranch)
    {
        if (_history.Count == 0) return;
        
        var firstInHistory = _history.First();
        _history.Reverse();
        ResetAndClearHistoryList(stopAtInternalBranch, firstInHistory);
    }

    private void ResetAndClearHistoryList(bool stopAtInternalBranch, UINode firstInHistory)
    {
        foreach (var currentNode in _history)
        {
            if (SkipIfNodeIsHotKeysParent(currentNode)) continue;
            
            currentNode.HasChildBranch.StartBranchExitProcess(OutTweenType.Cancel);
            ResetNode(currentNode, firstInHistory);
            if (IfNodeIsInternalBranch(currentNode, stopAtInternalBranch, _history)) return;
        }
        _historyTracker.AddNodeToTestRunner(null);
        _history.Clear();
    }

    private void ResetNode(UINode currentNode, UINode firstInHistory)
    {
        if (currentNode == firstInHistory)
        {
            currentNode.DeactivateNode();
        }
        else
        {
            currentNode.SetNodeAsNotSelected_NoEffects();
            currentNode.SetNotHighlighted();
        }
    }

    private bool SkipIfNodeIsHotKeysParent(UINode uiNode) 
        => !(_hotKeyParent is null) && uiNode == _hotKeyParent.ReturnNode;

    private bool IfNodeIsInternalBranch(UINode uiNode, bool stopAtInternalBranch, List<UINode> history)
    {
        if (!uiNode.HasChildBranch.IsInternalBranch() || !stopAtInternalBranch) return false;

        _historyTracker.AddNodeToTestRunner(uiNode);

        history.Remove(uiNode);
        history.Reverse();
        return true;
    }
}