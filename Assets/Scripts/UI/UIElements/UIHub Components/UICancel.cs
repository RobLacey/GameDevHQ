using System;

/// <summary>
/// Class handles all UI cancel behaviour from cancel type to context sensitive cases
/// </summary>
public class UICancel
{
    public UICancel(EscapeKey globalSetting)
    {
        _globalEscapeSetting = globalSetting;
        OnEnable();
    }

    //Variables
    private readonly EscapeKey _globalEscapeSetting;
    private readonly UIDataEvents _uiDataEvents = new UIDataEvents();
    private readonly UIControlsEvents _uiControlsEvents = new UIControlsEvents();
    private readonly UIPopUpEvents _uiPopUpEvents = new UIPopUpEvents();
    private bool _fromHotKey;
    private UIBranch _currentHomeBranch;
    private INode _lastSelected;
    private INode _lastHighlighted;
    private UIBranch _activeBranch;
    private bool _gameIsPaused;
    private bool _noResolvePopUps = true;
    private bool _noPopUps = true;

    //Events
    public static event Action<UIBranch> OnBackOneLevel; 
    public static event Action<UIBranch> OnBackToAPopUp; 

    //Properties
    private void SaveSelected(INode newNode) => _lastSelected = newNode;
    private void SaveLastHighlighted(INode newNode) => _lastHighlighted = newNode;
    private void SaveActiveBranch(UIBranch newBranch) => _activeBranch = newBranch;
    private void SaveFromHotKey() => _fromHotKey = true;
    private void SaveCurrentHomeBranch(UIBranch currentHomeBranch) => _currentHomeBranch = currentHomeBranch;
    private void SaveNoResolvePopUps(bool noResolvePopUps) => _noResolvePopUps = noResolvePopUps;
    private void SaveNoPopUps(bool noPopUps) => _noPopUps = noPopUps;
    private void SaveGameIsPaused(bool isPaused) => _gameIsPaused = isPaused;

    private void OnEnable()
    {
        _uiDataEvents.SubscribeToSelectedNode(SaveSelected);
        _uiDataEvents.SubscribeToActiveBranch(SaveActiveBranch);
        _uiDataEvents.SubscribeToCurrentHomeScreen(SaveCurrentHomeBranch);
        _uiDataEvents.SubscribeSetUpBranchesAtStart(SaveCurrentHomeBranch);
        _uiDataEvents.SubscribeToHighlightedNode(SaveLastHighlighted);
        _uiDataEvents.SubscribeToGameIsPaused(SaveGameIsPaused);
        _uiControlsEvents.SubscribeFromHotKey(SaveFromHotKey);
        _uiControlsEvents.SubscribeCancelOrBackButtonPressed(ProcessCancelType);
        _uiControlsEvents.SubscribeOnCancel(CancelPressed);
        _uiPopUpEvents.SubscribeNoResolvePopUps(SaveNoResolvePopUps);
        _uiPopUpEvents.SubscribeNoPopUps(SaveNoPopUps);
    }

    private void CancelPressed()
    {
        if(!_noResolvePopUps && !_gameIsPaused) return;
        if(_lastHighlighted.MyBranch.IsTimedPopUp)
        {
            CancelTimedPopUp();
            return;
        }

        ProcessCancelType(_fromHotKey ? EscapeKey.BackToHome : _activeBranch.EscapeKeySetting);
    }

    private void ProcessCancelType(EscapeKey escapeKey)
    {
        if (escapeKey == EscapeKey.GlobalSetting) escapeKey = _globalEscapeSetting;

        switch (escapeKey)
        {
            case EscapeKey.BackOneLevel:
                StartCancelProcess(BackOneLevel);
                break;
            case EscapeKey.BackToHome:
                StartCancelProcess(BackToHome);
                break;
        }
    }

    private void StartCancelProcess(Action endOfCancelAction) 
    {
        if (HasActivePopUps() && !_gameIsPaused)
        {
            _lastHighlighted.MyBranch.StartOutTweenProcess(OutTweenType.Cancel);
            ToNextPopUp();
        }
        else
        {
            _activeBranch.StartOutTweenProcess(OutTweenType.Cancel, endOfCancelAction);
        }
    }

    private bool HasActivePopUps() => !_noPopUps && _lastHighlighted.MyBranch.IsAPopUpBranch();
    private void BackOneLevel() => InvokeCancelEvent(_lastSelected.MyBranch);
    private void BackToHome() => InvokeCancelEvent(_currentHomeBranch);
    private void CancelTimedPopUp() => InvokeCancelEvent(_lastHighlighted.MyBranch);
    private void ToNextPopUp() => OnBackToAPopUp?.Invoke(_lastHighlighted.MyBranch);
    private static void InvokeCancelEvent(UIBranch targetBranch) => OnBackOneLevel?.Invoke(targetBranch);
}
