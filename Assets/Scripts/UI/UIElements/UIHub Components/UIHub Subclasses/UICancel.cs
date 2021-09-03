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
    private IHistoryTrack _uiHistoryTrack;
    private InputScheme _inputScheme;
    private IDataHub _myDataHub;


    //Properties 7 Getters / Setters
    private bool GameIsPaused => _myDataHub.GamePaused;
    private bool NoResolvePopUps => _myDataHub.NoResolvePopUp;
    private EscapeKey GlobalEscapeSetting => _inputScheme.GlobalCancelAction;

    public void OnEnable()
    {
        UseEZServiceLocator();
        ObserveEvents();
    }

    public void ObserveEvents()
    {
        InputEvents.Do.Subscribe<ICancelPressed>(CancelPressed);
        CancelEvents.Do.Subscribe<ICancelButtonActivated>(CancelOrBackButtonPressed);
        CancelEvents.Do.Subscribe<ICancelHoverOver>(CancelHooverOver);
    }

    public void UnObserveEvents() { }

    public void UseEZServiceLocator()
    {
        _inputScheme = EZService.Locator.Get<InputScheme>(this);
        _uiHistoryTrack = EZService.Locator.Get<IHistoryTrack>(this);
        _myDataHub = EZService.Locator.Get<IDataHub>(this);
    }

    private void CancelPressed(ICancelPressed args)
    {
        if(!NoResolvePopUps && !GameIsPaused) return;
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
