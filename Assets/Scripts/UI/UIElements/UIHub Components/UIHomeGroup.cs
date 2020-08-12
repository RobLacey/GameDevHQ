using System;

/// <summary>
/// This class Looks after switching between, clearing and correctly restoring the home screen branches. Main functionality
/// is for keyboard or controller. Differ from internal branch groups as involve Branches not Nodes
/// </summary>
public class UIHomeGroup
{
    public UIHomeGroup(UIBranch[] homeBranches, UIBranch[] allBranches)
    {
        _allBranches = allBranches;
        _homeGroup = homeBranches;
        _uiDataEvents = new UIDataEvents();
        _uiControlsEvents = new UIControlsEvents();
        OnEnable();
    }

    private readonly UIBranch[] _homeGroup;
    private readonly UIBranch[] _allBranches;
    private bool _allowKeys;
    private readonly UIDataEvents _uiDataEvents;
    private readonly UIControlsEvents _uiControlsEvents;
    private bool _fromHotKey;
    private bool _onHomeScreen;

    //Delegate
    public static event Action<UIBranch> DoCurrentHomeBranch; // Subscribe To track if on Home Screen
    
    //Properties
    private void SaveAllowKeys(bool allow) => _allowKeys = allow;
    private void SaveFromHotKey() => _fromHotKey = true;
    private int Index { get; set; }

    private void OnEnable()
    {
        _uiDataEvents.SubscribeToActiveBranch(SaveActiveBranch);
        _uiDataEvents.SubscribeToOnHomeScreen(SaveOnHomeScreen);
        _uiDataEvents.SubscribeToOnStart(SetStartPosition);
        _uiControlsEvents.SubscribeToAllowKeys(SaveAllowKeys);
        _uiControlsEvents.SubscribeFromHotKey(SaveFromHotKey);
        _uiControlsEvents.SubscribeSwitchGroups(SwitchHomeGroups);
    }

    private void SaveOnHomeScreen(bool onHomeScreen)
    {
        _onHomeScreen = onHomeScreen;
        if (_onHomeScreen)
        {
            RestoreHomeScreen();
        }
        else
        {
            ClearHomeScreen();
        }
    }

    private void SetStartPosition()
    {
        DoCurrentHomeBranch?.Invoke(_homeGroup[Index]);
    }
    
    private void SwitchHomeGroups(SwitchType switchType)
    {
        if (!_onHomeScreen) return;
        if(_homeGroup.Length == 1) return;
        
        SetNewIndex(switchType);
        DoCurrentHomeBranch?.Invoke(_homeGroup[Index]);
        
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

    private void ClearHomeScreen()
    {
        foreach (var branch in _allBranches)
        {
            if (AlreadyOffOrCanIgnore(branch)) continue;
            if (CanTurnOffPopUps(branch)) continue;
            branch.ClearBranch();
        }
    }
    
    private static bool CanTurnOffPopUps(UIBranch branch)
        => branch.IsOptionalPopUp && !branch.TurnOffPopUPs;

    private static bool AlreadyOffOrCanIgnore(UIBranch branch)
        => !branch.CanvasIsEnabled || branch.IgnoreThisBranch;

    private void RestoreHomeScreen()
    {
        foreach (var item in _homeGroup)
        {
            item.ResetHomeScreenBranch();
        }
    }

    private void SaveActiveBranch(UIBranch newBranch)
    {
        if (!_onHomeScreen) return;
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
