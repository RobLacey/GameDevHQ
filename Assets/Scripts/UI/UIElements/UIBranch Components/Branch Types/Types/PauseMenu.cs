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
    private bool WasInGame() => !_screenData._wasInTheMenu;
    public bool GameIsPaused => _gameIsPaused;
    
    //Events
    private static CustomEvent<IGameIsPaused> OnGamePaused { get; } = new CustomEvent<IGameIsPaused>();
    
    public override void ObserveEvents()
    {
        base.ObserveEvents();
        EventLocator.Subscribe<IPausePressed>(StartPopUp, this);
        EventLocator.Subscribe<IActiveBranch>(SaveActiveBranch, this);
    }
    
    public override void RemoveFromEvents()
    {
        base.RemoveFromEvents();
        EventLocator.Unsubscribe<IPausePressed>(StartPopUp);
        EventLocator.Unsubscribe<IActiveBranch>(SaveActiveBranch);
    }

    //Main
    public override void OnDisable()
    {
        base.OnDisable();
        RemoveFromEvents();
    }

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
        OnGamePaused?.RaiseEvent(this);
        EnterPause();
    }

    private void UnPauseGame()
    {
        _gameIsPaused = false;
        OnGamePaused?.RaiseEvent(this);
        ExitPause();
    }

    private void EnterPause()
    {
        _screenData.StoreClearScreenData(_allBranches, _myBranch.ThisBranch, BlockRaycast.Yes);
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

