using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

[System.Serializable]
public class HotKeys 
{
    [InputAxis] [AllowNesting] public string _hotkeyAxis;
    public UIBranch _UIBranch;

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
    public static void HotKeyActivate(UIBranch branch)
    {
        if (branch.MyBranchType == BranchType.Independent)
        {
            if (branch.ScreenType == ScreenType.ToFullScreen)
            {
                if (branch.MyCanvas.enabled == true) return;
                UIHomeGroup.ClearHomeScreen(branch);
            }
            else
            {
                UIHomeGroup.RestoreHomeScreen();
            }

            branch.MoveToNextLevel();
        }
        else
        {

            foreach (var node in branch.MyParentBranch.ThisGroupsUINodes) //****Check this functions corretcly
                if (node._navigation._childBranch == branch)
                {
                    if (branch.UIMaster.LastSelected != node && branch.UIMaster.LastSelected._navigation._childBranch != null)
                    {
                        if (branch.UIMaster.LastSelected.IsSelected == true)
                        {
                            branch.UIMaster.LastSelected.SetNotSelected_NoEffects();

                            if (branch.UIMaster.LastSelected._navigation._childBranch.WhenToMove == WhenToMove.OnClick)
                            {
                                branch.UIMaster.LastSelected._navigation._childBranch.OutTweenToParent();
                                TurnOff(branch, node);
                            }
                            else
                            {
                                branch.UIMaster.LastSelected._navigation._childBranch.OutTweenToParent(() => TurnOff(branch, node));
                            }
                        }
                        else
                        {
                            TurnOff(branch, node);
                        }
                    }
                    else
                    {

                        TurnOff(branch, node);
                    }
                }
        }
    } 

    private static void TurnOff(UIBranch branch, UINode node)
    {
        if (branch.ScreenType == ScreenType.ToFullScreen)
        {
            if (branch.UIMaster.OnHomeScreen)
            {
                UIHomeGroup.ClearHomeScreen(branch);
            }
        }
        else
        {
            if (!branch.UIMaster.OnHomeScreen)
            {

                UIHomeGroup.RestoreHomeScreen();
            }
        }
        ToNextBranch(branch, node);
    }

    private static void ToNextBranch(UIBranch branch, UINode node)
    {
        if (branch.UIMaster.LastSelected.IsSelected == true)
        {
            branch.UIMaster.LastSelected.Deactivate();
        }
        node.SetSelected_NoEffects();
        if (branch.MyParentBranch.MyBranchType != BranchType.HomeScreenUI)
        {
            branch.FromHotkey = true;
        }
        branch.MyParentBranch.LastHighlighted = node;
        branch.MyParentBranch.LastSelected = node;
        branch.UIMaster.LastSelected = node;
        branch.MyParentBranch.SaveLastSelected(node);
        branch.MoveToNextLevel();
    }
}
