using System;

/// <summary>
/// Need To Make this a singleton or check thee is only one of these
/// </summary>

public interface IPauseBranch : IBranchBase { }

public class PauseMenu : BranchBase, IGameIsPaused, IPauseBranch
{
    public PauseMenu(IBranch branch) : base(branch) { } 

    //Variables
    private IBranch ActiveBranch => _myDataHub.ActiveBranch;

    //Properties
    private bool WasInGame() => !_screenData.WasOnHomeScreen;
    public bool IsPaused { get; private set; }
    private IBranch[] AllBranches => _myDataHub.AllBranches;

    //Events
    private Action<IGameIsPaused> OnGamePaused { get; set; }
    
    public override void FetchEvents()
    {
        base.FetchEvents();
        OnGamePaused = HistoryEvents.Do.Fetch<IGameIsPaused>();
    }

    public override void ObserveEvents()
    {
        base.ObserveEvents();
        InputEvents.Do.Subscribe<IPausePressed>(StartPopUp);
    }

    //Main
    private void StartPopUp(IPausePressed args)
    {
        if(!CanStart) return;
        
        if (!GameIsPaused)
        {
            PauseGame();
            return;
        }
        
        if(GameIsPaused && ActiveBranch.IsPauseMenuBranch())
            UnPauseGame();
    }

    private void PauseGame()
    {
        IsPaused = true;
        OnGamePaused?.Invoke(this);
        EnterPause();
    }

    private void EnterPause()
    {
        _screenData.StoreClearScreenData(AllBranches, _myBranch, BlockRaycast.Yes);
        _myBranch.MoveToThisBranch();
    }

    private void UnPauseGame()
    {
        IsPaused = false;
        OnGamePaused?.Invoke(this);
        ExitPause();
    }

    private void ExitPause() => _myBranch.StartBranchExitProcess(OutTweenType.Cancel);

    public override void SetUpBranch(IBranch newParentController = null)
    {
        base.SetUpBranch(newParentController);
        SetCanvas(ActiveCanvas.Yes);
        CanGoToFullscreen_Paused();
    }

    public override void EndOfBranchExit()
    {
        base.EndOfBranchExit();
       RestoreLastStoredState();
    }

    private void RestoreLastStoredState()
    {
        if (WasInGame()) return;
        ActivateStoredPosition();
        _historyTrack.MoveToLastBranchInHistory();
    }
}

