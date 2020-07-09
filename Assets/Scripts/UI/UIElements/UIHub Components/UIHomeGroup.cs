﻿using System;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

public class UIHomeGroup
{
    readonly UIBranch[] _homeGroup;
    readonly UIBranch[] _allBranches;
    readonly UIHub _uIHub;

    public UIHomeGroup(UIHub uIHub, UIBranch[] homeBranches)
    {
        _uIHub = uIHub;
        _allBranches = _uIHub.AllBranches;
        _homeGroup = homeBranches;
        
    }

    public void SwitchHomeGroups(SwitchType switchType)
    {
        var index = ReturnNewIndex(switchType);
        _uIHub.HomeGroupIndex = index;
        _homeGroup[index].TweenOnChange = false;

        if (_homeGroup[index].LastSelected.Function == ButtonFunction.HoverToActivate && _homeGroup[index].AllowKeys)
        {
            _homeGroup[index].LastSelected.PressedActions();
        }
        else
        {
            _homeGroup[index].MoveToThisBranch();
        }
    }

    private int ReturnNewIndex(SwitchType switchType)
    {
        int index = _uIHub.HomeGroupIndex;
        _homeGroup[index].LastSelected.Deactivate();

        if (switchType == SwitchType.Positive)
        {
            return index.PositiveIterate(_homeGroup.Length);
        }
        return index.NegativeIterate(_homeGroup.Length);
    }

    public void SetHomeGroupIndex(UIBranch uIBranch)
    {
        for (int i = 0; i < _homeGroup.Length; i++)
        {
            if (_homeGroup[i] == uIBranch)
            {
                _uIHub.HomeGroupIndex = i;
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
                item.ResetHomeScreenBranch(_homeGroup[_uIHub.HomeGroupIndex]);
            }
        }
    }
}