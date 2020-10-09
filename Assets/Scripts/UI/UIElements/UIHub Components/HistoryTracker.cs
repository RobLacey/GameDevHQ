using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public interface IReturnToHome { }

public class HistoryTracker : MonoBehaviour, IHistoryTrack, IReturnToHome
{
    [SerializeField] private List<UINode> _history;
    [SerializeField] private UINode _lastHighlighted;
    [SerializeField] private UINode _lastSelected;
    [SerializeField] private UIBranch _activeBranch;
    [SerializeField] private UINode _hotKeyParent;

    private readonly UIDataEvents _uiDataEvents = new UIDataEvents();
    private bool _canStart;
    private ICustomEvent<IReturnToHome> ReturnedToHome { get; set; }

    private void Awake()
    {
        ServiceLocator.AddService<IHistoryTrack>(this);
        ReturnedToHome = new CustomEvent<IReturnToHome>();
    }
    
    private void OnEnable()
    {
        _uiDataEvents.SubscribeToHighlightedNode(SetLastHighlighted);
        _uiDataEvents.SubscribeToActiveBranch(SetActiveBranch);
        _uiDataEvents.SubscribeToOnStart(SetCanStart);
    }

    private void Start()
    {
        // ReturnedToHome = new TesTEvent<IReturnToHome>();
    }

    private void SetLastHighlighted(INode node) => _lastHighlighted = node.ReturnNode;
    private void SetActiveBranch(UIBranch branch) => _activeBranch = branch;
    private void SetCanStart() => _canStart = true;

    //Main
    public void SetSelected(INode node)
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

    public void CloseAllChildNodesAfterPoint(INode newNode)
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

    public void BackOneLevel()
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
        ReverseAndClearHistory();
        ReturnedToHome.RaiseEvent();
    }

    public void ReverseAndClearHistory()
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

    public void SetFromHotkey(UIBranch branch, INode parentNode)
    {
        HotKeyOnHomeScreen(branch);
        IfLastKeyPressedWasHotkey(parentNode);
        FindHomeScreenRoot(branch);
        _history.Clear();
        _history.Add(_lastSelected);
    }

    private void HotKeyOnHomeScreen(UIBranch branch)
    {
        if (branch.ScreenType == ScreenType.FullScreen) return;
        ReturnedToHome.RaiseEvent();
    }

    private void IfLastKeyPressedWasHotkey(INode parentNode)
    {
        if(_lastSelected)
            _lastSelected.DeactivateNode();
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
