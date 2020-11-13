using System;

public interface IManagePopUpHistory
{
    IManagePopUpHistory IsGamePaused(bool isPaused);
    IManagePopUpHistory NoPopUpAction(Action noPopUpAction);
    void DoPopUpCheckAndHandle();
    void HandlePopUps(UIBranch popUpToCancel);
    void MoveToNextPopUp();
}

public class ManagePopUpHistory : IEventUser, IManagePopUpHistory
{
    public ManagePopUpHistory(IHistoryTrack historyTracker, IPopUpController popUpController)
    {
        _historyTracker = historyTracker;
        _popUpController = popUpController;
        ObserveEvents();
    }
    
    //Variables
    private readonly IHistoryTrack _historyTracker;
    private readonly IPopUpController _popUpController;
    private bool _noPopUps, _isPaused;
    private Action _noPopUpAction;
    private UIBranch _popUpToRemove;

    //Properties
    private void ActivePopUps(INoPopUps args) => _noPopUps = args.NoActivePopUps;

    //Main
    public void ObserveEvents() => EventLocator.Subscribe<INoPopUps>(ActivePopUps, this);

    public void RemoveFromEvents() => EventLocator.Unsubscribe<INoPopUps>(ActivePopUps);

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

    public void HandlePopUps(UIBranch popUpToCancel)
    {
        if(popUpToCancel.EscapeKeySetting == EscapeKey.None) return;
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
        _popUpController.NextPopUp().MoveToBranchWithoutTween();
    }
}