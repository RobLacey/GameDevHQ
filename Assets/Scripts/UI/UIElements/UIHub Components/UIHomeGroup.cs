using System;
using UnityEngine;

/// <summary>
/// This class Looks after switching between, clearing and correctly restoring the home screen branches. Main functionality
/// is for keyboard or controller. Differ from internal branch groups as involve Branches not Nodes
/// </summary>
public class UIHomeGroup
{
    public UIHomeGroup(UIBranch[] homeBranches)
    {
        _homeGroup = homeBranches;
        OnEnable();
    }

    //Variables
    private readonly UIBranch[] _homeGroup;
    private readonly UIDataEvents _uiDataEvents = new UIDataEvents();
    private readonly UIControlsEvents _uiControlsEvents = new UIControlsEvents();
    private bool _fromHotKey;
    private bool _onHomeScreen = true;
    private int _index;
    private bool _afterStartUp;
    private UIBranch _lastActiveBranch;

    //Delegate
    public static event Action<UIBranch> DoSetCurrentHomeBranch; // Subscribe To track if on Home Screen
    
    //Properties
    private void SaveFromHotKey() => _fromHotKey = true;
    private void SaveOnHomeScreen(bool onHomeScreen) => _onHomeScreen = onHomeScreen;

    private void OnEnable()
    {
        _uiDataEvents.SubscribeToActiveBranch(SaveActiveBranch);
        _uiDataEvents.SubscribeToOnHomeScreen(SaveOnHomeScreen);
        _uiControlsEvents.SubscribeFromHotKey(SaveFromHotKey);
        _uiControlsEvents.SubscribeSwitchGroups(SwitchHomeGroups);
    }

    private void SwitchHomeGroups(SwitchType switchType)
    {
        if (!_onHomeScreen) return;
        if(_homeGroup.Length == 1) return;
        SetNewIndex(switchType);
        _homeGroup[_index].MoveToBranchWithoutTween();
    }

    private void SetNewIndex(SwitchType switchType)
    {
        _homeGroup[_index].LastSelected.DeactivateAndCancelChildren();
        
        switch (switchType)
        {
            case SwitchType.Positive:
                _index = _index.PositiveIterate(_homeGroup.Length);
                break;
            case SwitchType.Negative:
                _index = _index.NegativeIterate(_homeGroup.Length);
                break;
        }

        DoSetCurrentHomeBranch?.Invoke(_homeGroup[_index]);
    }

    private void SaveActiveBranch(UIBranch newBranch)
    {
        if(_lastActiveBranch == newBranch) return;
        _lastActiveBranch = newBranch;
        
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
            
            if(_afterStartUp) 
                DoSetCurrentHomeBranch?.Invoke(_homeGroup[_index]);
            
            _afterStartUp = true;
            break;
        }
    }
}
