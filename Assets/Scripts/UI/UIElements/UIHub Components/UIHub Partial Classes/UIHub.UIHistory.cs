using NaughtyAttributes;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// This Partial Class looks after the stored UI history and th current state of the UI
/// </summary>

public partial class UIHub
{
    //Properties
    private void SaveOnHomeScreen(bool onHomeScreen) => _onHomeScreen = onHomeScreen;
    private void SaveInMenu(bool isInMenu) => _inMenu = isInMenu;
    private void SaveNoActivePopUps(bool noActivePopUps) => _noActivePopUps = noActivePopUps;
    private bool CanStart { get; set; }
    private void SetLastSelected(UINode newNode)
    {
        if (_lastHomeScreenNode is null) _lastHomeScreenNode = newNode;
        if (_lastSelected == newNode) return;
        
        if (_onHomeScreen)
        {
            DeactivateLastHomeScreenNodes(newNode);
        }
        else
        {
            DeactiavteInternalBranches();
        }

        _lastSelected = newNode;
    }

    private void DeactiavteInternalBranches()
    {
        if (!_lastSelected.HasChildBranch) return; //Stops Tween Error when no child
        if (_lastSelected.HasChildBranch.MyBranchType == BranchType.Internal) _lastSelected.Deactivate();
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
        if (newNode == _lastHighlighted) return;
        _lastHighlighted.SetNotHighlighted();
        _lastHighlighted = newNode;
        if(_inMenu) SetEventSystem(_lastHighlighted.gameObject);
    }

    public static void SetEventSystem(GameObject newGameObject)
    {
        EventSystem.current.SetSelectedGameObject(newGameObject);
    }
}