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

    public static int SwitchBranchGroup(List<GroupList> groupsList, int index)
    {
        groupsList[index]._startNode.SetNotHighlighted();

        if (index == groupsList.Count - 1)
        {
            index = 0;
        }
        else
        {
            index++;
        }
        groupsList[index]._startNode._navigation.NavigateToNext();
        return index;
    }

}
