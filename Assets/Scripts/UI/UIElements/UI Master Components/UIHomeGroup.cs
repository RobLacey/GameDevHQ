﻿using System;
using System.Collections.Generic;

public static class UIHomeGroup 
{
    public static List<UIBranch> homeGroup;
    public static UIMasterController uIMaster;

    public static int SwitchHomeGroups(List<UIBranch> homeGroup, int index)
    {
        homeGroup[index].LastSelected.Deactivate();
        homeGroup[index].LastSelected.SetNotHighlighted();

        index++;
        if (index > homeGroup.Count - 1)
        {
            index = 0;
        }
        homeGroup[index].TweenOnChange = false;
        homeGroup[index].MoveToNextLevel();
        return index;
    }

    public static int SetHomeGroupIndex(UIBranch uIBranch, List<UIBranch> homeGroup, int index)
    {
        for (int i = 0; i < homeGroup.Count; i++)
        {
            if (homeGroup[i] == uIBranch)
            {
                return i;
            }
        }
        return index;
    }

    public static void ClearHomeScreen(UIBranch ignoreBranch)
    {
        if (!uIMaster.OnHomeScreen) return;
        uIMaster.OnHomeScreen = false;

        foreach (var branch in homeGroup)
        {
            if (branch != ignoreBranch)
            {
                branch.LastSelected.Deactivate();
                branch.MyCanvas.enabled = false;
            }
            branch.LastHighlighted.SetNotHighlighted();
        }
    }

    public static void RestoreHomeScreen()
    {
        if (!uIMaster.OnHomeScreen)
        {
            foreach (var item in homeGroup)
            {
                uIMaster.OnHomeScreen = true;
                item.ResetHomeScreenBranch(uIMaster.LastSelected.MyBranch);
            }
        }
    }

}
