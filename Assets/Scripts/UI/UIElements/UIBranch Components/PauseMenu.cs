using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Need To Make this a singleton or check thee is only one of these
/// </summary>

public class PauseMenu : Branch, IBranch
{
    public PauseMenu(UIBranch branch, UIBranch[] branchList, ScreenType screenType, CanvasGroup canvasGroup,
                     Canvas canvas)
    {
        _myBranch = branch;
        _allBranches = branchList;
        _screenType = screenType;
        _myCanvasGroup = canvasGroup;
        _myCanvas = canvas;

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
    private bool _onHomeScreen;
    private readonly ScreenType _screenType;
    private readonly bool _isHomeScreenBranch = false;
    private CanvasGroup _myCanvasGroup;
    private Canvas _myCanvas;


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
    private void SaveIfOnHomeScreen(bool onHomeScreen) => _onHomeScreen = onHomeScreen;


    private void OnEnable()
    {
        _uiDataEvents.SubscribeToHighlightedNode(SaveHighlighted);
        _uiDataEvents.SubscribeToSelectedNode(SaveSelected);
        _uiDataEvents.SubscribeToInMenu(SaveInMenu);
        _uiDataEvents.SubscribeToOnHomeScreen(SaveIfOnHomeScreen);
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
    

    public void SetUpStartUpBranch(UIBranch startBranch, IsActive inMenu)
    {
        //Nothing
    }

    public void ActivateBranch()
    {
        _myCanvasGroup.blocksRaycasts = true;
        _myCanvas.enabled = true;
    }

    public void ClearBranch(UIBranch ignoreThisBranch = null)
    {
        //Not Used to may need for Pause Nauigation
        if (ignoreThisBranch == _myBranch || !_myBranch.CanvasIsEnabled) return;
        Debug.Log("Pause clear");
        _myCanvas.enabled = false;
        _myCanvasGroup.blocksRaycasts = false;
    }

    public void CanClearOrRestoreScreen()
    {
        if (_screenType == ScreenType.FullScreen && !_onHomeScreen)
        {
            InvokeDoClearScreen(_myBranch);
        }

        InvokeHomeScreen(_isHomeScreenBranch);
    }
}

