using System;

/// <summary>
/// This class Looks after switching between, clearing and correctly restoring the home screen branches. Main functionality
/// is for keyboard or controller. Differ from internal branch groups as involve Branches not Nodes
/// </summary>
public class UIHomeGroup : IMono
{
    public UIHomeGroup(UIBranch[] homeBranches, UIBranch[] allBranches)
    {
        _allBranches = allBranches;
        _homeGroup = homeBranches;
        _uiData = new UIData();
        OnEnable();
    }

    private readonly UIBranch[] _homeGroup;
    private readonly UIBranch[] _allBranches;
    private bool _allowKeys;
    private readonly UIData _uiData;
    private bool _fromHotKey;

    //Delegate
    public static event Action<bool> DoOnHomeScreen; // Subscribe To track if on Home Screen
    
    //Properties
    private UIBranch CurrentHomeBranch() => _homeGroup[Index];
    private void SaveAllowKeys(bool allow) => _allowKeys = allow;
    private void SaveFromHotKey() => _fromHotKey = true;
    private bool OnHomeScreen { get; set; } = true;
    private int Index { get; set; }

    public void OnEnable()
    {
        _uiData.SubscribeToActiveBranch(SaveActiveBranch);
        _uiData.SubscribeToOnStart(InvokeOnHomeScreen);
        _uiData.SubscribeToAllowKeys(SaveAllowKeys);
        _uiData.SubscribeFromHotKey(SaveFromHotKey);
        UICancel.ReturnHomeBranch += CurrentHomeBranch;
    }

    public void OnDisable()
    {
        _uiData.OnDisable();
        UICancel.ReturnHomeBranch -= CurrentHomeBranch;
    }

    private void InvokeOnHomeScreen()
    {
        DoOnHomeScreen?.Invoke(OnHomeScreen);
    }

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
        InvokeOnHomeScreen();
        TurnOfAllAllowableBranches(ignoreThisBranch, turnOffPopUps);
    }

    private void TurnOfAllAllowableBranches(UIBranch ignoreThisBranch, IsActive turnOffPopUps)
    {
        foreach (var branch in _allBranches)
        {
            if (AlreadyOffOrCanIgnore(ignoreThisBranch, branch)) continue;
            if (CanTurnOffPopUps(turnOffPopUps, branch.IsOptionalPopUp)) continue;
            branch.ClearBranch();
        }
    }

    private static bool CanTurnOffPopUps(IsActive turnOffPopUps, bool isPopUp)
        => isPopUp && turnOffPopUps == IsActive.No;

    private static bool AlreadyOffOrCanIgnore(UIBranch ignoreThisBranch, UIBranch branch) 
        => !branch.CanvasIsEnabled || branch == ignoreThisBranch;

    public void RestoreHomeScreen()
    {
        if (OnHomeScreen) return;
        OnHomeScreen = true;
        InvokeOnHomeScreen();
        
        foreach (var item in _homeGroup)
        {
            item.ResetHomeScreenBranch();
        }
    }

    private void SaveActiveBranch(UIBranch newBranch)
    {
        if (!OnHomeScreen) return;
        if (_fromHotKey)
        {
            FindHomeScreenBranch(newBranch);
        }
        else
        {
            SearchHomeBranches(newBranch);
        }
    }

    private void FindHomeScreenBranch(UIBranch newBranch)
    {
        _fromHotKey = false;
        UIBranch homeBranch = newBranch;
        
        while (homeBranch.MyParentBranch != homeBranch)
        {
            homeBranch = homeBranch.MyParentBranch;
        }
        SearchHomeBranches(homeBranch);
    }

    private void SearchHomeBranches(UIBranch newBranch)
    {
        for (var index = 0; index < _homeGroup.Length; index++)
        {
            if (_homeGroup[index] != newBranch) continue;
            Index = index;
            break;
        }
    }
}
