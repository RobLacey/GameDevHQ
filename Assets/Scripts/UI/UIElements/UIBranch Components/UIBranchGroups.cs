using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class UIBranchGroups
{
    public static int SetGroupIndex(UINode DefaultStartPosition, List<GroupList> _groupsList)
    {
        int groupIndex = 0;

        if (DefaultStartPosition && _groupsList.Count > 0)
        {
            int index = 0;
            for (int i = 0; i < _groupsList.Count; i++)
            {
                foreach (var item in _groupsList[i]._nodes)
                {
                    if (item == DefaultStartPosition)
                    {
                        groupIndex = index;
                        break;
                    }
                }
                index++;
            }
        }
        return groupIndex;
    }

    public static int SwitchBranchGroup(List<GroupList> groupsList, int groupIndex, SwitchType switchType)
    {
        int newIndex;
        groupsList[groupIndex]._startNode.SetNotHighlighted();
        
        if (switchType == SwitchType.Positive)
        {
            //newIndex = PositveSwitch(groupsList, groupIndex);
            newIndex = groupIndex.PositiveIterate(groupsList.Count);
        }
        else
        {
            //newIndex = NegativeSwitch(groupsList, groupIndex);
            newIndex = groupIndex.NegativeIterate(groupsList.Count);
        }
        groupsList[newIndex]._startNode.Navigation.NavigateToNextNode();
        return newIndex;
    }

    // private static int PositveSwitch(List<GroupList> groupsList, int groupIndex)
    // {
    //     if (groupIndex == groupsList.Count - 1)
    //     {
    //         return 0;
    //     }
    //     return groupIndex + 1;
    // }
    //
    // private static int NegativeSwitch(List<GroupList> groupsList, int groupIndex)
    // {
    //     if (groupIndex == 0)
    //     {
    //         return groupsList.Count - 1;
    //     }
    //     return groupIndex - 1;
    // }
}
