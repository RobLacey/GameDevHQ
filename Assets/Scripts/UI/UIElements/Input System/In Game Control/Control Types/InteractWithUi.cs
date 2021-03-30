
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InteractWithUi : IEventUser
{
    private readonly Dictionary<UINode, RectTransform> _activeNodes = new Dictionary<UINode, RectTransform>();
    private readonly Dictionary<UINode, RectTransform> _sortedNodesDict = new Dictionary<UINode, RectTransform>();
    private readonly Dictionary<IBranch, RectTransform> _activeBranches = new Dictionary<IBranch, RectTransform>();
    private (UINode node, RectTransform rect) _lastHit;
    private bool _canStart = false;
    private INode _lastHighlighted;

    public void OnEnable() => ObserveEvents();

    public void ObserveEvents()
    {
        EVent.Do.Subscribe<IAllowKeys>(SaveAllowKeys);
        EVent.Do.Subscribe<IOnStart>(OnStart);
        EVent.Do.Subscribe<IAddNewBranch>(AddNodes);
        EVent.Do.Subscribe<IRemoveBranch>(RemoveNodes);
        EVent.Do.Subscribe<IHighlightedNode>(SaveHighlighted);
    }

    private void SaveHighlighted(IHighlightedNode args) => _lastHighlighted = args.Highlighted;

    private void OnStart(IOnStart args)
    {
        _canStart = true;
        ProcessBranchAndNodeLists.SortNodeList(_sortedNodesDict, _activeNodes);
    }

    private void SaveAllowKeys(IAllowKeys args)
    {
        if (args.CanAllowKeys)
            _lastHit = (null, null);
    }

    public void CheckIfCursorOverUI(VirtualCursor virtualCursor)
    {
        
        var pointerOverNode = _sortedNodesDict.FirstOrDefault(node => PointerInsideUIObject(node.Value, virtualCursor));

        if (pointerOverNode.Key)
        {
            if (UnderAnotherUIObject(pointerOverNode, virtualCursor)) return;
            if (pointerOverNode.Key == _lastHit.node) return;
            
            CloseLastHitNodeAsDifferent();
            StartNewNode(virtualCursor, pointerOverNode);
            return;
        }
        CheckIfStillOverLastHit(virtualCursor);
    }

    private void StartNewNode(VirtualCursor virtualCursor, KeyValuePair<UINode, RectTransform> node)
    {
        node.Key.OnPointerEnter(null);
        _lastHit = (node.Key, node.Value);
        virtualCursor.OverAnyObject = _lastHit.node.ReturnGameObject;
    }

    private void CloseLastHitNodeAsDifferent()
    {
        if (!_lastHit.node) return;
        _lastHit.node.OnPointerExit(null);
    }

    private static bool PointerInsideUIObject(RectTransform nodeRect, VirtualCursor virtualCursor)
    {
        return RectTransformUtility.RectangleContainsScreenPoint(nodeRect,
                                                                 virtualCursor.Position,
                                                                 null);
    }

    private bool UnderAnotherUIObject(KeyValuePair<UINode, RectTransform> node, VirtualCursor virtualCursor)
    {
        var branchesAbove = _activeBranches.Where(activeBranch =>
                                                       activeBranch.Key.MyCanvas.sortingOrder >
                                                       node.Key.MyBranch.MyCanvas.sortingOrder);
    
        foreach (var activeBranch in branchesAbove)
        {
            if (node.Key.MyBranch == activeBranch.Key || !node.Value.DoBranchesOverlap(activeBranch.Value)) continue;
            
            if (PointerInsideUIObject(activeBranch.Value, virtualCursor))
            {
                OverBranchButActiveNodeBelow(node);
                return true;
            }
        }
        return false;
    }

    private void CheckIfStillOverLastHit(VirtualCursor virtualCursor)
    {
        if(_lastHit.node)
        {
            _lastHit.node.OnPointerExit(null);
            _lastHit = (null, null);
        }       
        
        foreach (var branch in _activeBranches)
        {
            if(PointerInsideUIObject(branch.Value, virtualCursor))
            {
                virtualCursor.OverAnyObject = branch.Key.ThisBranchesGameObject;
                return;
            }        
        }
        virtualCursor.OverAnyObject = null;
    }

    private void OverBranchButActiveNodeBelow(KeyValuePair<UINode, RectTransform> node)
    {
        if (!ReferenceEquals(_lastHighlighted, node.Key)) return;
        
        node.Key.OnPointerExit(null);
        _lastHighlighted = null;
    }

    public bool UIObjectSelected(bool selected)
    {
        if (!_lastHit.node || !selected) return false;
        
        _lastHit.node.OnPointerDown(null);
        return true;
    }

    private void AddNodes(IAddNewBranch args)
    {
        var nodes = args.MyBranch.ThisGroupsUiNodes.Cast<UINode>().ToArray();
        
        ProcessBranchAndNodeLists.CheckAndAddNewBranch(args.MyBranch, _activeBranches);
        var needToSort = ProcessBranchAndNodeLists.AddNewNodesToList(nodes, _activeNodes);
        
        if(needToSort && _canStart) 
            ProcessBranchAndNodeLists.SortNodeList(_sortedNodesDict, _activeNodes);
    }

    private void RemoveNodes(IRemoveBranch args)
    {
        var list = args.MyBranch.ThisGroupsUiNodes.Cast<UINode>().ToArray();

        ProcessBranchAndNodeLists.CheckAndRemoveBranch(args.MyBranch, _activeBranches);
        var needSort = ProcessBranchAndNodeLists.RemoveNodeFromList(list, _activeNodes);

        if(needSort & _canStart) 
            ProcessBranchAndNodeLists.SortNodeList(_sortedNodesDict, _activeNodes);
    }

}

public static class ExtensionMethod
{
    public static bool DoBranchesOverlap(this RectTransform nodeRectTransform, RectTransform branchRectTransform)
    {
        Rect rect1 = new Rect(nodeRectTransform.rect);
        Rect rect2 = new Rect(branchRectTransform.rect);
        return rect1.Overlaps(rect2);
    }
}
