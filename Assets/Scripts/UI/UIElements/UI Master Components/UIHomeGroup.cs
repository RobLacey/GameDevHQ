using System;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

public class UIHomeGroup
{
    List<UIBranch> _homeGroup;
    UIBranch[] _allBranches;
    UIHub _uIHub;

    public UIHomeGroup(UIHub uIHub)
    {
        _uIHub = uIHub;
        _allBranches = _uIHub.AllBranches;
        _homeGroup = _uIHub.HomeGroupBranches;
    }

    public void SwitchHomeGroups()
    {
        int index = _uIHub.GroupIndex;
        _homeGroup[index].LastSelected.Deactivate();
        index++;

        if (index > _homeGroup.Count - 1)
        {
            index = 0;
        }
        _uIHub.GroupIndex = index;
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

    public void SetHomeGroupIndex(UIBranch uIBranch)
    {
        for (int i = 0; i < _homeGroup.Count; i++)
        {
            if (_homeGroup[i] == uIBranch)
            {
                _uIHub.GroupIndex = i;
            }
        }
        //return 0;
    }

    public void ClearHomeScreen(UIBranch ignoreBranch, IsActive turnOffPopUps)
    {
        if (!_uIHub.OnHomeScreen) return;
        _uIHub.OnHomeScreen = false;

        foreach (var item in _allBranches)
        {
            if(item.IsNonResolvePopUp && turnOffPopUps == IsActive.Yes) continue;
            if (item == ignoreBranch) continue;

            if (item.MyCanvas.enabled)
            {
                item.MyCanvas.enabled = false;
            }
        }
    }

    public void RestoreHomeScreen()
    {
        if (!_uIHub.OnHomeScreen)
        {
            foreach (var item in _homeGroup)
            {
                _uIHub.OnHomeScreen = true;
                item.ResetHomeScreenBranch(_homeGroup[_uIHub.GroupIndex]);
            }
        }
    }
}
