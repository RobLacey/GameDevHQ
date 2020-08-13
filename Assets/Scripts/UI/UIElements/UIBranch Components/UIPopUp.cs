using System;
using System.Collections.Generic;
using UnityEngine;

//Todo Stop Optional popups appearing when in fullscreen. Maybe new popups altogether and maybe cache them 
public class UIPopUp : IPopUp
{
    private readonly UIBranch _myBranch;
    private readonly UIBranch[] _allBranches;
    private bool _noActivePopUps = true;
    private bool _isInMenu;
    private readonly UIDataEvents _uiDataEvents;
    private readonly UIControlsEvents _uiControlsEvents;
    private UIPopUpEvents _uiPopUpEvents;
    private readonly List<UIBranch> _clearedBranches = new List<UIBranch>();
    private bool _gameIsPaused;
    private bool _inGameBeforePopUp;

    public UIPopUp(UIBranch branch, UIBranch[] branchList)
    {
        _uiDataEvents = new UIDataEvents();
        _uiControlsEvents = new UIControlsEvents();
        _uiPopUpEvents = new UIPopUpEvents();
        _myBranch = branch;
        _allBranches = branchList;
        OnEnable();
    }

    //Properties
    private void SaveNoActivePopUps(bool noActivePopUps) => _noActivePopUps = noActivePopUps;
    private void SaveIfGamePaused(bool paused) => _gameIsPaused = paused;
    private void SaveIfInMenu(bool inMenu) => _isInMenu = inMenu;
    
    //Delegates
    public static event Action<UIBranch> AddResolvePopUp;
    public static event Action<UIBranch> AddOptionalPopUp;


    private void OnEnable()
    {
        _uiControlsEvents.SubscribeToGameIsPaused(SaveIfGamePaused);
        _uiDataEvents.SubscribeToInMenu(SaveIfInMenu);
        _uiPopUpEvents.SubscribeNoPopUps(SaveNoActivePopUps);
    }

    public void StartPopUp()
    {
        if (_gameIsPaused) return;
        
        if (!_myBranch.CanvasIsEnabled)
            SetUpPopUp();
    }
    
    protected virtual void SetUpPopUp()
    {
        if (!_isInMenu && _noActivePopUps)
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
        
        if (_noActivePopUps && _inGameBeforePopUp)
        {
            ReturnToGame(lastBranch);
        }
        else
        {
            ToLastActiveBranch(lastBranch);
        }
    }

    private void ReturnToGame(UIBranch lastBranch)
    {
        lastBranch.LastHighlighted.ThisNodeIsHighLighted();
        _inGameBeforePopUp = false;
    }

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

