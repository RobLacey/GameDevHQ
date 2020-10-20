using System.Collections.Generic;
using System.Linq;

public static class UIBranchGroups
{
    private static int groupIndex;
    private static int index;

    public static int SetGroupIndex(UINode defaultStartPosition, List<GroupList> branchGroupsList)
    {
        groupIndex = 0;
        index = 0;
        foreach (var branchGroup in branchGroupsList)
        {
            if (branchGroup._nodes.Any(node => node == defaultStartPosition))
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
        groupsList[newIndex]._startNode.HandleOnEnter();
        return newIndex;
    }
}
