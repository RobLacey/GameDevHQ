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
    }

    //Variables
    private readonly UIBranch[] _allBranches;
    private UIBranch _activeBranch;
    
    //Properties
    private void SaveActiveBranch(UIBranch newBranch) => _activeBranch = newBranch;
    private bool WasInGame() => !_screenData._wasInTheMenu;
    
    //Events
    private static CustomEvent<IGameIsPaused, bool> OnGamePaused { get; } = new CustomEvent<IGameIsPaused, bool>();
    
    public override void ObserveEvents()
    {
        base.ObserveEvents();
        EventLocator.SubscribeToEvent<IPausePressed>(StartPopUp, this);
        EventLocator.SubscribeToEvent<IActiveBranch, UIBranch>(SaveActiveBranch, this);
    }
    
    public override void RemoveFromEvents()
    {
        base.RemoveFromEvents();
        EventLocator.UnsubscribeFromEvent<IPausePressed>(StartPopUp);
        EventLocator.UnsubscribeFromEvent<IActiveBranch, UIBranch>(SaveActiveBranch);
    }

    //Main
    public override void OnDisable()
    {
        base.OnDisable();
        RemoveFromEvents();
    }

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
        OnGamePaused?.RaiseEvent(_gameIsPaused);
        EnterPause();
    }

    private void UnPauseGame()
    {
        _gameIsPaused = false;
        OnGamePaused?.RaiseEvent(_gameIsPaused);
        ExitPause();
    }

    private void EnterPause()
    {
        _screenData.StoreClearScreenData(_allBranches, _myBranch, BlockRayCast.Yes);
        _myBranch.MoveToThisBranch();
    }

    public override void SetUpBranch(UIBranch newParentController = null)
    {
        ActivateBranchCanvas();
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

    private void ExitPause() => _myBranch.StartBranchExitProcess(OutTweenType.Cancel, RestoreLastStoredState);

    private void RestoreLastStoredState()
    {
        if (WasInGame()) return;
        ActivateStoredPosition();
    }
    
    public override void MoveBackToThisBranch(UIBranch lastBranch)
    {
        if (lastBranch != _myBranch) return;
        
        base.MoveBackToThisBranch(lastBranch);
        if (_myBranch.CanvasIsEnabled)
            _myBranch.SetNoTween();

        _myBranch.MoveToThisBranch();
    }
}

