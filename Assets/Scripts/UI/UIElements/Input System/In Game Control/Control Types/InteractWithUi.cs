using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public interface IInteractWithUi
{
    void OnEnable();
    void SetCanOnlyHitInGameObjects();
    void CheckIfCursorOverUI(IVirtualCursor virtualCursor);
    void CloseLastHitNodeAsDifferent();
    bool UIObjectSelected(bool selected);
}

public class InteractWithUi : IInteractWithUi, IEventUser
{
    private readonly Dictionary<UINode, RectTransform> _activeNodes = new Dictionary<UINode, RectTransform>();
    private readonly Dictionary<UINode, RectTransform> _sortedNodesDict = new Dictionary<UINode, RectTransform>();
    private readonly Dictionary<IBranch, RectTransform> _activeBranches = new Dictionary<IBranch, RectTransform>();
    private readonly Dictionary<IBranch, RectTransform> _sortedBranches = new Dictionary<IBranch, RectTransform>();
    private (UINode node, RectTransform rect) _lastHit;
    private bool _canStart = false;
    private bool _onlyHitInGameObjects;

    private void OnStart(IOnStart args)
    {
        _canStart = true;
        ProcessBranchAndNodeLists.SortNodeList(_sortedNodesDict, _activeNodes);
        ProcessBranchAndNodeLists.SortBranchList(_sortedBranches, _activeBranches);
    }

    private void SaveAllowKeys(IAllowKeys args)
    {
        if (args.CanAllowKeys)
            _lastHit = (null, null);
    }
    public void SetCanOnlyHitInGameObjects() => _onlyHitInGameObjects = true;

    //Main
    public void OnEnable() => ObserveEvents();
    
    public void ObserveEvents()
    {
        EVent.Do.Subscribe<IAllowKeys>(SaveAllowKeys);
        EVent.Do.Subscribe<IOnStart>(OnStart);
        EVent.Do.Subscribe<IAddNewBranch>(AddNodes);
        EVent.Do.Subscribe<IRemoveBranch>(RemoveNodes);
        EVent.Do.Subscribe<ICancelPressed>(CancelPressed);
    }

    private void CancelPressed(ICancelPressed args)
    {
        _lastHit = (null, null);
    }

    public void CheckIfCursorOverUI(IVirtualCursor virtualCursor)
    {
        var pointerOverNode = _sortedNodesDict.FirstOrDefault(node => PointerInsideUIObject(node.Value, virtualCursor));

        if (pointerOverNode.Key)
        {
            if (UnderAnotherUIObject(pointerOverNode, virtualCursor)) return;
            if(pointerOverNode.Key == _lastHit.node) return;
            StartNewNode(virtualCursor, pointerOverNode);
            return;
        }
        CheckIfNotOverLastHitNode(virtualCursor);
    }

    private bool UnderAnotherUIObject(KeyValuePair<UINode, RectTransform> node, IVirtualCursor virtualCursor)
    {
        foreach (var activeBranch in _sortedBranches)
        {
            if (PointerInsideUIObject(activeBranch.Value, virtualCursor))
            {
                return OverBranchButActiveNodeBelow(node, activeBranch.Key);
            }
        }
        return false;
    }

    private bool OverBranchButActiveNodeBelow(KeyValuePair<UINode, RectTransform> nodeCursorIsOver, 
                                              IBranch branchCursorIsOver)
    {
         if (nodeCursorIsOver.Key.MyBranch.MyCanvas.sortingOrder < branchCursorIsOver.MyCanvas.sortingOrder)
         {
             nodeCursorIsOver.Key.OnPointerExit(null);
             CloseLastHitAsNotOver(); 
             return true;
         }
         return false;
    }

    private void StartNewNode(IVirtualCursor virtualCursor, KeyValuePair<UINode, RectTransform> node)
    {
        CloseLastHitNodeAsDifferent();
        node.Key.OnPointerEnter(null);
        CloseLastBranchIfNotRelated(virtualCursor, node.Key.MyBranch);
        _lastHit = (node.Key, node.Value);
        virtualCursor.OverAnyObject = _lastHit.node.MyBranch;
        virtualCursor.OverAnyObject.AutoOpenCloseClass.OnPointerEnter();
    }

