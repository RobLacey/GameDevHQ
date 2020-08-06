
using UnityEngine;

public class UIPopUp : IPopUp
{
    //protected IGameToMenuSwitching _gameToMenuSwitching;
    protected UIBranch _myBranch;
    protected UIBranch[] _allBranches;
    protected bool _noActiveResolvePopUps = true;
    private bool _noActiveNonResolvePopUps = true;
    private bool _isInMenu;
    private readonly UIData _uiData;
    protected UIPopUp()
    {
        _uiData = new UIData();
    }

    //Properties
    protected ScreenData ScreenData { get; } = new ScreenData();
    private bool InGameBeforePopUp { get; set; }
    protected bool GameIsPaused { get; private set; }
    private bool NoActivePopUps => _noActiveResolvePopUps && _noActiveNonResolvePopUps;
    private void IsGamePaused(bool paused) => GameIsPaused = paused;
    private void SetResolveCount(bool activeResolvePopUps) => _noActiveResolvePopUps = activeResolvePopUps;
    private void SetNonResolveCount(bool activeNonResolvePopUps) => _noActiveNonResolvePopUps = activeNonResolvePopUps;
    private void SaveSwitchBetweenGameAndMenu(bool inMenu) => _isInMenu = inMenu;
    private UINode LastHighlighted { get; set; }
    private UINode LastSelected { get; set; }
    private void SaveHighlighted(UINode newNode) => LastHighlighted = newNode;
    private void SaveSelected(UINode newNode) => LastSelected = newNode;

    protected void OnEnable()
    {
        _uiData.IsGamePaused = IsGamePaused;
        _uiData.AmImMenu = SaveSwitchBetweenGameAndMenu;
        _uiData.NewHighLightedNode = SaveHighlighted;
        _uiData.NewSelectedNode = SaveSelected;
        _uiData.NoResolvePopUps = SetResolveCount;
        _uiData.NoNonResolvePopUps = SetNonResolveCount;
    }
    
    public void StartPopUp()
    {
        if (GameIsPaused) return;
        if (!_myBranch.MyCanvas.enabled)
        {
            SetUpPopUp();
        }
    }
    
    protected virtual void SetUpPopUp()
    {
        if (!_isInMenu && NoActivePopUps)
        {
            InGameBeforePopUp = true;
            //_gameToMenuSwitching.SwitchBetweenGameAndMenu();
        }
    }
    
    protected void PopUpStartProcess()
    {
        StoreScreenData();
        
        foreach (var branch in _allBranches)
        {
            if (branch == _myBranch) continue;
            if (!branch.CheckAndDisableBranchCanvas(_myBranch.ScreenType)) continue;
            ScreenData._clearedBranches.Add(branch);
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
        _myBranch.LastSelected.Audio.Play(UIEventTypes.Selected);
        _myBranch.MoveToThisBranch();
    }
    
    public void RestoreLastPosition(UINode lastNode)
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
        RestoreScreen();
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

    protected virtual void RestoreScreen()
    {
        foreach (var branch in ScreenData._clearedBranches)
        {
            if (GameIsPaused) continue;
            if (_noActiveResolvePopUps)
            {
                branch.MyCanvasGroup.blocksRaycasts = true;
            }
        }
    }

}

