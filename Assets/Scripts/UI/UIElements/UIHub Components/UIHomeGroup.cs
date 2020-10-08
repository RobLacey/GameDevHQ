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
    private bool _onHomeScreen = true;
    private int _index;
    private UIBranch _lastActiveHomeBranch;
    
    //Properties
    private void SaveOnHomeScreen(bool onHomeScreen) => _onHomeScreen = onHomeScreen;

    private void OnEnable()
    {
        _uiDataEvents.SubscribeToActiveBranch(SaveActiveBranch);
        _uiDataEvents.SubscribeToOnHomeScreen(SaveOnHomeScreen);
        _uiControlsEvents.SubscribeSwitchGroups(SwitchHomeGroups);
        HistoryTracker.OnHome += SetHomeGroup;
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
        
         HistoryTracker.clearHistory?.Invoke();
    }

    private void SaveActiveBranch(UIBranch newBranch)
    {
        if(newBranch.IsAPopUpBranch() || newBranch.IsPauseMenuBranch()) return;
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
