using System.Collections.Generic;
using System.Linq;

public static class BranchGroups
{
    private static int groupIndex;
    private static int index;
    
    public static void AddControlBarToGroupList(List<GroupList> groupsList,
                                                List<UIBranch> homeBranches,
                                                IBranch myBranch)
    {
        if(myBranch.ScreenType != ScreenType.FullScreen) return;
        
        bool hasControlBar = homeBranches.Any(homeBranch => homeBranch.IsControlBar());
        if(!hasControlBar) return;
        
        AddExistingNodesToGroupList(groupsList, myBranch);
        AddControlBarAsNewGroup(groupsList, homeBranches);
    }

    private static void AddExistingNodesToGroupList(List<GroupList> groupsList, IBranch myBranch)
    {
        if (groupsList.Count == 0)
            groupsList.Add(GroupList(myBranch));
    }

    private static void AddControlBarAsNewGroup(List<GroupList> groupsList, List<UIBranch> homeBranches)
    {
        foreach (var homeBranch in homeBranches.Where(homeBranch => homeBranch.IsControlBar()))
        {
            groupsList.Add(GroupList(homeBranch));
            return;
        }
    }

    private static GroupList GroupList(IBranch branch)
    {
        var newGroup = new GroupList
        {
            _startNode = (UINode) branch.DefaultStartOnThisNode,
            _nodes = new UINode[branch.ThisGroupsUiNodes.Length]
        };
        newGroup._nodes = branch.ThisGroupsUiNodes.Cast<UINode>().ToArray();
        return newGroup;
    }

    public static int SetGroupIndex(INode defaultStartPosition, List<GroupList> branchGroupsList)
    {
        groupIndex = 0;
        index = 0;
        foreach (var branchGroup in branchGroupsList)
        {
            if (branchGroup._nodes.Any(node => ReferenceEquals(node, defaultStartPosition)))
            {
                groupIndex = index;
                return groupIndex;
            }
            index++;
        }
        return groupIndex;
    }

    public static int SwitchBranchGroup(List<GroupList> groupsList, int passedIndex, SwitchType switchType)
    {
        int newIndex;
        
        if (switchType == SwitchType.Positive)
        {
            newIndex = passedIndex.PositiveIterate(groupsList.Count);
        }
        else
        {
           newIndex = passedIndex.NegativeIterate(groupsList.Count);
        }
        groupsList[newIndex]._startNode.SetNodeAsActive();
        return newIndex;
    }
}
