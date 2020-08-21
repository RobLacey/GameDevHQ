using System;

/// <summary>
/// Need To Make this a singleton or check thee is only one of these
/// </summary>

public class PauseMenu : BranchBase
{
    public PauseMenu(UIBranch branch, UIBranch[] branchList) : base(branch)
    {
        _allBranches = branchList;
        UIInput.OnPausedPressed += StartPauseMenu;
    }

    //Variables
    private readonly UIBranch[] _allBranches;
    
    public static event Action<bool> OnGamePaused; // Subscribe to trigger pause operations
    
    private void StartPauseMenu()
    {
        _gameIsPaused = !_gameIsPaused;
        if (_gameIsPaused && _canStart)
        {
            EnterPause();
            OnGamePaused?.Invoke(_gameIsPaused);
        }
        else
        {
            ExitPause();
        }
    }
    
    private void EnterPause()
    {
        _screenData.StoreClearScreenData(_allBranches, _myBranch, BlockRayCast.Yes);
        _myBranch.MoveToThisBranch();
    }
    
    public override void SetUpBranch(UIBranch newParentController = null)
    {
        ActivateBranch();
        CanClearScreen();

        if (_myBranch._saveExitSelection == IsActive.No)
        {
            _myBranch.ResetBranchStartPosition();
        }
    }

    protected override void MoveBackToThisBranch(UIBranch lastBranch)
    {
        if (lastBranch != _myBranch) return;
        base.MoveBackToThisBranch(lastBranch);
        if (_myBranch._stayOn == IsActive.Yes && _myBranch.CanvasIsEnabled) //TODO check works for internal
            _myBranch._tweenOnChange = false;

        _myBranch.MoveToThisBranch();

    }

    private void ExitPause() => _myBranch.StartOutTweenProcess(RestoreLastStoredState);

    private void RestoreLastStoredState() //TODO Look at how standrad branches do it
    {
        OnGamePaused?.Invoke(_gameIsPaused);

        _screenData.RestoreScreen();

        if (WasInGame()) return;
        _screenData._lastSelected.ThisNodeIsSelected();
        _screenData._activeBranch.MoveToBranchWithoutTween();
        _screenData._locked = false;
    }

    private bool WasInGame() => !_screenData._wasInTheMenu;
}

