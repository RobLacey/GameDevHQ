using System.Collections.Generic;
using UnityEngine;

public static class SetBranchesChildNodes
{
    public static INode[] GetChildNodes(UIBranch branch)
    {
        var listOfChildren = new List<UINode>();

        foreach (var child in branch.gameObject.GetComponentsInChildren<Transform>())
        {
            if (CheckIfNestedUIBranch(child, branch)) break;
            CheckIfChildUINode(listOfChildren, child);
        }

        return listOfChildren.ToArray();
    }

    private static bool CheckIfNestedUIBranch(Transform child, UIBranch branch)
    {
        var isBranch = child.gameObject.GetComponent<UIBranch>();
        return isBranch != null && isBranch != branch;
    }

    private static void CheckIfChildUINode(List<UINode> listOfChildren, Transform child)
    {
        var isNode = child.GetComponent<UINode>();
        if (isNode)
            listOfChildren.Add(isNode);
    }
}