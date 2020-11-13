﻿using UnityEngine;

public interface IHomeGroup : IMonoBehaviourSub { }

/// <summary>
/// This class Looks after switching between, clearing and correctly restoring the home screen branches. Main functionality
/// is for keyboard or controller. Differ from internal branch groups as involve Branches not Nodes
/// </summary>
public class UIHomeGroup : IEventUser, IHomeGroup
{
    public UIHomeGroup(UIBranch[] homeGroupBranches)
    {
        _homeGroup = homeGroupBranches;
        OnEnable();
    }
    
    //Variables
    private readonly UIBranch[] _homeGroup;
    private bool _onHomeScreen = true;
    private bool _gameIsPaused;
    private int _index;
    private UIBranch _lastActiveHomeBranch;
    
    //Properties
    private void SaveOnHomeScreen(IOnHomeScreen args) => _onHomeScreen = args.OnHomeScreen;
    private void GameIsPaused(IGameIsPaused args) => _gameIsPaused = args.GameIsPaused;

    public void OnEnable() => ObserveEvents();

    public void OnDisable() => RemoveFromEvents();

    public void ObserveEvents()
    {
        EventLocator.Subscribe<IReturnToHome>(ActivateHomeGroupBranch, this);
        EventLocator.Subscribe<ISwitchGroupPressed>(SwitchHomeGroups, this);
        EventLocator.Subscribe<IActiveBranch>(SetActiveHomeBranch, this);
        EventLocator.Subscribe<IGameIsPaused>(GameIsPaused, this);
        EventLocator.Subscribe<IOnHomeScreen>(SaveOnHomeScreen, this);
    }
    public void RemoveFromEvents()
    {
        EventLocator.Unsubscribe<IReturnToHome>(ActivateHomeGroupBranch);
        EventLocator.Unsubscribe<ISwitchGroupPressed>(SwitchHomeGroups);
        EventLocator.Unsubscribe<IActiveBranch>(SetActiveHomeBranch);
        EventLocator.Unsubscribe<IGameIsPaused>(GameIsPaused);
        EventLocator.Unsubscribe<IOnHomeScreen>(SaveOnHomeScreen);
    }

    private void SwitchHomeGroups(ISwitchGroupPressed args)
    {
        if (!_onHomeScreen) return;
        if(_homeGroup.Length == 1) return;
        SetNewIndex(args.SwitchType);
    }

    private void SetNewIndex(SwitchType switchType)
    {
        switch (switchType)
        {
            case SwitchType.Positive:
                _index = _index.PositiveIterate(_homeGroup.Length);
                break;
            case SwitchType.Negative:
                _index = _index.NegativeIterate(_homeGroup.Length);
                break;
        }
        _lastActiveHomeBranch = _homeGroup[_index];
        _homeGroup[_index].MoveToBranchWithoutTween();
    }

    private void SetActiveHomeBranch(IActiveBranch args)
    {
        if(ActiveBranchIsPopUpOrPaused(args)) return;
        if(_lastActiveHomeBranch == args.ActiveBranch) return;
        
        _lastActiveHomeBranch = args.ActiveBranch;
        FindHomeScreenBranch(args.ActiveBranch);
    }

    private bool ActiveBranchIsPopUpOrPaused(IActiveBranch args) 
        => args.ActiveBranch.IsAPopUpBranch() || args.ActiveBranch.IsPauseMenuBranch() || _gameIsPaused;

    private void FindHomeScreenBranch(UIBranch newBranch)
    {
        while (!newBranch.IsHomeScreenBranch())
        {
            newBranch = newBranch.MyParentBranch;
        }
        SearchHomeBranchesAndSet(newBranch);
    }

    private void SearchHomeBranchesAndSet(UIBranch newBranch)
    {
        for (var index = 0; index < _homeGroup.Length; index++)
        {
            if (_homeGroup[index] != newBranch) continue;
            _index = index;
            break;
        }
    }

    private void ActivateHomeGroupBranch(IReturnToHome args)
    {
        if (!args.ActivateOnReturnHome)
        {
            _homeGroup[_index].DontSetBranchAsActive();
        }
        _homeGroup[_index].MoveToBranchWithoutTween();
    }
}
