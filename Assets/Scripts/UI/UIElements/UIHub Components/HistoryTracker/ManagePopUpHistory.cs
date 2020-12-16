using System;
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

public class ManagePopUpHistory : IEventUser, IManagePopUpHistory
{
    public ManagePopUpHistory(IHistoryTrack historyTracker) => _historyTracker = historyTracker;

    //Variables
    private readonly IHistoryTrack _historyTracker;
    private readonly IPopUpController _popUpController = EJect.Class.NoParams<IPopUpController>();
    private bool _noPopUps = true;
    private bool _isPaused;
    private Action _noPopUpAction;
    private UIBranch _popUpToRemove;

    //Properties
    private void ActivePopUps(INoPopUps args) => _noPopUps = args.NoActivePopUps;

    //Main
    public void OnEnable()
    {
        ObserveEvents();
        _popUpController.OnEnable();
    }

    public void ObserveEvents() => EVent.Do.Subscribe<INoPopUps>(ActivePopUps);

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
        if(popUpToCancel.EscapeKeyType == EscapeKey.None) return;
        _popUpController.RemoveNextPopUp(popUpToCancel);
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