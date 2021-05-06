
using System;
using UnityEngine;

public interface IHomeGroup
{
    void OnEnable();
    void SetUpHomeGroup(IBranch[] homeGroupBranches);
    void SwitchHomeGroups(SwitchType switchType);
}

/// <summary>
/// This class Looks after switching between, clearing and correctly restoring the home screen branches. Main functionality
/// is for keyboard or controller. Differ from internal branch groups as involve Branches not Nodes
/// </summary>
public class UIHomeGroup : IEventUser, IHomeGroup, ISwitchGroupPressed, IEventDispatcher
{
    //Variables
    private IBranch[] _homeGroup;
    private bool _onHomeScreen = true;
    private bool _gameIsPaused;
    private int _index = 0;
    private IBranch _lastActiveHomeBranch;
    private bool _allowKeys;
    private IBranch _activeBranch;

    //Properties and Getters / Setters
    private void SaveOnHomeScreen(IOnHomeScreen args) => _onHomeScreen = args.OnHomeScreen;
    private void GameIsPaused(IGameIsPaused args) => _gameIsPaused = args.GameIsPaused;
    private void SaveAllowKeys(IAllowKeys args) => _allowKeys = args.CanAllowKeys;
    public SwitchType SwitchType { get; }

    //Events
    private Action<ISwitchGroupPressed> OnSwitchGroupPressed { get; set; }

    //Main
    public void OnEnable()
    {
        FetchEvents();
        ObserveEvents();
    }

    public void OnDisable() { }

    public void FetchEvents() => OnSwitchGroupPressed = EVent.Do.Fetch<ISwitchGroupPressed>();

    public void ObserveEvents()
    {
        EVent.Do.Subscribe<IReturnToHome>(ActivateHomeGroupBranch);
        EVent.Do.Subscribe<IActiveBranch>(SetActiveHomeBranch);
        EVent.Do.Subscribe<IGameIsPaused>(GameIsPaused);
        EVent.Do.Subscribe<IOnHomeScreen>(SaveOnHomeScreen);
        EVent.Do.Subscribe<IReturnHomeGroupIndex>(ReturnHomeGroup);
        EVent.Do.Subscribe<IHighlightedNode>(SaveHighlighted);
        EVent.Do.Subscribe<IAllowKeys>(SaveAllowKeys);
    }

    public void SetUpHomeGroup(IBranch[] homeGroupBranches)
    {
        _homeGroup = homeGroupBranches;
        _activeBranch = _homeGroup[_index];
    }
    
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

    private void ReturnHomeGroup(IReturnHomeGroupIndex args) => args.TargetNode = _homeGroup[_index].LastHighlighted;

    public void SwitchHomeGroups(SwitchType switchType)
    {
        if (!_onHomeScreen) return;
        if(_homeGroup.Length > 1)
            OnSwitchGroupPressed?.Invoke(this);
        SetNewIndex(switchType);
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
            case SwitchType.Activate:
                break;
        }
        _lastActiveHomeBranch = _homeGroup[_index];
        _homeGroup[_index].MoveToThisBranch();
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
        
        for (var index = 0; index < _homeGroup.Length; index++)
        {
            if (_homeGroup[index] != newBranch) continue;
            _index = index;
            break;
        }
    }

    private void ActivateHomeGroupBranch(IReturnToHome args)
    {
        _homeGroup[_index].MoveToThisBranch();
        
        foreach (var branch in _homeGroup)
        {
            if(branch == _homeGroup[_index]) continue;
            branch.DontSetBranchAsActive();
            branch.MoveToThisBranch();
        }
    }

}
