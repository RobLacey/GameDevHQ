using System;

public interface IManagePopUpHistory
{
    IManagePopUpHistory IsGamePaused(bool isPaused);
    IManagePopUpHistory NoPopUpAction(Action noPopUpAction);
    void DoPopUpCheckAndHandle();
    void HandlePopUps(IBranch popUpToCancel);
    void MoveToNextPopUp();
}

public class ManagePopUpHistory : IEventUser, IManagePopUpHistory
{
    public ManagePopUpHistory(IHistoryTrack historyTracker)
    {
        _historyTracker = historyTracker;
        ObserveEvents();
    }
    
    //Variables
    private readonly IHistoryTrack _historyTracker;
    private readonly IPopUpController _popUpController = InjectClass.Class.NoParams<IPopUpController>();
    private bool _noPopUps = true;
    private bool _isPaused;
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

    public void HandlePopUps(IBranch popUpToCancel)
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