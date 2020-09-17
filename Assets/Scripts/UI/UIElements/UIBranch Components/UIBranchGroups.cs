using System.Collections.Generic;
using UnityEngine.EventSystems;

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
            foreach (var node in branchGroup._nodes)
            {
                if (node != defaultStartPosition) continue;
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
       
        // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
        if (switchType == SwitchType.Positive)
        {
            newIndex = passedIndex.PositiveIterate(groupsList.Count);
        }
        else
        {
           newIndex = passedIndex.NegativeIterate(groupsList.Count);
        }
        groupsList[newIndex]._startNode.OnPointerEnter(new PointerEventData(EventSystem.current));
        return newIndex;
    }
}
