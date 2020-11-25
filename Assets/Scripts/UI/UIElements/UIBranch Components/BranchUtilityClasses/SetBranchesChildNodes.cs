using System.Collections.Generic;
using UnityEngine;

public class SetBranchesChildNodes
{
    public static INode[] GetChildNodes(IBranch branch)
    {
        var listOfChildren = new List<INode>();

        foreach (var child in branch.ThisBranchesGameObject.GetComponentsInChildren<Transform>())
        {
            if (CheckIfNestedUIBranch(child, branch)) break;
            CheckIfChildUINode(listOfChildren, child);
        }

        return listOfChildren.ToArray();
    }

    private static bool CheckIfNestedUIBranch(Transform child, IBranch branch)
    {
        var isBranch = child.gameObject.GetComponent<IBranch>();
        return isBranch != null && isBranch != branch;
    }

    private static void CheckIfChildUINode(List<INode> listOfChildren, Transform child)
    {
        var isNode = child.GetComponent<INode>();
        if (!(isNode is null))
            listOfChildren.Add(isNode);
    }
}