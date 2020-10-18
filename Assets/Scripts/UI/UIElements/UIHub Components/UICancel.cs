using System;

/// <summary>
/// Class handles all UI cancel behaviour from cancel type to context sensitive cases
/// </summary>
public class UICancel : IServiceUser, IEventUser
{
    public UICancel(EscapeKey globalSetting)
    {
        _globalEscapeSetting = globalSetting;
        OnEnable();
    }

    //Variables
    private readonly EscapeKey _globalEscapeSetting;
    private readonly UIDataEvents _uiDataEvents = new UIDataEvents();
    private readonly UIPopUpEvents _uiPopUpEvents = new UIPopUpEvents();
    private INode _lastHighlighted;
    private UIBranch _activeBranch;
    private bool _gameIsPaused;
    private bool _noResolvePopUps = true;
    private bool _noPopUps = true;
    private IHistoryTrack _uiHistoryTrack;

    //Events
    public static event Action<UIBranch> OnBackOrCancel; 
    public static event Action<UIBranch> OnBackToAPopUp; 

    //Properties
    private void SaveLastHighlighted(INode newNode) => _lastHighlighted = newNode;
    private void SaveActiveBranch(UIBranch newBranch) => _activeBranch = newBranch;
    private void SaveNoResolvePopUps(bool noResolvePopUps) => _noResolvePopUps = noResolvePopUps;
    private void SaveNoPopUps(bool noPopUps) => _noPopUps = noPopUps;
    private void SaveGameIsPaused(bool isPaused) => _gameIsPaused = isPaused;

    private void OnEnable()
    {
        _uiDataEvents.SubscribeToActiveBranch(SaveActiveBranch);
        _uiPopUpEvents.SubscribeNoResolvePopUps(SaveNoResolvePopUps);
        _uiPopUpEvents.SubscribeNoPopUps(SaveNoPopUps);
        SubscribeToService();
        ObserveEvents();
    }

    public void OnDisable() => RemoveFromEvents();

    public void ObserveEvents()
    {
        EventLocator.SubscribeToEvent<ICancelPressed>(CancelPressed, this);
        EventLocator.SubscribeToEvent<ICancelButtonActivated, EscapeKey>(ProcessCancelType, this);
        EventLocator.SubscribeToEvent<IGameIsPaused, bool>(SaveGameIsPaused, this);
        EventLocator.SubscribeToEvent<IHighlightedNode, INode>(SaveLastHighlighted, this);
    }

    public void RemoveFromEvents()
    {
        EventLocator.UnsubscribeFromEvent<ICancelPressed>(CancelPressed);
        EventLocator.UnsubscribeFromEvent<ICancelButtonActivated, EscapeKey>(ProcessCancelType);
        EventLocator.UnsubscribeFromEvent<IGameIsPaused, bool>(SaveGameIsPaused);
        EventLocator.UnsubscribeFromEvent<IHighlightedNode, INode>(SaveLastHighlighted);
    }

    public void SubscribeToService()
    {
        _uiHistoryTrack = ServiceLocator.GetNewService<IHistoryTrack>(this);
    }


    private void CancelPressed()
    {
        if(!_noResolvePopUps && !_gameIsPaused) return;
        if(_lastHighlighted.MyBranch.IsTimedPopUp)
        {
            CancelTimedPopUp();
            return;
        }

        ProcessCancelType(_activeBranch.EscapeKeySetting);
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
            _lastHighlighted.PlayCancelAudio();
            _lastHighlighted.MyBranch.StartBranchExitProcess(OutTweenType.Cancel);
            ToNextPopUp();
        }
        else
        {
            _activeBranch.MyParentBranch.LastSelected.PlayCancelAudio();
            _activeBranch.StartBranchExitProcess(OutTweenType.Cancel, endOfCancelAction);
        }
    }

    private bool HasActivePopUps() => !_noPopUps && _lastHighlighted.MyBranch.IsAPopUpBranch();
    private void BackOneLevel() => _uiHistoryTrack.BackOneLevel();
    private void BackToHome() => _uiHistoryTrack.BackToHome();
    private void CancelTimedPopUp() => InvokeCancelEvent(_lastHighlighted.MyBranch);
    private void ToNextPopUp() => OnBackToAPopUp?.Invoke(_lastHighlighted.MyBranch);
    private static void InvokeCancelEvent(UIBranch targetBranch) => OnBackOrCancel?.Invoke(targetBranch);
}
