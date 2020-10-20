using System;

interface ICancel : IMonoBehaviourSub { }

/// <summary>
/// Class handles all UI cancel behaviour from cancel type to context sensitive cases
/// </summary>
public class UICancel : ICancel, IServiceUser, IEventUser
{
    public UICancel(EscapeKey globalSetting)
    {
        _globalEscapeSetting = globalSetting;
        OnEnable();
    }

    //Variables
    private readonly EscapeKey _globalEscapeSetting;
    private INode _lastHighlighted;
    private UIBranch _activeBranch;
    private bool _gameIsPaused;
    private bool _noResolvePopUps = true;
    private bool _noPopUps = true;
    private IHistoryTrack _uiHistoryTrack;

    //Events
    private static CustomEvent<IBackToNextPopUp, UIBranch> OnBackToAPopUp { get; } 
        = new CustomEvent<IBackToNextPopUp, UIBranch>();
    private static CustomEvent<IBackOrCancel, UIBranch> OnBackOrCancel { get;  } 
        = new CustomEvent<IBackOrCancel, UIBranch>();

    //Properties
    private void SaveLastHighlighted(INode newNode) => _lastHighlighted = newNode;
    private void SaveActiveBranch(UIBranch newBranch) => _activeBranch = newBranch;
    private void SaveNoResolvePopUps(bool noResolvePopUps) => _noResolvePopUps = noResolvePopUps;
    private void SaveNoPopUps(bool noPopUps) => _noPopUps = noPopUps;
    private void SaveGameIsPaused(bool isPaused) => _gameIsPaused = isPaused;

    public void OnEnable()
    {
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
        EventLocator.SubscribeToEvent<IActiveBranch, UIBranch>(SaveActiveBranch, this);
        EventLocator.SubscribeToEvent<INoPopUps, bool>(SaveNoPopUps, this);
        EventLocator.SubscribeToEvent<INoResolvePopUp, bool>(SaveNoResolvePopUps, this);
    }

    public void RemoveFromEvents()
    {
        EventLocator.UnsubscribeFromEvent<ICancelPressed>(CancelPressed);
        EventLocator.UnsubscribeFromEvent<ICancelButtonActivated, EscapeKey>(ProcessCancelType);
        EventLocator.UnsubscribeFromEvent<IGameIsPaused, bool>(SaveGameIsPaused);
        EventLocator.UnsubscribeFromEvent<IHighlightedNode, INode>(SaveLastHighlighted);
        EventLocator.UnsubscribeFromEvent<IActiveBranch, UIBranch>(SaveActiveBranch);
        EventLocator.UnsubscribeFromEvent<INoPopUps, bool>(SaveNoPopUps);
        EventLocator.UnsubscribeFromEvent<INoResolvePopUp, bool>(SaveNoResolvePopUps);
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
    private void ToNextPopUp() => OnBackToAPopUp?.RaiseEvent(_lastHighlighted.MyBranch);
    private static void InvokeCancelEvent(UIBranch targetBranch) => OnBackOrCancel?.RaiseEvent(targetBranch);
}
