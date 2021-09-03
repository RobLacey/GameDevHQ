using System;
using EZ.Service;

public interface IManagePopUpHistory
{
    void OnEnable();
    IManagePopUpHistory NoPopUpAction(Action noPopUpAction);
    void DoPopUpCheckAndHandle();
    void HandlePopUps(IBranch popUpToCancel);
    void MoveToNextPopUp();
}

public class ManagePopUpHistory : IManagePopUpHistory, IServiceUser
{
    public ManagePopUpHistory(IHistoryTrack historyTracker) => _historyTracker = historyTracker;

    //Variables
    private readonly IHistoryTrack _historyTracker;
    private readonly IPopUpController _popUpController = EZInject.Class.NoParams<IPopUpController>();
    private Action _noPopUpAction;
    private UIBranch _popUpToRemove;
    private IDataHub _myDataHub;

    //Properties
    private bool NoPopUps => _myDataHub.NoPopups;
    private bool OnHomeScreen => _myDataHub.OnHomeScreen;
    private bool IsPaused => _myDataHub.GamePaused;

    //Main
    public void OnEnable()
    {
        UseEZServiceLocator();
        _popUpController.OnEnable();
    }
    
    public void UseEZServiceLocator() => _myDataHub = EZService.Locator.Get<IDataHub>(this);


    public IManagePopUpHistory NoPopUpAction(Action noPopUpAction)
    {
        _noPopUpAction = noPopUpAction;
        return this;
    }

    public void DoPopUpCheckAndHandle()
    {
        if (!NoPopUps && !IsPaused)
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
        if(OnHomeScreen)
            popUpToCancel.StartBranchExitProcess(OutTweenType.Cancel, RemovedPopUpCallback);
    }

    private void RemovedPopUpCallback()
    {
        if (NoPopUps)
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