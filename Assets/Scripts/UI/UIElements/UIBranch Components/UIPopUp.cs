using System;
using System.Collections.Generic;

//Todo Stop Optional popups appearing when in fullscreen. Maybe new popups altogether and maybe cache them 
public class UIPopUp : IPopUp
{
    private readonly UIBranch _myBranch;
    private readonly UIBranch[] _allBranches;
    private bool _noActiveResolvePopUps = true;
    private bool _noActiveNonResolvePopUps = true;
    private bool _isInMenu;
    private readonly UIData _uiData;
    private readonly List<UIBranch> _clearedBranches = new List<UIBranch>();
    private bool _gameIsPaused;
    private bool _inGameBeforePopUp;

    public UIPopUp(UIBranch branch, UIBranch[] branchList)
    {
        _uiData = new UIData();
        _myBranch = branch;
        _allBranches = branchList;
        OnEnable();
    }

    //Properties
    private bool NoActivePopUps => _noActiveResolvePopUps && _noActiveNonResolvePopUps;
    private void SaveIfGamePaused(bool paused) => _gameIsPaused = paused;
    private void SetResolvePopUpCount(bool activeResolvePopUps) => _noActiveResolvePopUps = activeResolvePopUps;
    private void SetOptionalPopUpCount(bool activeNonResolvePopUps) => _noActiveNonResolvePopUps = activeNonResolvePopUps;
    private void SaveIfInMenu(bool inMenu) => _isInMenu = inMenu;
    
    //Delegates
    public static event Action<UIBranch> AddResolvePopUp;
    public static event Action<UIBranch> AddOptionalPopUp;


    public void OnEnable()
    {
        _uiData.SubscribeToGameIsPaused(SaveIfGamePaused);
        _uiData.SubscribeToInMenu(SaveIfInMenu);
        _uiData.SubscribeNoResolvePopUps(SetResolvePopUpCount);
        _uiData.SubscribeNoOptionalPopUps(SetOptionalPopUpCount);
    }

    public void OnDisable() => _uiData.OnDisable();

    public void StartPopUp()
    {
        if (_gameIsPaused) return;
        
        if (!_myBranch.CanvasIsEnabled)
            SetUpPopUp();
    }
    
    protected virtual void SetUpPopUp()
    {
        if (!_isInMenu && NoActivePopUps)
            _inGameBeforePopUp = true;

        if (_myBranch.IsResolvePopUp)
            AddResolvePopUp?.Invoke(_myBranch);

        if (_myBranch.IsOptionalPopUp)
            AddOptionalPopUp?.Invoke(_myBranch);
        
        PopUpStartProcess();
    }

    private void PopUpStartProcess()
    {
        _clearedBranches.Clear();

        if (_myBranch.IsResolvePopUp || _myBranch.IsPauseMenuBranch())
            ClearAndStoreActiveBranches();
        
        ActivatePopUp();
    }

    private void ClearAndStoreActiveBranches()
    {
        foreach (var branch in _allBranches)
        {
            if (branch == _myBranch) continue;
            if (!branch.CheckIfActiveAndDisableBranch(_myBranch.ScreenType)) continue;
            _clearedBranches.Add(branch);
        }
    }

    private void ActivatePopUp()
    {
        _myBranch.LastSelected.Audio.Play(UIEventTypes.Selected);
        _myBranch.MoveToThisBranch();
    }
    
    public void MoveToNextPopUp(UIBranch lastBranch)
    {
        if (_myBranch.WhenToMove == WhenToMove.AfterEndOfTween)
        {
            _myBranch.StartOutTween(() => EndOfTweenActions(lastBranch));
        }
        else
        {
            _myBranch.StartOutTween();
            EndOfTweenActions(lastBranch);
        }
    }

    private void EndOfTweenActions(UIBranch lastBranch)
    {
        DoRestoreScreen();
        
        if (NoActivePopUps && _inGameBeforePopUp)
        {
            ReturnToGame();
        }
        else
        {
            ToLastActiveBranch(lastBranch);
        }
    }

    private void ReturnToGame() => _inGameBeforePopUp = false;

    private static void ToLastActiveBranch(UIBranch lastActiveBranch) 
        => lastActiveBranch.MoveToBranchWithoutTween();

    private void DoRestoreScreen()
    {
        foreach (var branch in _clearedBranches)
        {
            branch.ActivateBranch();
        }
    }
}

