using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class BranchGroups
{
    private static int groupIndex;
    private static int index;
    
    public static void AddControlBarToGroupList(List<GroupList> groupsList,
                                                List<IBranch> homeBranches,
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

    private static void AddControlBarAsNewGroup(List<GroupList> groupsList, List<IBranch> homeBranches)
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
            StartNode = (UINode) branch.DefaultStartOnThisNode,
            GroupNodes = new UINode[branch.ThisGroupsUiNodes.Length]
        };
        newGroup.GroupNodes = branch.ThisGroupsUiNodes.Cast<UINode>().ToArray();
        return newGroup;
    }

    public static int SetGroupIndex(INode defaultStartPosition, List<GroupList> branchGroupsList)
    {
        groupIndex = 0;
        index = 0;
        foreach (var branchGroup in branchGroupsList)
        {
            if (branchGroup.GroupNodes.Any(node => ReferenceEquals(node, defaultStartPosition)))
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
        int newIndex = passedIndex;
        
        if (switchType == SwitchType.Positive)
        {
            newIndex = passedIndex.PositiveIterate(groupsList.Count);
        }
        if (switchType == SwitchType.Negative)
        {
           newIndex = passedIndex.NegativeIterate(groupsList.Count);
        }

        groupsList[newIndex].StartNode.SetNodeAsActive();
        return newIndex;
    }
}