    public void CloseLastHitNodeAsDifferent()
    {
        if (!_lastHit.node) return;
        _lastHit.node.OnPointerExit(null);
    }

    private void CloseLastBranchIfDifferent(IVirtualCursor virtualCursor, IBranch currentBranch)
    {
        if (virtualCursor.OverAnyObject.IsNotNull() && virtualCursor.OverAnyObject != currentBranch)
        {
            virtualCursor.OverAnyObject.AutoOpenCloseClass.OnPointerExit();
        }
    }
    
    private void CloseLastBranchIfNotRelated(IVirtualCursor virtualCursor, IBranch currentBranch)
    {
        if(virtualCursor.OverAnyObject.IsNull() || currentBranch.IsNull()) return;
        
        if (virtualCursor.OverAnyObject.MyCanvas.sortingOrder > currentBranch.MyCanvas.sortingOrder)
        {
            virtualCursor.OverAnyObject.AutoOpenCloseClass.OnPointerExit();
        }
    }

    private void CheckIfNotOverLastHitNode(IVirtualCursor virtualCursor)
    {
        CloseLastHitAsNotOver();  
        
        if (SetOverAnActiveBranch(virtualCursor)) return;
        
        CloseLastBranchIfDifferent(virtualCursor, null);
        virtualCursor.OverAnyObject = null;
    }

    private static bool PointerInsideUIObject(RectTransform nodeRect, IVirtualCursor virtualCursor)
    {
        return RectTransformUtility.RectangleContainsScreenPoint(nodeRect,
                                                                 virtualCursor.CursorRect.position,
                                                                 null);
    }

    private void CloseLastHitAsNotOver()
    {
        if (!_lastHit.node) return;
        _lastHit.node.OnPointerExit(null);
        _lastHit = (null, null);
    }

    private bool SetOverAnActiveBranch(IVirtualCursor virtualCursor)
    {
        foreach (var branch in _sortedBranches)
        {
            if (PointerInsideUIObject(branch.Value, virtualCursor))
            {
                CloseLastBranchIfDifferent(virtualCursor, branch.Key);
                if(virtualCursor.OverAnyObject != branch.Key)
                {
                    branch.Key.AutoOpenCloseClass.OnPointerEnter();
                    virtualCursor.OverAnyObject = branch.Key;
                }                
                return true;
            }
        }
        return false;
    }

    public bool UIObjectSelected(bool selected)
    {
        if (!_lastHit.node || !selected) return false;
        _lastHit.node.OnPointerDown(null);
        return true;
    }

    private void AddNodes(IAddNewBranch args)
    {
        if(_onlyHitInGameObjects && !args.MyBranch.IsInGameBranch()) return;
        
        var nodes = args.MyBranch.ThisGroupsUiNodes.Cast<UINode>().ToArray();
        
        var branchesNeedSort = ProcessBranchAndNodeLists.CheckAndAddNewBranch(args.MyBranch, _activeBranches);
        var needToSort = ProcessBranchAndNodeLists.AddNewNodesToList(nodes, _activeNodes);
        
        if(needToSort && _canStart) 
            ProcessBranchAndNodeLists.SortNodeList(_sortedNodesDict, _activeNodes);
        
        if(branchesNeedSort && _canStart)
        {
            ProcessBranchAndNodeLists.SortBranchList(_sortedBranches, _activeBranches);
        }    
    }

    private void RemoveNodes(IRemoveBranch args)
    {
        if(_onlyHitInGameObjects && !args.MyBranch.IsInGameBranch()) return;

        var list = args.MyBranch.ThisGroupsUiNodes.Cast<UINode>().ToArray();

        ProcessBranchAndNodeLists.CheckAndRemoveBranch(args.MyBranch, _activeBranches);
        var needSort = ProcessBranchAndNodeLists.RemoveNodeFromList(list, _activeNodes);

        if(needSort & _canStart) 
            ProcessBranchAndNodeLists.SortNodeList(_sortedNodesDict, _activeNodes);
    }
}