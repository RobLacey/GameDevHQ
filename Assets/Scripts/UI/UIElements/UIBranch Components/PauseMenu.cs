using System;
using UnityEngine;

/// <summary>
/// Need To Make this a singleton or check thee is only one of these
/// </summary>
public class PauseMenu : BranchBase, IStartPopUp
{
    public PauseMenu(UIBranch branch, UIBranch[] branchList) : base(branch)
    {
        _allBranches = branchList;
        _uiControlsEvents.SubscribePausedPressed(StartPopUp);
        _uiDataEvents.SubscribeToActiveBranch(SaveActiveBranch);
    }

    //Variables
    private readonly UIBranch[] _allBranches;
    private UIBranch _activeBranch;
    
    //Properties
    private void SaveActiveBranch(UIBranch newBranch) => _activeBranch = newBranch;
    private bool WasInGame() => !_screenData._wasInTheMenu;

    
    public static event Action<bool> OnGamePaused; // Subscribe to trigger pause operations

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
        OnGamePaused?.Invoke(_gameIsPaused);
        EnterPause();
    }

    private void UnPauseGame()
    {
        _gameIsPaused = false;
        OnGamePaused?.Invoke(_gameIsPaused);
        ExitPause();
    }

    private void EnterPause()
    {
        _screenData.StoreClearScreenData(_allBranches, _myBranch, BlockRayCast.Yes);
        _myBranch.MoveToThisBranch();
    }

    public override void SetUpBranch(UIBranch newParentController = null)
    {
        ActivateBranch();
        CanGoToFullscreen();
        _myBranch.ResetBranchesStartPosition();
    }

    protected override void CanGoToFullscreen()
    {
        if (_onHomeScreen)
        {
            InvokeOnHomeScreen(_myBranch.IsHomeScreenBranch());
        }
        InvokeDoClearScreen(_myBranch);
    }

    private void ExitPause() => _myBranch.StartOutTweenProcess(OutTweenType.Cancel, RestoreLastStoredState);

    private void RestoreLastStoredState()
    {
        if (WasInGame()) return;
        ActivateStoredPosition();
    }

    private void ActivateStoredPosition()
    {
        _screenData.RestoreScreen();
        _screenData._activeBranch.MoveToBranchWithoutTween();
        if (_screenData._wasOnHomeScreen)
            InvokeOnHomeScreen(true);
        
        _screenData._locked = false;
    }

    protected override void MoveBackToThisBranch(UIBranch lastBranch)
    {
        if (lastBranch != _myBranch) return;
        
        base.MoveBackToThisBranch(lastBranch);
        if (_myBranch.CanvasIsEnabled)
            _myBranch.SetNoTween();

        _myBranch.MoveToThisBranch();
    }
}

