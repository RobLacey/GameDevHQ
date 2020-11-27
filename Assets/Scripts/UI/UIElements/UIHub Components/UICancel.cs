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
    private bool _gameIsPaused, _resolvePopUps;
    private IHistoryTrack _uiHistoryTrack;

    //Properties
    private void SaveResolvePopUps(INoResolvePopUp args) => _resolvePopUps = args.ActiveResolvePopUps;
    private void SaveGameIsPaused(IGameIsPaused args) => _gameIsPaused = args.GameIsPaused;

    public void OnEnable()
    {
        SubscribeToService();
        ObserveEvents();
    }

    public void OnDisable() => RemoveFromEvents();

    public void ObserveEvents()
    {
        EVent.Do.Subscribe<INoResolvePopUp>(SaveResolvePopUps);
        EVent.Do.Subscribe<ICancelPressed>(CancelPressed);
        EVent.Do.Subscribe<ICancelButtonActivated>(CancelOrBackButtonPressed);
        EVent.Do.Subscribe<IGameIsPaused>(SaveGameIsPaused);
    }

    public void RemoveFromEvents()
    {
        EVent.Do.Unsubscribe<INoResolvePopUp>(SaveResolvePopUps);
        EVent.Do.Unsubscribe<ICancelPressed>(CancelPressed);
        EVent.Do.Unsubscribe<ICancelButtonActivated>(CancelOrBackButtonPressed);
        EVent.Do.Unsubscribe<IGameIsPaused>(SaveGameIsPaused);
    }

    public void SubscribeToService()
    {
        _uiHistoryTrack = ServiceLocator.Get<IHistoryTrack>(this);
    }

    private void CancelPressed(ICancelPressed args)
    {
        if(_resolvePopUps && !_gameIsPaused) return;
        ProcessCancelType(args.EscapeKeySettings);
    }

    private void CancelOrBackButtonPressed(ICancelButtonActivated args)
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
            case EscapeKey.None:
                break;
            case EscapeKey.GlobalSetting:
                break;
            case EscapeKey.HoverClose:
                StartCancelProcess(DoHover);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(escapeKey), escapeKey, null);
        }
    }

    private void StartCancelProcess(Action endOfCancelAction) 
        => _uiHistoryTrack.CheckForPopUpsWhenCancelPressed(endOfCancelAction);
    private void BackOneLevel() => _uiHistoryTrack.BackOneLevel();
    private void BackToHome() => _uiHistoryTrack.BackToHome();
    private void DoHover() => _uiHistoryTrack.DoCancelHoverToActivate();
}
