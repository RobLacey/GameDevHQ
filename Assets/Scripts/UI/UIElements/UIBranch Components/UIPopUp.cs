
using UnityEngine;

public class UIPopUp : IPopUp, IHUbData, INodeData
{
    protected IGameToMenuSwitching _gameToMenuSwitching;
    protected UIBranch _myBranch;
    protected UIBranch[] _allBranches;
    protected bool _noActiveResolvePopUps = true;
    private bool _noActiveNonResolvePopUps = true;
    private bool _isInMenu;

    //Properties
    protected ScreenData ScreenData { get; } = new ScreenData();
    private bool InGameBeforePopUp { get; set; }
    public bool GameIsPaused { get; private set; }
    private bool NoActivePopUps => _noActiveResolvePopUps && _noActiveNonResolvePopUps;
    public void IsGamePaused(bool paused) => GameIsPaused = paused;
    private void SetResolveCount(bool activeResolvePopUps) => _noActiveResolvePopUps = activeResolvePopUps;
    private void SetNonResolveCount(bool activeNonResolvePopUps) => _noActiveNonResolvePopUps = activeNonResolvePopUps;
    private void SetSetInMenu(bool inMenu) => _isInMenu = inMenu;
    public UINode LastHighlighted { get; private set; }
    public UINode LastSelected { get; private set; }
    public void SaveHighlighted(UINode newNode) => LastHighlighted = newNode;
    public void SaveSelected(UINode newNode) => LastSelected = newNode;

    public void OnEnable()
    {
        UIHub.GamePaused += IsGamePaused;
        UIHub.SetInMenu += SetSetInMenu;
        UINode.DoHighlighted += SaveHighlighted;
        UINode.DoSelected += SaveSelected;
        PopUpController.NoResolvePopUps += SetResolveCount;
        PopUpController.NoNonResolvePopUps += SetNonResolveCount;
    }

    public void OnDisable()
    {
        UIHub.GamePaused -= IsGamePaused;
        UIHub.SetInMenu -= SetSetInMenu;
        UINode.DoHighlighted -= SaveHighlighted;
        UINode.DoSelected -= SaveSelected;
        PopUpController.NoResolvePopUps -= SetResolveCount;
        PopUpController.NoNonResolvePopUps -= SetNonResolveCount;
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
            _gameToMenuSwitching.SwitchBetweenGameAndMenu();
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
        _gameToMenuSwitching.SwitchBetweenGameAndMenu();
        InGameBeforePopUp = false;
    }

    private void ToLastActiveNode(UINode lastNode)
    {
        lastNode.MyBranch.TweenOnChange = false;
        lastNode.MyBranch.MoveToThisBranch();
    }

    private void SetLastSelected(UINode lastNode)
    {
        if (IfLastNodeHasParent(lastNode))
        {
            lastNode.MyBranch.MyParentBranch.LastSelected.SetAsSelected();
        }
        else
        {
            ScreenData._lastSelected.SetAsSelected();
        }
    }

    private bool IfLastNodeHasParent(UINode lastHomeGroupNode)
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

