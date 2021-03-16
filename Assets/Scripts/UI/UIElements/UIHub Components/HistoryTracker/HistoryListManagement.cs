using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public interface IHistoryManagement
{
    IHistoryManagement CloseToThisPoint(INode node);
    IHistoryManagement CurrentHistory(List<INode> history);
    void Run();
    void ClearAllHistory();
    void ClearHistoryWithStopPointCheck(INode stopAtThisNode);
}

public class HistoryListManagement : IHistoryManagement
{
    public HistoryListManagement(IHistoryTrack historyTracker)
    {
        _historyTracker = historyTracker;
    }

    //Variables
    private readonly IHistoryTrack _historyTracker;
    private INode _targetNode;
    private bool _hasHistory;
    private List<INode> _history = new List<INode>();

    public IHistoryManagement CloseToThisPoint(INode node)
    {
        _targetNode = node;
        return this;
    }
    
    public IHistoryManagement CurrentHistory(List<INode> history)
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
        if (!_history.Contains(_targetNode)) return;
        
        for (int i = _history.Count -1; i > 0; i--)
        {
            if (_history[i] == _targetNode) break;
            CloseThisLevel(_history[i]);
        }
        CloseThisLevel(_targetNode);
    }

    private void CloseThisLevel(INode node)
    {
        _history.Remove(node);
        node.HasChildBranch.LastSelected.DeactivateNode();
        node.HasChildBranch.StartBranchExitProcess(OutTweenType.Cancel, EndOfTweenActions);
        _historyTracker.AddNodeToTestRunner(node);

        void EndOfTweenActions() => node.MyBranch.MoveToThisBranch();
    }
    
    public void ClearAllHistory()
    {
        CheckForMissingHistory();
        if (_history.Count == 0) return;
        ResetAndClearHistoryList();
    }
    
    public void ClearHistoryWithStopPointCheck(INode stopAtThisNode)
    {
        CheckForMissingHistory();
        if (_history.Count == 0) return;
        ResetAndClearHistoryList(stopAtThisNode);
    }

    private void ResetAndClearHistoryList(INode stopAtThisNode = null)
    {
        var firstInHistory = _history.First();
        _history.Reverse();
        
        foreach (var currentNode in _history)
        {
            ExitNode(currentNode, firstInHistory);
            if (CheckIfAtStopPoint(stopAtThisNode, currentNode)) return;
        }
        
        _historyTracker.AddNodeToTestRunner(null);
        _history.Clear();
    }

    private static void ExitNode(INode currentNode, INode firstInHistory)
    {
        currentNode.HasChildBranch.StartBranchExitProcess(OutTweenType.Cancel);
        ResetNode(currentNode, firstInHistory);
    }

    private bool CheckIfAtStopPoint(INode stopAtThisNode, INode currentNode)
    {
        if (stopAtThisNode is null) return false;
        if (currentNode.MyBranch != stopAtThisNode.MyBranch) return false;
        
        currentNode.DeactivateNode();
        _historyTracker.AddNodeToTestRunner(currentNode);
        _history.Remove(currentNode);
        return true;

    }

    private static void ResetNode(INode currentNode, INode firstInHistory)
    {
        if (currentNode == firstInHistory)
        {
            currentNode.DeactivateNode();
        }
        else
        {
            currentNode.ClearNode();
        }
    }
}