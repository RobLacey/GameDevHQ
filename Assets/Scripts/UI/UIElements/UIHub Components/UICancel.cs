using System;
using UnityEngine;

interface ICancel
{
    void OnEnable();
    void OnDisable();
}

/// <summary>
/// Class handles all UI cancel behaviour from cancel type to context sensitive cases
/// </summary>
public class UICancel : ICancel, IEServUser, IEventUser, IIsAService
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
        UseEServLocator();
        ObserveEvents();
    }

    public void OnDisable() => RemoveEvents();

    public void ObserveEvents()
    {
        EVent.Do.Subscribe<INoResolvePopUp>(SaveResolvePopUps);
        EVent.Do.Subscribe<ICancelPressed>(CancelPressed);
        EVent.Do.Subscribe<ICancelButtonActivated>(CancelOrBackButtonPressed);
        EVent.Do.Subscribe<IGameIsPaused>(SaveGameIsPaused);
        EVent.Do.Subscribe<ICancelHoverOver>(CancelHooverOver);
    }

    public void RemoveEvents()
    {
        EVent.Do.Unsubscribe<INoResolvePopUp>(SaveResolvePopUps);
        EVent.Do.Unsubscribe<ICancelPressed>(CancelPressed);
        EVent.Do.Unsubscribe<ICancelButtonActivated>(CancelOrBackButtonPressed);
        EVent.Do.Unsubscribe<IGameIsPaused>(SaveGameIsPaused);
        EVent.Do.Subscribe<ICancelHoverOver>(CancelHooverOver);
    }

    public void UseEServLocator() => _uiHistoryTrack = EServ.Locator.Get<IHistoryTrack>(this);

    private void CancelPressed(ICancelPressed args)
    {
        if(_resolvePopUps && !_gameIsPaused) return;
        ProcessCancelType(args.EscapeKeySettings);
    }

    private void CancelOrBackButtonPressed(ICancelButtonActivated args) => ProcessCancelType(args.EscapeKeyType);

    private void CancelHooverOver(ICancelHoverOver args) => ProcessCancelType(args.EscapeKeyType);

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
        }
    }

    private void StartCancelProcess(Action endOfCancelAction) 
        => _uiHistoryTrack.CheckForPopUpsWhenCancelPressed(endOfCancelAction);
    private void BackOneLevel() => _uiHistoryTrack.BackOneLevel();
    private void BackToHome() => _uiHistoryTrack.BackToHome();
}
