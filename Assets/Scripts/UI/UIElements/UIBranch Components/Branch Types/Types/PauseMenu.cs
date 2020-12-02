using System;

/// <summary>
/// Need To Make this a singleton or check thee is only one of these
/// </summary>

public interface IPauseBranch : IBranchBase { }

public class PauseMenu : BranchBase, IStartPopUp, IGameIsPaused, IPauseBranch
{
    public PauseMenu(IBranch branch) : base(branch)
    {
        _allBranches = branch.FindAllBranches();
    }

    //Variables
    private readonly IBranch[] _allBranches;
    private IBranch _activeBranch;
    
    //Properties
    private void SaveActiveBranch(IActiveBranch args) => _activeBranch = args.ActiveBranch;
    private bool WasInGame() => !_screenData.WasOnHomeScreen;
    public bool GameIsPaused => _gameIsPaused;
    
    //Events
    private Action<IGameIsPaused> OnGamePaused { get; set; }


    public override void FetchEvents()
    {
        base.FetchEvents();
        OnGamePaused = EVent.Do.Fetch<IGameIsPaused>();
    }

    public override void ObserveEvents()
    {
        base.ObserveEvents();
        EVent.Do.Subscribe<IPausePressed>(StartPopUp);
        EVent.Do.Subscribe<IActiveBranch>(SaveActiveBranch);
    }

    public override void RemoveEvents()
    {
        base.RemoveEvents();
        EVent.Do.Unsubscribe<IPausePressed>(StartPopUp);
        EVent.Do.Unsubscribe<IActiveBranch>(SaveActiveBranch);
    }

    //Main

    private void StartPopUp(IPausePressed e) => StartPopUp();

    public void StartPopUp()
    {
        if(!_canStart) return;
        
        if (!_gameIsPaused)
        {
            PauseGame();
            return;
        }
        
        if(_gameIsPaused && _activeBranch.IsPauseMenuBranch())
            UnPauseGame();
    }

    private void PauseGame()
    {
        _gameIsPaused = true;
        OnGamePaused?.Invoke(this);
        EnterPause();
    }

    private void UnPauseGame()
    {
        _gameIsPaused = false;
        OnGamePaused?.Invoke(this);
        ExitPause();
    }

    private void EnterPause()
    {
        _screenData.StoreClearScreenData(_allBranches, _myBranch, BlockRaycast.Yes);
        _myBranch.MoveToThisBranch();
    }

    public override void SetUpBranch(IBranch newParentController = null)
    {
        SetCanvas(ActiveCanvas.Yes);
        CanGoToFullscreen();
        _myBranch.ResetBranchesStartPosition();
    }

    private void ExitPause() => _myBranch.StartBranchExitProcess(OutTweenType.Cancel, RestoreLastStoredState);

    private void RestoreLastStoredState()
    {
        if (WasInGame()) return;
        ActivateStoredPosition();
        _historyTrack.MoveToLastBranchInHistory();
    }
}

