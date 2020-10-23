using System;
using UnityEngine;

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

    //Properties
    private void SaveLastHighlighted(IHighlightedNode args) => _lastHighlighted = args.Highlighted;
    private void SaveActiveBranch(IActiveBranch args) => _activeBranch = args.ActiveBranch;
    private void SaveNoResolvePopUps(INoResolvePopUp args) => _noResolvePopUps = args.NoActiveResolvePopUps;
    private void SaveNoPopUps(bool noPopUps) => _noPopUps = noPopUps;
    private void SaveGameIsPaused(IGameIsPaused args) => _gameIsPaused = args.GameIsPaused;

    public void OnEnable()
    {
        SubscribeToService();
        ObserveEvents();
    }

    public void OnDisable() => RemoveFromEvents();

    public void ObserveEvents()
    {
        EventLocator.Subscribe<ICancelPressed>(CancelPressed, this);
        EventLocator.Subscribe<ICancelButtonActivated>(EventPassedCancelType, this);
        EventLocator.Subscribe<IGameIsPaused>(SaveGameIsPaused, this);
        EventLocator.Subscribe<IHighlightedNode>(SaveLastHighlighted, this);
        EventLocator.Subscribe<IActiveBranch>(SaveActiveBranch, this);
        EventLocator.SubscribeToEvent<INoPopUps, bool>(SaveNoPopUps, this);
        EventLocator.Subscribe<INoResolvePopUp>(SaveNoResolvePopUps, this);
    }

    public void RemoveFromEvents()
    {
        EventLocator.Unsubscribe<ICancelPressed>(CancelPressed);
        EventLocator.Unsubscribe<ICancelButtonActivated>(EventPassedCancelType);
        EventLocator.Unsubscribe<IGameIsPaused>(SaveGameIsPaused);
        EventLocator.Unsubscribe<IHighlightedNode>(SaveLastHighlighted);
        EventLocator.Unsubscribe<IActiveBranch>(SaveActiveBranch);
        EventLocator.UnsubscribeFromEvent<INoPopUps, bool>(SaveNoPopUps);
        EventLocator.Unsubscribe<INoResolvePopUp>(SaveNoResolvePopUps);
    }

    public void SubscribeToService()
    {
        _uiHistoryTrack = ServiceLocator.GetNewService<IHistoryTrack>(this);
    }

    private void CancelPressed(ICancelPressed args)
    {
        if(!_noResolvePopUps && !_gameIsPaused) return;
        ProcessCancelType(_activeBranch.EscapeKeySetting);
    }

    private void EventPassedCancelType(ICancelButtonActivated args)
    {
        ProcessCancelType(args.EscapeKeyType);
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
    private void ToNextPopUp() => OnBackToAPopUp?.RaiseEvent(_lastHighlighted.MyBranch);
}
