using System.Collections.Generic;

/// <summary>
/// Need To Make this a singleton or check thee is only one of these
/// </summary>

public class PauseMenu : IPauseMenu
{
    public PauseMenu(UIBranch branch, UIBranch[] branchList)
    {
        _myBranch = branch;
        _allBranches = branchList;
        _uiDataEvents = new UIDataEvents();
        _uiControlsEvents = new UIControlsEvents();
        OnEnable();
    }

    private readonly UIBranch _myBranch;
    private readonly UIBranch[] _allBranches;
    private readonly UIDataEvents _uiDataEvents;
    private UIControlsEvents _uiControlsEvents;
    private bool _inMenu;

    //Internal Class
    private class ScreenData
    {
        public readonly List<UIBranch> _clearedBranches = new List<UIBranch>();
        public UINode _lastHighlighted;
        public UINode _lastSelected;
        public bool  _wasInTheMenu;
    }

    private ScreenData ClearedScreenData { get; } = new ScreenData();
    private UINode LastHighlighted { get; set; }
    private UINode LastSelected { get; set; }
    private void SaveHighlighted(UINode newNode) => LastHighlighted = newNode;
    private void SaveSelected(UINode newNode) => LastSelected = newNode;
    private void SaveInMenu(bool isInMenu) => _inMenu = isInMenu;
    
    public void OnEnable()
    {
        _uiDataEvents.SubscribeToHighlightedNode(SaveHighlighted);
        _uiDataEvents.SubscribeToSelectedNode(SaveSelected);
        _uiDataEvents.SubscribeToInMenu(SaveInMenu);
        _uiControlsEvents.SubscribeToGameIsPaused(StartPauseMenu);
    }
    
    public void StartPauseMenu(bool isGamePaused)
    {
        if (isGamePaused)
        {
            PopUpStartProcess();
        }
        else
        {
            RestoreLastPosition();
        }
    }
    
    private void PopUpStartProcess()
    {
        StoreClearScreenData();
        
        foreach (var branch in _allBranches)
        {
            if (branch == _myBranch) continue;
            if (!branch.CheckIfActiveAndDisableBranch(_myBranch.ScreenType)) continue;
            ClearedScreenData._clearedBranches.Add(branch);
        }
        ActivatePopUp();
    }
    
    private void ActivatePopUp()
    {
        LastSelected.Audio.Play(UIEventTypes.Selected);
        _myBranch.MoveToThisBranch();
    }

    private void StoreClearScreenData()
    {
        ClearedScreenData._wasInTheMenu = _inMenu;
        ClearedScreenData._clearedBranches.Clear();
        ClearedScreenData._lastSelected = LastSelected;
        ClearedScreenData._lastHighlighted = LastHighlighted;
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
        var nextNode = ClearedScreenData._lastHighlighted;
        
        foreach (var branch in ClearedScreenData._clearedBranches)
        {
            branch.ActivateBranch();
        }

        if (!ClearedScreenData._wasInTheMenu) return;
        ClearedScreenData._lastSelected.ThisNodeIsSelected();
        nextNode.MyBranch.MoveToBranchWithoutTween();
    }
}

