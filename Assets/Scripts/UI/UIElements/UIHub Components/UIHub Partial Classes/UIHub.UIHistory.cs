using JetBrains.Annotations;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// This Partial Class looks after the stored UI history and th current state of the UI
/// </summary>

public partial class UIHub
{
    //Properties
    [ShowNativeProperty] private UINode LastSelected { get; set; }
    [ShowNativeProperty] private UINode LastHighlighted { get; set; }
    [ShowNativeProperty] private UIBranch ActiveBranch { get; set; }
    [ShowNativeProperty] private bool GameIsPaused { get; set; }
    private bool InMenu { get; set; } = true;
    private bool CanStart { get; set; }

    private void SetLastSelected(UINode newNode)
    {
        if (_lastHomeScreenNode is null) _lastHomeScreenNode = newNode;
        if (LastSelected == newNode) return;
        
        if (_onHomeScreen)
        {
            WhenOnHomeScreen(newNode);
        }
        else
        {
            WhenNotOnHomeScreen();
        }

        LastSelected = newNode;
    }

    private void WhenNotOnHomeScreen()
    {
        if (!LastSelected.HasChildBranch) return; //Stops Tween Error when no child
        if (LastSelected.HasChildBranch.MyBranchType == BranchType.Internal) LastSelected.Deactivate();
    }

    private void WhenOnHomeScreen(UINode newNode)
    {
        if (newNode.MyBranch.IsAPopUpBranch() || newNode.MyBranch.IsPauseMenuBranch()) return;

        while (newNode.MyBranch != newNode.MyBranch.MyParentBranch)
        {
            newNode = newNode.MyBranch.MyParentBranch.LastSelected;
        }

        if (newNode != _lastHomeScreenNode && _lastHomeScreenNode.IsSelected) 
            _lastHomeScreenNode.Deactivate();

        _lastHomeScreenNode = newNode;
    }

    private void SetLastHighlighted(UINode newNode)
    {
        if (newNode == LastHighlighted) return;
        LastHighlighted.SetNotHighlighted();
        LastHighlighted = newNode;
        if(!GameIsPaused)_popUpController.SetLastNodeBeforePopUp(newNode);
        EventSystem.current.SetSelectedGameObject(LastHighlighted.gameObject);
    }
}