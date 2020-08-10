using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Need To Make this a singleton or check thee is only one of these
/// </summary>

public class PauseMenu : IPauseMenu
{
    public PauseMenu(UIBranch branch, UIBranch[] branchList)
    {
        _myBranch = branch;
        _allBranches = branchList;
        _uiData = new UIData();
        OnEnable();
    }

    private readonly UIBranch _myBranch;
    private readonly UIBranch[] _allBranches;
    private bool _noActiveResolvePopUps = true;
    private readonly UIData _uiData;
    private bool _inMenu;

    private ScreenData ClearedScreenData { get; } = new ScreenData();
    private void SetResolveCount(bool activeResolvePopUps) => _noActiveResolvePopUps = activeResolvePopUps;
    private UINode LastHighlighted { get; set; }
    private UINode LastSelected { get; set; }
    private void SaveHighlighted(UINode newNode) => LastHighlighted = newNode;
    private void SaveSelected(UINode newNode) => LastSelected = newNode;
    private void SaveInMenu(bool isInMenu) => _inMenu = isInMenu;
    
    public static event Action<bool> GamePaused; // Subscribe to trigger pause operations
    public static event Action<(bool gamepaused, GameObject sender)> NewGamePaused;

    public void OnEnable()
    {
        _uiData.SubscribeNoResolvePopUps(SetResolveCount);
        _uiData.SubscribeToHighlightedNode(SaveHighlighted);
        _uiData.SubscribeToSelectedNode(SaveSelected);
        _uiData.SubscribeToInMenu(SaveInMenu);
    }

    public void OnDisable()
    {
       _uiData.OnDisable();
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

        GamePaused?.Invoke(isGamePaused);
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

