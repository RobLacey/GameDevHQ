using System;
using System.Collections.Generic;
using System.Linq;

public interface INewSelectionProcess
{
    UINode Run();
    INewSelectionProcess NewNode(UINode newNode);
    INewSelectionProcess CurrentHistory(List<UINode> history);
    INewSelectionProcess LastSelectedNode(UINode lastSelected);
}

public class NewSelectionProcess : INewSelectionProcess
{
    public NewSelectionProcess(HistoryTracker historyTracker, IHistoryManagement historyManagement )
    {
        _historyTracker = historyTracker;
        _historyManagement = historyManagement;
    }

    //Variables
    private readonly HistoryTracker _historyTracker;
    private UINode _newNode, _lastSelected;
    private List<UINode> _history = new List<UINode>();
    private readonly IHistoryManagement _historyManagement;
    private bool _hasHistory, _hasLastSelected, _hasNewNode;

    public INewSelectionProcess NewNode(UINode newNode)
    {
        _newNode = newNode;
        _hasNewNode = true;
        return this;
    }
    public INewSelectionProcess CurrentHistory(List<UINode> history)
    {
        _history = history;
        _hasHistory = true;
        return this;
    }
    public INewSelectionProcess LastSelectedNode(UINode lastSelected)
    {
        _lastSelected = lastSelected;
        _hasLastSelected = true;
        return this;
    }

    public UINode Run()
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

    private void DoesntContainNewNode()
    {
        if (!_historyTracker.IsPaused)
            SelectedNodeInDifferentBranch();

        _historyTracker.AddNodeToTestRunner(_newNode);

        _history.Add(_newNode.ReturnNode);
        _lastSelected = _newNode;
    }

    private void SetLastSelectedWhenNoHistory() 
        => _lastSelected = _history.Count > 0 ? _history.Last() : _newNode;

    private void SelectedNodeInDifferentBranch()
    {
        if (_lastSelected.HasChildBranch != _newNode.MyBranch) 
            _historyManagement.CurrentHistory(_history).ClearHistoryStopAtInternalBranch();
    }
}