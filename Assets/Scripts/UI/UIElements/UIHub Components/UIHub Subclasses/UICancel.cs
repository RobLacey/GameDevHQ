using System;
using EZ.Events;
using EZ.Service;
using UIElements;
using UnityEngine;

interface ICancel
{
    void OnEnable();
}

/// <summary>
/// Class handles all UI cancel behaviour from cancel type to context sensitive cases
/// </summary>
public class UICancel : ICancel, IServiceUser, IEZEventUser, IMonoEnable
{
    //Variables
    private bool _gameIsPaused, _resolvePopUps;
    private IHistoryTrack _uiHistoryTrack;
    private InputScheme _inputScheme;


    //Properties 7 Getters / Setters
    public EscapeKey GlobalEscapeSetting => _inputScheme.GlobalCancelAction;
    private void SaveResolvePopUps(INoResolvePopUp args) => _resolvePopUps = args.ActiveResolvePopUps;
    private void SaveGameIsPaused(IGameIsPaused args) => _gameIsPaused = args.GameIsPaused;

    public void OnEnable()
    {
        UseEZServiceLocator();
        ObserveEvents();
    }

    public void ObserveEvents()
    {
        PopUpEvents.Do.Subscribe<INoResolvePopUp>(SaveResolvePopUps);
        InputEvents.Do.Subscribe<ICancelPressed>(CancelPressed);
        CancelEvents.Do.Subscribe<ICancelButtonActivated>(CancelOrBackButtonPressed);
        HistoryEvents.Do.Subscribe<IGameIsPaused>(SaveGameIsPaused);
        CancelEvents.Do.Subscribe<ICancelHoverOver>(CancelHooverOver);
    }

    public void UseEZServiceLocator()
    {
        _inputScheme = EZService.Locator.Get<InputScheme>(this);
        _uiHistoryTrack = EZService.Locator.Get<IHistoryTrack>(this);
    }

    private void CancelPressed(ICancelPressed args)
    {
        if(_resolvePopUps && !_gameIsPaused) return;
        ProcessCancelType(args.EscapeKeySettings);
    }

    private void CancelOrBackButtonPressed(ICancelButtonActivated args) => ProcessCancelType(args.EscapeKeyType);

    private void CancelHooverOver(ICancelHoverOver args) => ProcessCancelType(args.EscapeKeyType);

    private void ProcessCancelType(EscapeKey escapeKey)
    {
        if (escapeKey == EscapeKey.GlobalSetting) escapeKey = GlobalEscapeSetting;
        
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
