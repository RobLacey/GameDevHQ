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
    [ShowNativeProperty] public UINode LastSelected { get; private set; }
    [ShowNativeProperty] public UINode LastHighlighted { get; private set; }
    [ShowNativeProperty] public UIBranch ActiveBranch { get; private set; }
    [ShowNativeProperty] private bool GameIsPaused { get; set; }
    public UINode LastNodeBeforePopUp { get; private set; }
    public int HomeGroupIndex { get; set; }
    public bool OnHomeScreen { get; set; }
    public bool InMenu { get; private set; } = true;
    public bool CanStart { get; private set; }

    private void SetLastSelected(UINode newNode)
    {
        if (_lastHomeScreenNode is null) _lastHomeScreenNode = newNode;
        if (LastSelected == newNode) return;
        
        if (OnHomeScreen)
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
        if (newNode.MyBranch.IsAPopUpBranch() || newNode.MyBranch.IsPause()) return;

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
        //ActiveBranch = newNode.MyBranch;
        if (!newNode.MyBranch.IsAPopUpBranch() && !GameIsPaused) LastNodeBeforePopUp = newNode; //Move
        if (OnHomeScreen) _uiHomeGroup.SetHomeGroupIndex(LastHighlighted.MyBranch);
        EventSystem.current.SetSelectedGameObject(LastHighlighted.gameObject);
    }
}