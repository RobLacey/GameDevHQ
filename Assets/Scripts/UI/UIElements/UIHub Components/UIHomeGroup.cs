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
    private bool _gameIsPaused = false;
    private int _index;
    private UIBranch _lastActiveHomeBranch;
    private IHistoryTrack _uiHistoryTrack;
    
    //Properties
    private void SaveOnHomeScreen(bool onHomeScreen) => _onHomeScreen = onHomeScreen;
    private void GameIsPaused(bool paused) => _gameIsPaused = paused;

    public void OnEnable()
    {
        SubscribeToService();
        ObserveEvents();
    }

    public void OnDisable() => RemoveFromEvents();

    public void ObserveEvents()
    {
        EventLocator.SubscribeToEvent<IReturnToHome>(SetHomeGroup, this);
        EventLocator.SubscribeToEvent<ISwitchGroupPressed, SwitchType>(SwitchHomeGroups, this);
        EventLocator.SubscribeToEvent<IActiveBranch, UIBranch>(SaveActiveBranch, this);
        EventLocator.SubscribeToEvent<IGameIsPaused, bool>(GameIsPaused, this);
        EventLocator.SubscribeToEvent<IOnHomeScreen, bool>(SaveOnHomeScreen, this);
    }
    public void RemoveFromEvents()
    {
        EventLocator.UnsubscribeFromEvent<IReturnToHome>(SetHomeGroup);
        EventLocator.UnsubscribeFromEvent<ISwitchGroupPressed, SwitchType>(SwitchHomeGroups);
        EventLocator.UnsubscribeFromEvent<IActiveBranch, UIBranch>(SaveActiveBranch);
        EventLocator.UnsubscribeFromEvent<IGameIsPaused, bool>(GameIsPaused);
        EventLocator.UnsubscribeFromEvent<IOnHomeScreen, bool>(SaveOnHomeScreen);
    }

    public void SubscribeToService()
    {
        _uiHistoryTrack = ServiceLocator.GetNewService<IHistoryTrack>(this);
        //return _uiHistoryTrack is null;
    }

    private void SwitchHomeGroups(SwitchType switchType)
    {
        if (!_onHomeScreen) return;
        if(_homeGroup.Length == 1) return;
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
        }
        _lastActiveHomeBranch = _homeGroup[_index];
        _homeGroup[_index].MoveToBranchWithoutTween();
        _uiHistoryTrack.ReverseAndClearHistory();
    }

    private void SaveActiveBranch(UIBranch newBranch)
    {
        if(newBranch.IsAPopUpBranch() || newBranch.IsPauseMenuBranch() || _gameIsPaused) return;
        if(_lastActiveHomeBranch == newBranch) return;
        _lastActiveHomeBranch = newBranch;
        FindHomeScreenBranch(newBranch);
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

    private void SetHomeGroup()
    {
        _homeGroup[_index].DontSetBranchAsActive();
        _homeGroup[_index].Branch.MoveBackToThisBranch(_homeGroup[_index]);
    }
}
