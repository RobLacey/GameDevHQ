
using System;
using System.Collections.Generic;
using EZ.Events;
using EZ.Service;
using UnityEngine;

public interface IHomeGroup
{
    void OnEnable();
    void SetUpHomeGroup();
    void SwitchHomeGroups(SwitchType switchType);
}

/// <summary>
/// This class Looks after switching between, clearing and correctly restoring the home screen branches. Main functionality
/// is for keyboard or controller. Differ from internal branch groups as involve Branches not Nodes
/// </summary>
public class UIHomeGroup : IEZEventUser, IHomeGroup, ISwitchGroupPressed, IEZEventDispatcher, IServiceUser
{
    //Variables
    private bool _onHomeScreen = true;
    private bool _gameIsPaused;
    private int _index = 0;
    private IBranch _lastActiveHomeBranch;
    private bool _allowKeys;
    private IBranch _activeBranch;
    private IHub _myUIHub;

    //Properties and Getters / Setters
    private IBranch[] HomeGroup => _myUIHub.HomeBranches.ToArray();
    private void SaveOnHomeScreen(IOnHomeScreen args) => _onHomeScreen = args.OnHomeScreen;
    private void GameIsPaused(IGameIsPaused args) => _gameIsPaused = args.GameIsPaused;
    private void SaveAllowKeys(IAllowKeys args) => _allowKeys = args.CanAllowKeys;
    public SwitchType SwitchType { get; }

    //Events
    private Action<ISwitchGroupPressed> OnSwitchGroupPressed { get; set; }

    //Main
    public void OnEnable()
    {
        UseEZServiceLocator();
        FetchEvents();
        ObserveEvents();
    }

    public void UseEZServiceLocator() => _myUIHub = EZService.Locator.Get<IHub>(this);

    public void OnDisable() { }

    public void FetchEvents() => OnSwitchGroupPressed = InputEvents.Do.Fetch<ISwitchGroupPressed>();

    public void ObserveEvents()
    {
        HistoryEvents.Do.Subscribe<IReturnToHome>(ActivateHomeGroupBranch);
        HistoryEvents.Do.Subscribe<IActiveBranch>(SetActiveHomeBranch);
        HistoryEvents.Do.Subscribe<IGameIsPaused>(GameIsPaused);
        HistoryEvents.Do.Subscribe<IOnHomeScreen>(SaveOnHomeScreen);
        HistoryEvents.Do.Subscribe<IReturnHomeGroupIndex>(ReturnHomeGroup);
        HistoryEvents.Do.Subscribe<IHighlightedNode>(SaveHighlighted);
        InputEvents.Do.Subscribe<IAllowKeys>(SaveAllowKeys);
    }

    public void SetUpHomeGroup() => _activeBranch = HomeGroup[_index];

    private void SaveHighlighted(IHighlightedNode args)
    {
        if(_allowKeys) return;
        
        if (IsHomeScreenBranchAndNoChildrenOpen())
        {
            SearchHomeBranchesAndSet(args.Highlighted.MyBranch);
        }

        bool IsHomeScreenBranchAndNoChildrenOpen() 
            => args.Highlighted.MyBranch.IsHomeScreenBranch() && _activeBranch.IsHomeScreenBranch();
    }

    private void ReturnHomeGroup(IReturnHomeGroupIndex args) => args.TargetNode = HomeGroup[_index].LastHighlighted;

    public void SwitchHomeGroups(SwitchType switchType)
    {
        if (!_onHomeScreen) return;
        if(HomeGroup.Length > 1)
            OnSwitchGroupPressed?.Invoke(this);
        SetNewIndex(switchType);
    }

    private void SetNewIndex(SwitchType switchType)
    {
        switch (switchType)
        {
            case SwitchType.Positive:
                _index = _index.PositiveIterate(HomeGroup.Length);
                break;
            case SwitchType.Negative:
                _index = _index.NegativeIterate(HomeGroup.Length);
                break;
            case SwitchType.Activate:
                break;
        }
        _lastActiveHomeBranch = HomeGroup[_index];
        HomeGroup[_index].MoveToThisBranch();
    }

    private void SetActiveHomeBranch(IActiveBranch args)
    {
        _activeBranch = args.ActiveBranch;
        if(DontDoSearch(_activeBranch)) return;
        if(_lastActiveHomeBranch == _activeBranch) return;
        
        _lastActiveHomeBranch = _activeBranch;
        FindHomeScreenBranch(_activeBranch);
    }

    private bool DontDoSearch(IBranch newBranch) 
        => newBranch.IsAPopUpBranch() || newBranch.IsPauseMenuBranch() 
                                              || newBranch.IsInGameBranch() 
                                              || _gameIsPaused;

    private void FindHomeScreenBranch(IBranch newBranch)
    {
        while (!newBranch.IsHomeScreenBranch() && !DontDoSearch(newBranch))
        {
            newBranch = newBranch.MyParentBranch;
        }
        
        SearchHomeBranchesAndSet(newBranch);
    }

    private void SearchHomeBranchesAndSet(IBranch newBranch)
    {
        if(!newBranch.IsHomeScreenBranch()) return;
        
        for (var index = 0; index < HomeGroup.Length; index++)
        {
            if (HomeGroup[index] != newBranch) continue;
            _index = index;
            break;
        }
    }

    private void ActivateHomeGroupBranch(IReturnToHome args)
    {
        HomeGroup[_index].MoveToThisBranch();
        
        foreach (var branch in HomeGroup)
        {
            if(branch == HomeGroup[_index]) continue;
            branch.DontSetBranchAsActive();
            branch.MoveToThisBranch();
        }
    }

}
