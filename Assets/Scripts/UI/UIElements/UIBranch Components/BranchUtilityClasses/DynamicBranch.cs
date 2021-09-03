using System.Linq;
using EZ.Service;

public static class DynamicBranch
{
    public static void AddNodeToBranch(IDynamicBranch branch)
    {
        var noNodes = branch.ThisGroupsUiNodes.Length == 0;
        
        branch.SetThisGroupsNode(BranchChildNodeUtil.GetChildNodes(branch.ThisBranch));
        
        if (noNodes)
        {
            branch.DefaultStartOnThisNode = branch.ThisGroupsUiNodes.First();
            branch.LastHighlighted = branch.DefaultStartOnThisNode;
            branch.LastSelected = branch.DefaultStartOnThisNode;
        }
    }

    public static void RemoveNodeFromBranch(IDynamicBranch branch, INode nodeToRemove)
    {
        branch.SetThisGroupsNode(BranchChildNodeUtil.RemoveNode(branch.ThisGroupsUiNodes, nodeToRemove));

        if (branch.ThisGroupsUiNodes.Length == 0 && !branch.IsInGameBranch())
        {
            EZService.Locator.LateGet<IHistoryTrack>().ReturnToNextHomeGroup();
            return;
        }

        SetValuesToNextNode(branch, nodeToRemove);
    }

    private static void SetValuesToNextNode(IDynamicBranch branch, INode nodeToRemove)
    {
        if(branch.ThisGroupsUiNodes.Length == 0) return;
        
        if (branch.LastHighlighted == nodeToRemove && branch.GetSaveOnExit == IsActive.Yes)
        {
            branch.ThisGroupsUiNodes.Last().SetNodeAsActive();
        }

        if (branch.LastSelected == nodeToRemove)
        {
            branch.LastSelected = branch.ThisGroupsUiNodes.Last();
        }
    }

}