using System;
using EZ.Events;
using UnityEngine;

public interface IManagePopUpHistory
{
    void OnEnable();
    IManagePopUpHistory IsGamePaused(bool isPaused);
    IManagePopUpHistory NoPopUpAction(Action noPopUpAction);
    void DoPopUpCheckAndHandle();
    void HandlePopUps(IBranch popUpToCancel);
    void MoveToNextPopUp();
}

public class ManagePopUpHistory : IEZEventUser, IManagePopUpHistory
{
    public ManagePopUpHistory(IHistoryTrack historyTracker) => _historyTracker = historyTracker;

    //Variables
    private readonly IHistoryTrack _historyTracker;
    private readonly IPopUpController _popUpController = EZInject.Class.NoParams<IPopUpController>();
    private bool _noPopUps = true;
    private bool _isPaused;
    private Action _noPopUpAction;
    private UIBranch _popUpToRemove;
    private bool _onHomeScreen = true;

    //Properties
    private void ActivePopUps(INoPopUps args) => _noPopUps = args.NoActivePopUps;
    private void SaveOnHomeScreen(IOnHomeScreen args) => _onHomeScreen = args.OnHomeScreen;

    //Main
    public void OnEnable()
    {
        ObserveEvents();
        _popUpController.OnEnable();
    }

    public void ObserveEvents()
    {
        PopUpEvents.Do.Subscribe<INoPopUps>(ActivePopUps);
        HistoryEvents.Do.Subscribe<IOnHomeScreen>(SaveOnHomeScreen);
    }

    public IManagePopUpHistory IsGamePaused(bool isPaused)
    {
        _isPaused = isPaused;
        return this;
    }

    public IManagePopUpHistory NoPopUpAction(Action noPopUpAction)
    {
        _noPopUpAction = noPopUpAction;
        return this;
    }

    public void DoPopUpCheckAndHandle()
    {
        if (!_noPopUps && !_isPaused)
        {
            HandlePopUps(_popUpController.NextPopUp());
        }
        else
        {
            _noPopUpAction?.Invoke();
        }
    }

    public void HandlePopUps(IBranch popUpToCancel)
    {
        _popUpController.RemoveNextPopUp(popUpToCancel);
        if(_onHomeScreen)
            popUpToCancel.StartBranchExitProcess(OutTweenType.Cancel, RemovedPopUpCallback);
    }

    private void RemovedPopUpCallback()
    {
        if (_noPopUps)
        {
            _historyTracker.MoveToLastBranchInHistory();
        }
        else
        {
            MoveToNextPopUp();
        }
    }

    public void MoveToNextPopUp()
    {
        _popUpController.NextPopUp().MoveToThisBranch();
    }
}