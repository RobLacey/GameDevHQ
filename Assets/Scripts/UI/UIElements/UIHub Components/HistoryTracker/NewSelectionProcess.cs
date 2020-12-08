using System;
using System.Collections.Generic;
using System.Linq;

public interface INewSelectionProcess
{
    INode Run();
    INewSelectionProcess NewNode(INode newNode);
    INewSelectionProcess CurrentHistory(List<INode> history);
    INewSelectionProcess LastSelectedNode(INode lastSelected);
}

public class NewSelectionProcess : INewSelectionProcess
{
    public NewSelectionProcess(IHistoryTrack historyTracker)
    {
        _historyTracker = historyTracker;
        _historyManagement = historyTracker.HistoryListManagement;
    }

    //Variables
    private readonly IHistoryTrack _historyTracker;
    private INode _newNode, _lastSelected;
    private List<INode> _history = new List<INode>();
    private readonly IHistoryManagement _historyManagement;
    private bool _hasHistory, _hasLastSelected, _hasNewNode;

    public INewSelectionProcess NewNode(INode newNode)
    {
        _newNode = newNode;
        _hasNewNode = true;
        return this;
    }
    public INewSelectionProcess CurrentHistory(List<INode> history)
    {
        _history = history;
        _hasHistory = true;
        return this;
    }
    public INewSelectionProcess LastSelectedNode(INode lastSelected)
    {
        _lastSelected = lastSelected;
        _hasLastSelected = true;
        return this;
    }

    public INode Run()
    {
        CheckForExceptions();
        DoSelectedProcess();
        return _lastSelected;
    }

    private void CheckForExceptions()
    {
        if (!_hasHistory) throw new Exception("Missing Current History");
        if (!_hasNewNode) throw new Exception("Missing New Node");
        if (!_hasLastSelected) throw new Exception("Missing Last Selected");
        _hasHistory = false;
        _hasLastSelected = false;
        _hasNewNode = false;
    }

    private void DoSelectedProcess()
    {
        CheckAndSetLastSelectedIfNull();
        if (_history.Contains(_newNode))
        {
            ContainsNewNode();
        }
        else
        {
            DoesntContainNewNode();
        }
    }

    private void CheckAndSetLastSelectedIfNull()
    {
        if (_lastSelected is null)
            _lastSelected = _newNode;
    }

    private void ContainsNewNode()
    {
        _historyManagement.CloseToThisPoint(_newNode)
                          .CurrentHistory(_history)
                          .Run();
        SetLastSelectedWhenNoHistory();
    }

    private void SetLastSelectedWhenNoHistory() 
        => _lastSelected = _history.Count > 0 ? _history.Last() : _newNode;

    private void DoesntContainNewNode()
    {
        if (!_historyTracker.IsPaused)
            SelectedNodeInDifferentBranch();

        _historyTracker.AddNodeToTestRunner(_newNode);

        _history.Add(_newNode);
        _lastSelected = _newNode;
    }

    private void SelectedNodeInDifferentBranch()
    {
        bool IfNewBranchIsNotAnInternalBranch() => _lastSelected.HasChildBranch != _newNode.MyBranch;

        if (IfNewBranchIsNotAnInternalBranch()) 
            _historyManagement.CurrentHistory(_history).ClearHistoryWithStopPointCheck(_newNode);
    }
}