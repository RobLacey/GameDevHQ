using NaughtyAttributes;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// This Partial Class looks after the stored UI history and th current state of the UI
/// </summary>

public partial class UIHub
{
    //Properties
    private void SaveActiveBranch(UIBranch newBranch) => ActiveBranch = newBranch;
    private void SaveOnHomeScreen(bool onHomeScreen) => _onHomeScreen = onHomeScreen;
    private void SaveInMenu(bool isInMenu) => _inMenu = isInMenu;
    [ShowNativeProperty] private UINode LastSelected { get; set; }
    [ShowNativeProperty] private UINode LastHighlighted { get; set; }
    [ShowNativeProperty] private UIBranch ActiveBranch { get; set; }
    [ShowNativeProperty] private bool GameIsPaused { get; set; }
    private bool CanStart { get; set; }

    private void SetLastSelected(UINode newNode)
    {
        if (_lastHomeScreenNode is null) _lastHomeScreenNode = newNode;
        if (LastSelected == newNode) return;
        
        if (_onHomeScreen)
        {
            DeactivateLastHomeScreenNodes(newNode);
        }
        else
        {
            DeactiavteInternalBranches();
        }

        LastSelected = newNode;
    }

    private void DeactiavteInternalBranches()
    {
        if (!LastSelected.HasChildBranch) return; //Stops Tween Error when no child
        if (LastSelected.HasChildBranch.MyBranchType == BranchType.Internal) LastSelected.Deactivate();
    }

    private void DeactivateLastHomeScreenNodes(UINode newNode)
    {
        if (IsAPopUpOrPauseMenu(newNode)) return;
        newNode = FindNewNodesHomeScreenParent(newNode);

        if (CanDeactivateLastHomeScreenNode(newNode)) 
            _lastHomeScreenNode.Deactivate();
        
        _lastHomeScreenNode = newNode;
    }

    private static bool IsAPopUpOrPauseMenu(UINode newNode)
        => newNode.MyBranch.IsAPopUpBranch() || newNode.MyBranch.IsPauseMenuBranch();

    private bool CanDeactivateLastHomeScreenNode(UINode newNode)
        => newNode != _lastHomeScreenNode && _lastHomeScreenNode.IsSelected;

    private static UINode FindNewNodesHomeScreenParent(UINode newNode)
    {
        while (newNode.MyBranch != newNode.MyBranch.MyParentBranch)
        {
            newNode = newNode.MyBranch.MyParentBranch.LastSelected;
        }
        return newNode;
    }

    private void SetLastHighlighted(UINode newNode)
    {
        if (newNode == LastHighlighted) return;
        LastHighlighted.SetNotHighlighted();
        LastHighlighted = newNode;
        if(!GameIsPaused)_popUpController.SetLastNodeBeforePopUp(newNode);
        if(_inMenu) SetEventSystem(LastHighlighted.gameObject);
    }

    public static void SetEventSystem(GameObject newGameObject)
    {
        EventSystem.current.SetSelectedGameObject(newGameObject);
    }
}