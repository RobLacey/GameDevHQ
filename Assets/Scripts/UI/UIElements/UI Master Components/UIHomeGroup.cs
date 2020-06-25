using System;
using System.Collections.Generic;
using UnityEngine;

public static class UIHomeGroup 
{
    public static List<UIBranch> _homeGroup;
    public static UIBranch[] _allBranches;
    public static UIHub _myUIHub;

    public static void SwitchHomeGroups(ref int index)
    {
        _homeGroup[index].LastSelected.Deactivate();
        index++;

        if (index > _homeGroup.Count - 1)
        {
            index = 0;
        }
        _homeGroup[index].TweenOnChange = false;

        if (_homeGroup[index].LastSelected.Function == ButtonFunction.HoverToActivate && _homeGroup[index].AllowKeys)
        {
            _homeGroup[index].LastSelected.PressedActions();
        }
        else
        {
            _homeGroup[index].MoveToNextLevel();
        }
    }

    public static int SetHomeGroupIndex(UIBranch uIBranch)
    {
        for (int i = 0; i < _homeGroup.Count; i++)
        {
            if (_homeGroup[i] == uIBranch)
            {
                return i;
            }
        }
        return 0;
    }

    public static void ClearHomeScreen(UIBranch ignoreBranch)
    {
        if (!_myUIHub.OnHomeScreen) return;
        _myUIHub.OnHomeScreen = false;

        foreach (var item in _allBranches)
        {
            if (item == ignoreBranch) continue;

            if (item.MyCanvas.enabled)
            {
                item.MyCanvas.enabled = false;
            }
        }
    }

    public static void RestoreHomeScreen()
    {
        if (!_myUIHub.OnHomeScreen)
        {
            foreach (var item in _homeGroup)
            {
                _myUIHub.OnHomeScreen = true;
                item.ResetHomeScreenBranch(_homeGroup[_myUIHub.GroupIndex]);
            }
        }
        _myUIHub.ActivePopUps_NonResolve.Clear();
    }
}
