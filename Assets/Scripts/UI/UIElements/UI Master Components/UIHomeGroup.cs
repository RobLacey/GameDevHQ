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

    public void SwitchHomeGroups(SwitchType switchType)
    {
        var index = ReturnNewIndex(switchType);
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

    private int ReturnNewIndex(SwitchType switchType)
    {
        int index = _uIHub.GroupIndex;
        _homeGroup[index].LastSelected.Deactivate();

        if (switchType == SwitchType.Positive)
        {
            return index.PositiveIterate(_homeGroup.Count);
        }
        return index.NegativeIterate(_homeGroup.Count);
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
    }

    public void ClearHomeScreen(UIBranch ignoreBranch, IsActive turnOffPopUps)
    {
        if (!_uIHub.OnHomeScreen) return;
        _uIHub.OnHomeScreen = false;

        foreach (var branch in _allBranches)
        {
            if(branch.IsNonResolvePopUp && turnOffPopUps == IsActive.Yes) continue;
            if (branch == ignoreBranch) continue;

            if (branch.MyCanvas.enabled)
            {
                branch.MyCanvas.enabled = false;
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
