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
   // [SerializeField] private bool _onHomeScreen = true;

    UIDataEvents _uiDataEvents = new UIDataEvents();
    private bool _canStart;
    public static Action<UINode> selected;
    public static Action<UINode> clearDisabledChildren;
    public static Action<UIBranch, INode> setFromHotKey;
    public static Action backOneLevel;
    public static Action clearHome;
    public static Action clearHistory;
   // private UIControlsEvents _uiControlsEvents = new UIControlsEvents();
    public static event Action OnHome;
    private INode _hotKeyParent;
    
    private void OnEnable()
    {
        selected += SetSelected;
        clearHome += ToHome;
        backOneLevel += BackOneLevel;
        clearHistory += ClearHistory;
        clearDisabledChildren += ClearDisableChildren;
        setFromHotKey += SetFromHotkey;
        _uiDataEvents.SubscribeToHighlightedNode(SetLastHighlighted);
        _uiDataEvents.SubscribeToActiveBranch(SetActiveBranch);
        _uiDataEvents.SubscribeToOnStart(SetCanStart);
        //_uiControlsEvents.SubscribeFromHotKey(ClearHistory);

       // _uiDataEvents.SubscribeToOnHomeScreen(SetIfOnHomeScreen);
    }

    private void SetLastHighlighted(INode node)
    {
        _lastHighlighted = node.ReturnNode;
    }

    // void SetIfOnHomeScreen(bool onHomeScreen)
    // {
    //     _onHomeScreen = onHomeScreen;
    // }
    
    private void SetSelected(INode node)
    {
        if(!_canStart) return;
        if(node.ReturnNode.DontStoreNodeInHistory) return;
        if (_lastSelected is null)
        {
            _lastSelected = node.ReturnNode;
        }
        
        if (_history.Contains(node.ReturnNode))
        {
            var delete = _history.SkipWhile(x => x != node.ReturnNode).ToArray();
            
            foreach (var t in delete)
            {
                t.HasChildBranch.StartBranchExitProcess(OutTweenType.Cancel);
                t.DeactivateNode();
                _history.Remove(t);
            }

            if(_history.Count > 0)
            {
                _lastSelected = _history.Last();
            }
            else
            {
                _lastSelected = node.ReturnNode;
            }
        }
        else
        {
            if (_lastSelected.HasChildBranch != node.ReturnNode.MyBranch)
            {
                Debug.Log("Here");
                ClearHistory();
            }
            _history.Add(node.ReturnNode);
            _lastSelected = node.ReturnNode;
        }
    }

    private void BackOneLevel()
    {
        _history.Last().HasChildBranch.StartBranchExitProcess(OutTweenType.Cancel);
        _history.Last().DeactivateNode();
        if (_hotKeyParent != null)
        {
            _hotKeyParent.SetNodeAsNotSelected_NoEffects();
            _hotKeyParent = null;
        }
        if(_history.Last().MyBranch.IsHomeScreenBranch())
        {
            Debug.Log("On Home");
            OnHome?.Invoke();
        }
        _history.Last().MyBranch.Branch.MoveBackToThisBranch(_history.Last().MyBranch); //TODO may not need parameter
        _history.Remove(_history.Last());
        if(_history.Count > 0) _lastSelected = _history.Last();
    }

    private void ToHome()
    {
        if(_history.Count > 0)
        {
            ClearHistory();
            OnHome?.Invoke();
        }        
    }

    private void ClearHistory()
    {
        _history.Reverse();
        foreach (var uiNode in _history)
        {
            uiNode.HasChildBranch.StartBranchExitProcess(OutTweenType.Cancel);
            if (uiNode.HasChildBranch.IsInternalBranch())
            {
                _history.Remove(uiNode);
                _history.Reverse();
                return;
            }
            uiNode.DeactivateNode();
        }
        _history.Clear();
    }

    private void ClearDisableChildren(INode node)
    {
        if (_history.Contains(node.ReturnNode))
        {
            var delete = _history.SkipWhile(x => x != node.ReturnNode).ToArray();
            
            foreach (var t in delete)
            {
                t.HasChildBranch.StartBranchExitProcess(OutTweenType.Cancel);
                t.DeactivateNode();
                _history.Remove(t);
            }
        }
    }

    private void SetFromHotkey(UIBranch branch, INode node)
    {
        _hotKeyParent = node;
        ClearHistory();
        var temp = branch;
        while (!temp.IsHomeScreenBranch())
        {
            temp = temp.MyParentBranch;
        }

        _lastSelected = temp.LastSelected.ReturnNode;
        _history.Add(_lastSelected);
    }


    private void SetActiveBranch(UIBranch branch)
    {
        _activeBranch = branch;
    }

    private void SetCanStart() => _canStart = true;
}
