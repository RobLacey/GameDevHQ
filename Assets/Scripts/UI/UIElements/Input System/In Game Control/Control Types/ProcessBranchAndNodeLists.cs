using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class ProcessBranchAndNodeLists
{
    public static bool AddNewNodesToList(IEnumerable<UINode> list, IDictionary<UINode, RectTransform> activeNodes)
    {
        bool needSort = false;
        
        foreach (var node in list)
        {
            if (activeNodes.ContainsKey(node)) continue;
            var newRect = node.GetComponent<RectTransform>();
            activeNodes.Add(node, newRect);
            needSort = true;
        }
        return needSort;
    }

    public static void CheckAndAddNewBranch(IBranch newBranch, IDictionary<IBranch, RectTransform> activeBranches)
    {
        if (!activeBranches.ContainsKey(newBranch))
        {
            activeBranches.Add(newBranch, newBranch.ThisBranchesGameObject.GetComponent<RectTransform>());
        }
    }

    public static bool RemoveNodeFromList(IEnumerable<UINode> list, IDictionary<UINode, RectTransform> activeNodes)
    {
        bool needSort = false;
        
        foreach (var node in list)
        {
            if (!activeNodes.ContainsKey(node)) continue;
            activeNodes.Remove(node);
            needSort = true;
        }
        return needSort;
    }

    public static void CheckAndRemoveBranch(IBranch newBranch, IDictionary<IBranch, RectTransform> activeBranches)
    {
        if (activeBranches.ContainsKey(newBranch))
        {
            activeBranches.Remove(newBranch);
        }
    }

    public static void SortNodeList(IDictionary<UINode, RectTransform> sortedNodesDict, IDictionary<UINode, RectTransform> activeNodes)
    {
        sortedNodesDict.Clear();
        var sortedNodeList = activeNodes.Keys.OrderByDescending(node => node.MyBranch.MyCanvas.sortingOrder).ToList();
        
        foreach (var node in sortedNodeList)
        {
            sortedNodesDict.Add(node, activeNodes[node]);
        }
    }
}