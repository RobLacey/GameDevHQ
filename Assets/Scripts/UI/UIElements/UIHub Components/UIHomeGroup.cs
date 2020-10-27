using UnityEngine;

public interface IHomeGroup : IMonoBehaviourSub { }

/// <summary>
/// This class Looks after switching between, clearing and correctly restoring the home screen branches. Main functionality
/// is for keyboard or controller. Differ from internal branch groups as involve Branches not Nodes
/// </summary>
public class UIHomeGroup : IServiceUser, IEventUser, IHomeGroup
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
    private IHistoryTrack _uiHistoryTrack;
    
    //Properties
    private void SaveOnHomeScreen(IOnHomeScreen args) => _onHomeScreen = args.OnHomeScreen;
    private void GameIsPaused(IGameIsPaused args) => _gameIsPaused = args.GameIsPaused;

    public void OnEnable()
    {
        SubscribeToService();
        ObserveEvents();
    }

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

    public void SubscribeToService()
    {
        _uiHistoryTrack = ServiceLocator.GetNewService<IHistoryTrack>(this);
        //return _uiHistoryTrack is null;
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
        _uiHistoryTrack.ReverseAndClearHistory();
    }

    private void SetActiveHomeBranch(IActiveBranch args)
    {
        if(args.ActiveBranch.IsAPopUpBranch() || args.ActiveBranch.IsPauseMenuBranch() || _gameIsPaused) return;
        if(_lastActiveHomeBranch == args.ActiveBranch) return;
        _lastActiveHomeBranch = args.ActiveBranch;
        FindHomeScreenBranch(args.ActiveBranch);
    }

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
        if (!args.ActivateBranchOnReturnHome)
            _homeGroup[_index].DontSetBranchAsActive();
        _homeGroup[_index].Branch.MoveBackToThisBranch(_homeGroup[_index]);
    }
}
