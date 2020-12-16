
using System;
using UnityEngine;

public interface IHomeGroup
{
    void OnEnable();
}

/// <summary>
/// This class Looks after switching between, clearing and correctly restoring the home screen branches. Main functionality
/// is for keyboard or controller. Differ from internal branch groups as involve Branches not Nodes
/// </summary>
public class UIHomeGroup : IEventUser, IHomeGroup, IIsAService
{
    public UIHomeGroup(IHub hub)
    {
        _homeGroup = hub.HomeBranches;

        foreach (var branch in _homeGroup)
        {
            if(!branch.IsHomeScreenBranch() && !branch.IsControlBar())
                throw new Exception(
                    $"{branch.ThisBranchesGameObject.name} isn't a Home Screen or Control Bar branch");
        }
    }

    //Variables
    private readonly IBranch[] _homeGroup;
    private bool _onHomeScreen = true;
    private bool _gameIsPaused;
    private int _index;
    private IBranch _lastActiveHomeBranch;

    //Properties

    private void SaveOnHomeScreen(IOnHomeScreen args) => _onHomeScreen = args.OnHomeScreen;

    private void GameIsPaused(IGameIsPaused args) => _gameIsPaused = args.GameIsPaused;
    
    public void OnEnable() => ObserveEvents();
    
    public void OnDisable() { }

    public void ObserveEvents()
    {
        EVent.Do.Subscribe<IReturnToHome>(ActivateHomeGroupBranch);
        EVent.Do.Subscribe<ISwitchGroupPressed>(SwitchHomeGroups);
        EVent.Do.Subscribe<IActiveBranch>(SetActiveHomeBranch);
        EVent.Do.Subscribe<IGameIsPaused>(GameIsPaused);
        EVent.Do.Subscribe<IOnHomeScreen>(SaveOnHomeScreen);
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
        _homeGroup[_index].MoveToThisBranch();
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

    private void FindHomeScreenBranch(IBranch newBranch)
    {
        while (!newBranch.IsHomeScreenBranch())
        {
            newBranch = newBranch.MyParentBranch;
        }
        SearchHomeBranchesAndSet(newBranch);
    }

    private void SearchHomeBranchesAndSet(IBranch newBranch)
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
        if (args.ActivateOnReturnHome == ActivateNodeOnReturnHome.No)
        {
            _homeGroup[_index].DontSetBranchAsActive();
        }
        _homeGroup[_index].MoveToThisBranch();
    }
}
