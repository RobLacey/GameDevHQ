using System.Collections.Generic;

/// <summary>
/// Need To Make this a singleton or check thee is only one of these
/// </summary>

public class PauseMenu
{
    public PauseMenu(UIBranch branch, UIBranch[] branchList)
    {
        _myBranch = branch;
        _allBranches = branchList;
        OnEnable();
    }

    //Variables
    private readonly UIBranch _myBranch;
    private readonly UIBranch[] _allBranches;
    private readonly UIDataEvents _uiDataEvents = new UIDataEvents();
    private readonly UIControlsEvents _uiControlsEvents = new UIControlsEvents();
    private readonly ScreenData _clearedScreenData = new ScreenData();
    private bool _inMenu;
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
    private void SaveInMenu(bool isInMenu) => _inMenu = isInMenu;

    private void OnEnable()
    {
        _uiDataEvents.SubscribeToHighlightedNode(SaveHighlighted);
        _uiDataEvents.SubscribeToSelectedNode(SaveSelected);
        _uiDataEvents.SubscribeToInMenu(SaveInMenu);
        _uiControlsEvents.SubscribeToGameIsPaused(StartPauseMenu);
    }

    private void StartPauseMenu(bool isGamePaused)
    {
        if (isGamePaused)
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
            if(branchToClear.ClearActiveBranches(_myBranch, _myBranch.ScreenType))
                _clearedScreenData._clearedBranches.Add(branchToClear);
        }
        ActivatePauseMenu();
    }

    private void ActivatePauseMenu()
    {
        _myBranch.MoveToThisBranch();
    }

    private void StoreClearScreenData()
    {
        _clearedScreenData._wasInTheMenu = _inMenu;
        _clearedScreenData._clearedBranches.Clear();
        _clearedScreenData._lastSelected = _lastSelected;
        _clearedScreenData._lastHighlighted = _lastHighlighted;
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

