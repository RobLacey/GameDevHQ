using System;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

/// <summary>
/// This class Looks after switching between, clearing and correctly restoring the home screen branches. Main functionality
/// is for keyboard or controller. Differ from internal branch groups as involve Branches not Nodes
/// </summary>
public class UIHomeGroup : IMono
{
    private readonly UIBranch[] _homeGroup;
    private readonly UIBranch[] _allBranches;
    private bool _allowKeys;

    public UIHomeGroup(UIBranch[] homeBranches, UIBranch[] allBranches)
    {
        _allBranches = allBranches;
        _homeGroup = homeBranches;
        OnEnable();
    }
    
    public void OnEnable()
    {
        UIBranch.DoActiveBranch += SaveActiveBranch;
        UICancel.ReturnHomeBranch += CurrentHomeBranch;
        ChangeControl.DoAllowKeys += SaveAllowKeys;
    }

    public void OnDisable()
    {
        UIBranch.DoActiveBranch -= SaveActiveBranch;
        UICancel.ReturnHomeBranch -= CurrentHomeBranch;
        ChangeControl.DoAllowKeys -= SaveAllowKeys;
    }

    //Delegate
    public static event Action<bool> DoOnHomeScreen; // Subscribe To track if on Home Screen
    
    //Properties
    private UIBranch CurrentHomeBranch() => _homeGroup[Index];
    private void SaveAllowKeys(bool allow) => _allowKeys = allow;
    private bool OnHomeScreen { get; set; } = true;
    private int Index { get; set; }


    public void SwitchHomeGroups(SwitchType switchType)
    {
        SetNewIndex(switchType);
        if (ActivateHoverOverIfKeysAllowed())
        {
            _homeGroup[Index].LastSelected.PressedActions();
        }
        else
        {
            _homeGroup[Index].MoveToBranchWithoutTween();
        }
    }

    private bool ActivateHoverOverIfKeysAllowed() 
        => _homeGroup[Index].LastSelected.Function == ButtonFunction.HoverToActivate && _allowKeys;

    private void SetNewIndex(SwitchType switchType)
    {
        _homeGroup[Index].LastSelected.Deactivate();
        if (switchType == SwitchType.Positive)
        {
            Index = Index.PositiveIterate(_homeGroup.Length);
        }
        else
        {
            Index = Index.NegativeIterate(_homeGroup.Length);
        }
    }

    public void ClearHomeScreen(UIBranch ignoreThisBranch, IsActive turnOffPopUps)
    {
        if (!OnHomeScreen) return;
        OnHomeScreen = false;
        DoOnHomeScreen?.Invoke(OnHomeScreen);
        ProcessAllBranches(ignoreThisBranch, turnOffPopUps);
    }

    private void ProcessAllBranches(UIBranch ignoreThisBranch, IsActive turnOffPopUps)
    {
        foreach (var branch in _allBranches)
        {
            if (AlreadyOffOrCanIgnore(ignoreThisBranch, branch)) continue;
            if (CanTurnOffPopUps(turnOffPopUps, branch.IsNonResolvePopUp)) continue;
            branch.MyCanvas.enabled = false;
        }
    }

    private static bool CanTurnOffPopUps(IsActive turnOffPopUps, bool isPopUp)
    {
        return isPopUp && turnOffPopUps == IsActive.No;
    }

    private static bool AlreadyOffOrCanIgnore(UIBranch ignoreThisBranch, UIBranch branch)
    {
        return !branch.MyCanvas.enabled || branch == ignoreThisBranch;
    }

    public void RestoreHomeScreen()
    {
        if (OnHomeScreen) return;
        OnHomeScreen = true;
        DoOnHomeScreen?.Invoke(OnHomeScreen);
        
        foreach (var item in _homeGroup)
        {
            item.ResetHomeScreenBranch();
        }
    }

    private void SaveActiveBranch(UIBranch newBranch)
    {
        if (!OnHomeScreen) return;
        for (var index = 0; index < _homeGroup.Length; index++)
        {
            if (_homeGroup[index] != newBranch) continue;
            Index = index;
            break;
        }
    }
}
