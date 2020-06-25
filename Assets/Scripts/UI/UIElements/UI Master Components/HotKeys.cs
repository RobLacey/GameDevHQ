using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

[System.Serializable]
public class HotKeys 
{
    [InputAxis] [AllowNesting] public string _hotkeyAxis;
    [ValidateInput("IsAllowedType", "Can't have PopUp as Hotkey as HotKey")] public UIBranch _UIBranch;

    public bool IsAllowedType()
    {
        if (_UIBranch.IsAPopUpBranch())
        {
            Debug.Log("Can't have PopUp as Hotkey as HotKey");
            return false;
        }
        return true;
    }
    public bool CheckHotkeys()
    {
        if (Input.GetButtonDown(_hotkeyAxis))
        {
            HotKeyProcess.HotKeyActivate(_UIBranch);
            return true;
        }
        return false;
    }
}

public static class HotKeyProcess
{
    public static UIHub _myUIHub;

    public static void HotKeyActivate(UIBranch branch)
    {
        if (branch.MyCanvas.enabled == true) return;

        foreach (UINode node in branch.MyParentBranch.ThisGroupsUINodes)
        {
            if (node.ChildBranch == branch)
            {
                if (_myUIHub.LastSelected != node && _myUIHub.LastSelected.ChildBranch != null 
                    && _myUIHub.LastSelected.IsSelected == true)
                {
                    StartTween(branch, node);
                }
                else
                {

                    TurnOff(branch, node);
                }
                ToNextBranch(branch, node);
                break;
            }
        }
    }

    private static void StartTween(UIBranch branch, UINode parentNode)
    {
        if (_myUIHub.LastSelected.ChildBranch.WhenToMove == WhenToMove.OnClick)
        {
            _myUIHub.LastSelected.ChildBranch.OutTweenToParent();
            TurnOff(branch, parentNode);
        }
        else
        {
            _myUIHub.LastSelected.ChildBranch.OutTweenToParent(() => TurnOff(branch, parentNode));
        }
    }

    private static void ToNextBranch(UIBranch branch, UINode parentNode)
    {
        UICancel.ResetHierachy();

        if (branch.MyBranchType != BranchType.HomeScreenUI)
                { branch.FromHotkey = true; } //Ensures back to home is used on cancel

        if (branch.ScreenType == ScreenType.ToFullScreen)
        {
            parentNode.IsSelected = true;
        }
        else
        {
            parentNode.SetSelected_NoEffects();
        }
        branch.DefaultStartPosition.IAudio.Play(UIEventTypes.Selected);
        branch.MyParentBranch.SaveLastHighlighted(parentNode);
        branch.MyParentBranch.SaveLastSelected(parentNode);
    }

    private static void TurnOff(UIBranch branch, UINode parentNode)
    {
        if (branch.ScreenType == ScreenType.ToFullScreen)
        {
            if (_myUIHub.OnHomeScreen)
            {
                UIHomeGroup.ClearHomeScreen(branch);
                _myUIHub.GroupIndex = UIHomeGroup.SetHomeGroupIndex(parentNode.MyBranch);
            }
        }
        else
        {
            if (!_myUIHub.OnHomeScreen)
            {
                UIHomeGroup.RestoreHomeScreen();
            }
        }
        branch.MoveToNextLevel();
    }
}
