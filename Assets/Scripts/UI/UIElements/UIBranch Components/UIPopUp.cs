using System;
using System.Collections.Generic;
using UnityEngine;

//Todo Stop Optional popups appearing when in fullscreen. Maybe new popups altogether and maybe cache them 
public class UIPopUp : BranchBase
{
    public UIPopUp(UIBranch branch, UIBranch[] branchList) : base(branch)
    {
        //_myBranch = branch;
        _allBranches = branchList;
        _onStartPopUp = StartPopUp;
        //ActivateNextPopUp = EndOfTweenActions;
        OnEnable();
    }

    //Variables
    private readonly UIBranch[] _allBranches;
    private bool _noActivePopUps = true;
    private readonly UIControlsEvents _uiControlsEvents = new UIControlsEvents();
    private readonly UIPopUpEvents _uiPopUpEvents = new UIPopUpEvents();
    private readonly List<UIBranch> _clearedBranches = new List<UIBranch>();
    private bool _gameIsPaused;
    private bool _inGameBeforePopUp;

    //Properties
    private void SaveNoActivePopUps(bool noActivePopUps) => _noActivePopUps = noActivePopUps;
    private void SaveIfGamePaused(bool paused) => _gameIsPaused = paused;

    private void OnEnable()
    {
        _uiControlsEvents.SubscribeToGameIsPaused(SaveIfGamePaused);
        _uiPopUpEvents.SubscribeNoPopUps(SaveNoActivePopUps);
    }

    private void StartPopUp()
    {
        if (_gameIsPaused) return; //TODO add to buffer goes here for when paused. trigger from SaveOnHome?

        if (!_myBranch.CanvasIsEnabled)
        {
            //_myBranch.LastSelected.Audio.Play(UIEventTypes.Selected);
            //_myBranch.MoveToBranchFromPopUp();
        }

    }

    public override void SetUpBranch(UIBranch newParentController = null)
    {
        //ActivateBranch();
        
        if (!_inMenu && _noActivePopUps)
            _inGameBeforePopUp = true;
    }

    protected override void MoveBackToThisBranch(UIBranch lastBranch)
    {
        throw new NotImplementedException();
    }

    private void PopUpStartProcess()
    {
        _clearedBranches.Clear();
        
        // if (_myBranch.IsResolvePopUp || _myBranch.IsPauseMenuBranch())
        //     ClearAndStoreActiveBranches();
    }

    private void ClearAndStoreActiveBranches()
    {
        foreach (var branchToClear in _allBranches)
        {
            if (branchToClear.CanvasIsEnabled && branchToClear != _myBranch)
                _clearedBranches.Add(branchToClear);
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
            //branch.ActivateBranch();
        }
    }
}

