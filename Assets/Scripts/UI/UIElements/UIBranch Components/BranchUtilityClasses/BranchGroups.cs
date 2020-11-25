using System.Collections.Generic;
using System.Linq;

public static class BranchGroups
{
    private static int groupIndex;
    private static int index;

    public static int SetGroupIndex(INode defaultStartPosition, List<GroupList> branchGroupsList)
    {
        groupIndex = 0;
        index = 0;
        foreach (var branchGroup in branchGroupsList)
        {
            if (branchGroup._nodes.Any(node => ReferenceEquals(node, defaultStartPosition))) //TODO check this works
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
