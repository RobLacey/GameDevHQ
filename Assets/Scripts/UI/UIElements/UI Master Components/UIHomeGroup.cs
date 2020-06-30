using System;
using System.Collections.Generic;
using UnityEngine;

public class UIHomeGroup : IHomeGroup
{
    List<UIBranch> _homeGroup;
    UIBranch[] _allBranches;
    IHubData _myUIHub;

    public UIHomeGroup(IHubData hubData)
    {
        _myUIHub = hubData;
        _allBranches = _myUIHub.AllBranches;
        _homeGroup = _myUIHub.HomeGroupBranches;
    }

    public void SwitchHomeGroups()
    {
        int index = _myUIHub.GroupIndex;
        _homeGroup[index].LastSelected.Deactivate();
        index++;

        if (index > _homeGroup.Count - 1)
        {
            index = 0;
        }
        _myUIHub.GroupIndex = index;
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
                _myUIHub.GroupIndex = i;
            }
        }
        //return 0;
    }

    public void ClearHomeScreen(UIBranch ignoreBranch)
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

    public void RestoreHomeScreen()
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
