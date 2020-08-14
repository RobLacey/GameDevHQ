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
        OnEnable();
    }

    //Variables
    private readonly UIBranch[] _homeGroup;
    private readonly UIBranch[] _allBranches;
    private readonly UIDataEvents _uiDataEvents = new UIDataEvents();
    private readonly UIControlsEvents _uiControlsEvents = new UIControlsEvents();
    private bool _allowKeys;
    private bool _fromHotKey;
    private bool _onHomeScreen;
    private int _index;

    //Delegate
    public static event Action<UIBranch> DoSetCurrentHomeBranch; // Subscribe To track if on Home Screen
    
    //Properties
    private void SaveAllowKeys(bool allow) => _allowKeys = allow;
    private void SaveFromHotKey() => _fromHotKey = true;
    private void SetLastHighlightedBranch(UINode newNode) => SaveActiveBranch(newNode.MyBranch);

    private void OnEnable()
    {
        _uiDataEvents.SubscribeToActiveBranch(SaveActiveBranch);
        _uiDataEvents.SubscribeToOnHomeScreen(SaveOnHomeScreen);
        _uiDataEvents.SubscribeToOnStart(SetStartPosition);
        _uiDataEvents.SubscribeToHighlightedNode(SetLastHighlightedBranch);
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

    private void SetStartPosition() => DoSetCurrentHomeBranch?.Invoke(_homeGroup[0]);

    private void SwitchHomeGroups(SwitchType switchType)
    {
        if (!_onHomeScreen) return;
        if(_homeGroup.Length == 1) return;
        
        SetNewIndex(switchType);
        
        if (ActivateHoverOverIfKeysAllowed())
        {
            _homeGroup[_index].LastSelected.PressedActions();
        }
        else
        {
            _homeGroup[_index].MoveToBranchWithoutTween();
        }
        
    }

    private bool ActivateHoverOverIfKeysAllowed() 
        => _homeGroup[_index].LastSelected.Function == ButtonFunction.HoverToActivate && _allowKeys;

    private void SetNewIndex(SwitchType switchType)
    {
        _homeGroup[_index].LastSelected.Deactivate();
        
        // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
        if (switchType == SwitchType.Positive)
        {
            _index = _index.PositiveIterate(_homeGroup.Length);
        }
        else
        {
            _index = _index.NegativeIterate(_homeGroup.Length);
        }
        DoSetCurrentHomeBranch?.Invoke(_homeGroup[_index]);
    }

    private void ClearHomeScreen()
    {
        foreach (var branch in _allBranches)
        {
            branch.ClearBranch();
        }
    }
    
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
            SearchHomeBranchesAndSet(newBranch);
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
        SearchHomeBranchesAndSet(homeBranch);
    }

    private void SearchHomeBranchesAndSet(UIBranch newBranch)
    {
        for (var index = 0; index < _homeGroup.Length; index++)
        {
            if (_homeGroup[index] != newBranch) continue;
            _index = index;
            DoSetCurrentHomeBranch?.Invoke(_homeGroup[_index]);
            break;
        }
    }
}
