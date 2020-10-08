using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HistoryTracker : MonoBehaviour
{
    [SerializeField] private List<UINode> _history;
    [SerializeField] private UINode _lastHighlighted;
    [SerializeField] private UINode _lastSelected;
    [SerializeField] private UIBranch _activeBranch;
    [SerializeField] private UINode _hotKeyParent;

    private readonly UIDataEvents _uiDataEvents = new UIDataEvents();
    private bool _canStart;
    public static Action<UINode> selected;
    public static Action<UINode> clearDisabledChildren;
    public static Action<UIBranch, INode> setFromHotKey;
    public static Action backOneLevel;
    public static Action clearHome;
    public static Action clearHistory;
    public static event Action OnHome;
    
    private void OnEnable()
    {
        selected += SetSelected;
        clearHome += SetToHome;
        backOneLevel += BackOneLevel;
        clearHistory += ReverseAndClearHistory;
        clearDisabledChildren += CloseAllChildNodesAfterPoint;
        setFromHotKey += SetFromHotkey;
        _uiDataEvents.SubscribeToHighlightedNode(SetLastHighlighted);
        _uiDataEvents.SubscribeToActiveBranch(SetActiveBranch);
        _uiDataEvents.SubscribeToOnStart(SetCanStart);
    }

    private void SetLastHighlighted(INode node) => _lastHighlighted = node.ReturnNode;
    private void SetActiveBranch(UIBranch branch) => _activeBranch = branch;
    private void SetCanStart() => _canStart = true;

    //Main
    private void SetSelected(INode node)
    {
        if(!_canStart) return;
        if(node.ReturnNode.DontStoreTheseNodeTypesInHistory) return;
        SetNullLastSelected(node);
        _hotKeyParent = null;
        
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

    private void CloseAllChildNodesAfterPoint(INode newNode)
    {
        if (!_history.Contains(newNode.ReturnNode)) return;

        foreach (var uiNode in _history.SkipWhile(node => node != node.ReturnNode).ToArray())
        {
            uiNode.HasChildBranch.StartBranchExitProcess(OutTweenType.Cancel);
            uiNode.DeactivateNode();
            _history.Remove(uiNode);
        }
    }

    private void SetLastSelectedWhenNoHistory(INode node) 
        => _lastSelected = _history.Count > 0 ? _history.Last() : node.ReturnNode;

    private void BackOneLevel()
    {
        var lastNode = _history.Last();
        DoMoveBackOneLevel(lastNode);
        IfLastSelectedIsOnHomeScreen(lastNode);
        if(_history.Count > 0) 
            _lastSelected = _history.Last();
    }

    private void DoMoveBackOneLevel(UINode lastNode)
    {
        lastNode.MyBranch.Branch.MoveBackToThisBranch(lastNode.MyBranch);
        lastNode.DeactivateNode();
        if(_hotKeyParent) _hotKeyParent.DeactivateNode();
        _hotKeyParent = null;
    }


    private void IfLastSelectedIsOnHomeScreen(UINode lastNode)
    {
        if (lastNode.MyBranch.IsHomeScreenBranch())
        {
            SetToHome();
        }
        else
        {
            _history.Remove(lastNode);
        }
    }

    private void SetToHome()
    {
        if (_history.Count <= 0) return;
        ReverseAndClearHistory();
        OnHome?.Invoke();
    }

    private void ReverseAndClearHistory()
    {
        _history.Reverse();
        if (HistoryProcessed()) return;
        _history.Clear();
    }

    private bool HistoryProcessed()
    {
        foreach (var uiNode in _history)
        {
            uiNode.HasChildBranch.StartBranchExitProcess(OutTweenType.Cancel);
            uiNode.DeactivateNode();
            if (IfNodeIsInternalBranch(uiNode)) return true;
        }
        return false;
    }

    private bool IfNodeIsInternalBranch(UINode uiNode)
    {
        if (!uiNode.HasChildBranch.IsInternalBranch()) return false;
        _history.Remove(uiNode);
        _history.Reverse();
        return true;
    }

    private void SetFromHotkey(UIBranch branch, INode parentNode)
    {
        HotKeyOnHomeScreen(branch);
        IfLastKeyPressedWasHotkey(parentNode);
        FindHomeScreenRoot(branch);
        _history.Clear();
        _history.Add(_lastSelected);
    }

    private static void HotKeyOnHomeScreen(UIBranch branch)
    {
        if (branch.ScreenType == ScreenType.FullScreen) return;
        OnHome?.Invoke();
    }

    private void IfLastKeyPressedWasHotkey(INode parentNode)
    {
        if (_hotKeyParent)
            _hotKeyParent.DeactivateNode();
        _hotKeyParent = parentNode.ReturnNode;
    }

    private void FindHomeScreenRoot(UIBranch branch)
    {
        while (!branch.IsHomeScreenBranch())
        {
            branch = branch.MyParentBranch;
        }
        _lastSelected = branch.LastSelected.ReturnNode;
    }
}
