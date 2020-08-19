using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Need To Make this a singleton or check thee is only one of these
/// </summary>

public class PauseMenu : BranchBase
{
    public PauseMenu(UIBranch branch, UIBranch[] branchList) : base(branch)
    {
        _allBranches = branchList;
        OnEnable();
    }

    //Variables
    private readonly UIBranch[] _allBranches;
    private readonly UIControlsEvents _uiControlsEvents = new UIControlsEvents();
    private readonly ScreenData _clearedScreenData = new ScreenData();
    private UINode _lastHighlighted;
    private UINode _lastSelected;


    //Internal Class
    private class ScreenData
    {
        public readonly List<UIBranch> _clearedBranches = new List<UIBranch>();
        public UINode _lastHighlighted;
        public UINode _lastSelected;
        public bool  _wasInTheMenu;
    }

    private void SaveHighlighted(UINode newNode) => _lastHighlighted = newNode;
    private void SaveSelected(UINode newNode) => _lastSelected = newNode;


    private void OnEnable()
    {
        _uiDataEvents.SubscribeToHighlightedNode(SaveHighlighted);
        _uiDataEvents.SubscribeToSelectedNode(SaveSelected);
        _uiControlsEvents.SubscribeToGameIsPaused(StartPauseMenu);
    }

    private void StartPauseMenu(bool isGamePaused)
    {
        if (isGamePaused && _canStart)
        {
            PauseStartProcess();
        }
        else
        {
            RestoreLastPosition();
        }
    }
    
    private void PauseStartProcess()
    {
        StoreClearScreenData();
        
        foreach (var branchToClear in _allBranches)
        {
            if (branchToClear.CanvasIsEnabled && branchToClear != _myBranch)
            {
                _clearedScreenData._clearedBranches.Add(branchToClear);
            }
        }
        _myBranch.MoveToThisBranch();
    }

    private void StoreClearScreenData()
    {
        _clearedScreenData._wasInTheMenu = _inMenu;
        _clearedScreenData._clearedBranches.Clear();
        _clearedScreenData._lastSelected = _lastSelected;
        _clearedScreenData._lastHighlighted = _lastHighlighted;
    }

    public override void BasicSetUp(UIBranch newParentController = null)
    {
        ActivateBranch();
        CanClearOrRestoreScreen();

        if (_myBranch._saveExitSelection == IsActive.No)
        {
            _myBranch.ResetBranchStartPosition();
        }
    }


    private void RestoreLastPosition()
    {
        if (_myBranch.WhenToMove == WhenToMove.AfterEndOfTween)
        {
            _myBranch.StartOutTween(EndOfTweenActions);
        }
        else
        {
            _myBranch.StartOutTween();
            EndOfTweenActions();
        }
    }
    
    private void EndOfTweenActions()
    {
        ActiveClearedBranches();

        if (WasInGame()) return;
        _clearedScreenData._lastSelected.ThisNodeIsSelected();
        _clearedScreenData._lastHighlighted.MyBranch.MoveToBranchWithoutTween();
    }

    private void ActiveClearedBranches()
    {
        foreach (var branch in _clearedScreenData._clearedBranches)
        {
            branch.ActivateBranch();
        }
    }

    private bool WasInGame() => !_clearedScreenData._wasInTheMenu;

}

