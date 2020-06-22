using System;
using System.Collections.Generic;
using UnityEngine;

public static class UIHomeGroup 
{
    public static List<UIBranch> _homeGroup;
    public static List<UIBranch> _popUps = new List<UIBranch>();
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

    public static void SetHomeGroupIndex(UIBranch uIBranch, ref int index)
    {
        for (int i = 0; i < _homeGroup.Count; i++)
        {
            if (_homeGroup[i] == uIBranch)
            {
                index = i;
            }
        }
    }

    public static void ClearHomeScreen(UIBranch ignoreBranch)
    {
        if (!_myUIHub.OnHomeScreen) return;
        _myUIHub.OnHomeScreen = false;

        foreach (var item in _allBranches)
        {
            if (item == ignoreBranch) continue;

            if (item.MyBranchType != BranchType.PopUp && item.MyCanvas.enabled)
            {
                item.MyCanvas.enabled = false;
            }
        }

        foreach (var item in _popUps)
        {
            if (!item.RetainPopups)
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
                item.ResetHomeScreenBranch(_myUIHub.LastSelected.MyBranch);
            }
        }
    }

    public static void ClearAllPopUpsRegardless()
    {
        foreach (var item in _popUps)
        {
            item.MyCanvas.enabled = false;
        }
    }
}
