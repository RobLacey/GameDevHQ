
using System;
using UnityEngine;

public class UIPopUp : IPopUp
{
    //protected IGameToMenuSwitching _gameToMenuSwitching;
    private UIBranch _myBranch;
    private UIBranch[] _allBranches;
    private bool _noActiveResolvePopUps = true;
    private bool _noActiveNonResolvePopUps = true;
    private bool _isInMenu;
    private readonly UIData _uiData;

    public UIPopUp(UIBranch branch, UIBranch[] branchList)
    {
        _uiData = new UIData();
        _myBranch = branch;
        _allBranches = branchList;
        OnEnable();
    }

    //Properties
    private ScreenData ScreenData { get; } = new ScreenData();
    private bool InGameBeforePopUp { get; set; }
    private bool GameIsPaused { get; set; }
    private bool NoActivePopUps => _noActiveResolvePopUps && _noActiveNonResolvePopUps;
    private void IsGamePaused(bool paused) => GameIsPaused = paused;
    private void SetResolvePopUpCount(bool activeResolvePopUps) => _noActiveResolvePopUps = activeResolvePopUps;
    private void SetOptionalPopUpCount(bool activeNonResolvePopUps) => _noActiveNonResolvePopUps = activeNonResolvePopUps;
    private void SaveInMenu(bool inMenu) => _isInMenu = inMenu;
    private UINode LastHighlighted { get; set; }
    private UINode LastSelected { get; set; }
    private void SaveHighlighted(UINode newNode) => LastHighlighted = newNode;
    private void SaveSelected(UINode newNode) => LastSelected = newNode;
    
    public static event Action<UIBranch> AddResolvePopUp;
    public static event Action<UIBranch> AddOptionalPopUp;


    public void OnEnable()
    {
        _uiData.SubscribeToGameIsPaused(IsGamePaused);
        _uiData.SubscribeToInMenu(SaveInMenu);
        _uiData.SubscribeToHighlightedNode(SaveHighlighted);
        _uiData.SubscribeToSelectedNode(SaveSelected);
        _uiData.SubscribeNoResolvePopUps(SetResolvePopUpCount);
        _uiData.SubscribeNoOptionalPopUps(SetOptionalPopUpCount);
    }

    public void OnDisable()
    {
        _uiData.OnDisable();
    }
    
    public void StartPopUp()
    {
        if (GameIsPaused) return;
        if (!_myBranch.CanvasIsEnabled)
        {
            SetUpPopUp();
        }
    }
    
    protected virtual void SetUpPopUp()
    {
        if (!_isInMenu && NoActivePopUps)
        {
            InGameBeforePopUp = true;
        }

        if (_myBranch.IsResolvePopUp)
        {
            AddResolvePopUp?.Invoke(_myBranch);
        }

        if (_myBranch.IsOptionalPopUp)
        {
            AddOptionalPopUp?.Invoke(_myBranch);
        }
        
        PopUpStartProcess();
    }
    
    protected void PopUpStartProcess()
    {
        StoreScreenData();

        if (_myBranch.IsResolvePopUp || _myBranch.IsPauseMenuBranch())
        {
            foreach (var branch in _allBranches)
            {
                if (branch == _myBranch) continue;
                if (!branch.CheckIfActiveAndDisableBranch(_myBranch.ScreenType)) continue;
                ScreenData._clearedBranches.Add(branch);
            }
        }

        ActivatePopUp();
    }
    
    private void StoreScreenData()
    {
        ScreenData._clearedBranches.Clear();
        ScreenData._lastSelected = LastSelected;
        ScreenData._lastHighlighted = LastHighlighted;
    }
    
    private void ActivatePopUp()
    {
        LastSelected.Audio.Play(UIEventTypes.Selected);
        _myBranch.MoveToThisBranch();
    }
    
    public void MoveToNextPopUp(UINode lastNode)
    {
        if (_myBranch.WhenToMove == WhenToMove.AfterEndOfTween)
        {
            _myBranch.StartOutTween(() => EndOfTweenActions(lastNode));
        }
        else
        {
            _myBranch.StartOutTween();
            EndOfTweenActions(lastNode);
        }
    }

    private void EndOfTweenActions(UINode lastNode)
    {
        DoRestoreScreen();
        SetLastSelected(lastNode);
        if (NoActivePopUps && InGameBeforePopUp)
        {
            ReturnToGame();
        }
        else
        {
            ToLastActiveNode(lastNode);
        }
    }

    private void ReturnToGame()
    {
        //_gameToMenuSwitching.SwitchBetweenGameAndMenu();
        InGameBeforePopUp = false;
    }

    private static void ToLastActiveNode(UINode lastNode)
    {
        lastNode.MyBranch.MoveToBranchWithoutTween();
    }

    private void SetLastSelected(UINode lastNode)
    {
        if (IfLastNodeHasParent(lastNode))
        {
            lastNode.MyBranch.MyParentBranch.LastSelected.ThisNodeIsSelected();
        }
        else
        {
            ScreenData._lastSelected.ThisNodeIsSelected();
        }
    }

    private static bool IfLastNodeHasParent(UINode lastHomeGroupNode)
    {
        return lastHomeGroupNode.MyBranch.MyParentBranch;
    }

    private void DoRestoreScreen()
    {
        if (GameIsPausedOrNotAResolvePopUp()) return;
        
        foreach (var branch in ScreenData._clearedBranches)
        {
            branch.ActivateBranch();
        }
    }

    private bool GameIsPausedOrNotAResolvePopUp()
    {
        return GameIsPaused || !_myBranch.IsResolvePopUp;
    }
}

